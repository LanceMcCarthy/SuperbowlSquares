using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using SBSquaresLibrary;

namespace SuperbowlSquares
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void GenerateNumbersButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Disable the button immediately to prevent accidental double click
            GenerateNumbersButton.IsEnabled = false;

            var generator = new RandomAxis();

            // *** COLUMNS *** //

            var columns = generator.GenerateAxis().ToList();

            // Get the random numbers from the generator for the COLUMNS
            for (int i = 0; i <= columns.Count - 1; i++)
            {
                // Create a TextBlock to show the number.
                var tb = GenerateTextBlock($"{columns[i]}");

                // This sets the column the TextBlock will be placed in.
                Grid.SetColumn(tb, i + 1);

                // Adds the TextBlock to the Grid
                SquaresGrid.Children.Add(tb);
            }
            
            // *** ROWS *** //

            var rows = generator.GenerateAxis().ToList();

            // Get the random numbers from the generator for the ROWS
            for (int i = 0; i <= rows.Count - 1; i++)
            {
                // Create a TextBlock to show the number.
                var tb = GenerateTextBlock($"{rows[i]}");

                // This sets the column the TextBlock will be placed in.
                Grid.SetRow(tb, i + 1);

                // Adds the TextBlock to the Grid
                SquaresGrid.Children.Add(tb);
            }
            
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
               var file =  await RenderAsync();

                // *** Dialog to ask user to email image *** //
                var md = new MessageDialog("What would you like to do with the rendered image?", "Image Generated");
                md.Commands.Add(new UICommand("Save As"));
                md.Commands.Add(new UICommand("Email"));
                md.Commands.Add(new UICommand("Copy to Clipboard"));

                var result = await md.ShowAsync();

                // *** Send email *** //
                if (result.Label == "Email")
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
                else if (result.Label == "Save As")
                {
                    var savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    savePicker.FileTypeChoices.Add("PNG Image", new List<string> { ".png" });
                    savePicker.SuggestedFileName = "Superbowl Squares 2019";

                    var saveAsFile = await savePicker.PickSaveFileAsync();
                    
                    await file.CopyAndReplaceAsync(saveAsFile);

                    CachedFileManager.DeferUpdates(file);

                    var status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == FileUpdateStatus.Complete)
                    {
                        var dataPackage = new DataPackage();
                        dataPackage.RequestedOperation = DataPackageOperation.Copy;
                        dataPackage.SetText(saveAsFile.Path);
                        Clipboard.SetContent(dataPackage);

                        await new MessageDialog($"The image has been saved, the file's path has been copied into your clipboard.").ShowAsync();
                    }
                }
                else if (result.Label == "Copy to Clipboard")
                {
                    var dataPackage = new DataPackage();
                    dataPackage.RequestedOperation = DataPackageOperation.Copy;
                    dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(file));

                    Clipboard.SetContent(dataPackage);

                    await new MessageDialog($"The bitmap has been copied to your clipboard. You can paste this directly into an image editor, email or other documents.").ShowAsync();
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

        public async Task<StorageFile> RenderAsync()
        {
            var rtb = new RenderTargetBitmap();

            await rtb.RenderAsync(SquaresGrid);

            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var displayInformation = DisplayInformation.GetForCurrentView();

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("superbowlsquares.png", CreationCollisionOption.ReplaceExisting);

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

            return file;
        }
    }
}