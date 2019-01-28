using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using HelperLib;

namespace SuperbowlSquares
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void GenerateNumbersButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Disable the button immediately to prevent accidental double click
            GenerateNumbersButton.IsEnabled = false;

            // Get Rick's random generator
            var generator = new NumberGenerator();

            // *** COLUMNS *** //

            // Get the random numbers from the generator for the COLUMNS
            generator.GetTenNumbers().ToList().ForEach(number =>
            {
                // Create a TextBlock to show the number.
                var tb = GenerateTextBlock($"{number}");

                // This sets the column the TextBlock will be placed in.
                Grid.SetColumn(tb, number);

                // Adds the TextBlock to the Grid
                SquaresGrid.Children.Add(tb);
            });

            // *** ROWS *** //

            // Get the random numbers from the generator for the ROWS
            generator.GetTenNumbers().ToList().ForEach(number =>
            {
                // Create a TextBlock to show the number.
                var tb = GenerateTextBlock($"{number}");

                // This sets the column the TextBlock will be placed in.
                Grid.SetRow(tb, number);

                // Adds the TextBlock to the Grid
                SquaresGrid.Children.Add(tb);
            });

            // Hide the number generation button
            GenerateNumbersButton.Visibility = Visibility.Collapsed;

            // show the image generation button
            GenerateImageButton.Visibility = Visibility.Visible;
        }

        private TextBlock GenerateTextBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private async void GenerateImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BusyIndicator.IsActive = true;

                // *** Image Rendering *** //
                // Render the UI pixels to PNG pixels
                var rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(SquaresGrid);

                var pixelBuffer = await rtb.GetPixelsAsync();
                var pixels = pixelBuffer.ToArray();
                var displayInformation = DisplayInformation.GetForCurrentView();

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"superbowl_squares" + ".png", CreationCollisionOption.GenerateUniqueName);

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        (uint)rtb.PixelWidth,
                        (uint)rtb.PixelHeight,
                        displayInformation.RawDpiX,
                        displayInformation.RawDpiY,
                        pixels);

                    await encoder.FlushAsync();
                }

                // *** Dialog to ask user to email image *** //
                var md = new MessageDialog("Would you like to send an email to Julie?", "Image Generated");
                md.Commands.Add(new UICommand("YES"));
                md.Commands.Add(new UICommand("NO"));

                var result = await md.ShowAsync();

                // *** Send email *** //
                if (result.Label == "YES")
                {
                    var emailMessage = new EmailMessage();
                    emailMessage.Subject = "Superbowl Squares Generator Results";
                    emailMessage.Body = "The Superbowl generator results were rendered as a png image. find that image attached. Have any problems or questions, contact Lance or Rick.";

                    var emailRecipient = new EmailRecipient("lance.mccarthy@progress.com");
                    emailMessage.To.Add(emailRecipient);

                    // attach the image
                    var attachment = new EmailAttachment();
                    attachment.FileName = file.Name;
                    attachment.Data = file;

                    emailMessage.Attachments.Add(attachment);

                    await EmailManager.ShowComposeNewEmailAsync(emailMessage);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went horribly wrong, here's the error: {ex}", "Error").ShowAsync();
            }
            finally
            {
                BusyIndicator.IsActive = false;
            }
        }
    }
}
