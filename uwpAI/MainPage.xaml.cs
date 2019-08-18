using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using uwpAI.Classes;
using uwpAI.Dialogs;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace uwpAI {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage: Page {

        public MainPage() {
            InitializeComponent();
        }

        private async void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();

            if (file != null) {

                var bitmap = new BitmapImage();

                bitmap.SetSource(await file.OpenReadAsync());

                selectedImage.Source = bitmap;

                byte[] imgs = await SerializerAsync(file);

                MakePrediction(imgs);
            }

        }

        private async void MakePrediction(byte[] imgs) {

            var url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/f1211596-1e54-4660-b94b-716d787d5ab3/classify/iterations/uwpLandmarks/image";
            var prediction_key = "PASTE YOU KEY HERE";
            var content_type = "application/octet-stream";

            using (var client = new HttpClient()) {

                client.DefaultRequestHeaders.Add("Prediction-Key", prediction_key);

                using (var content = new ByteArrayContent(imgs)) {

                    content.Headers.ContentType = new MediaTypeHeaderValue(content_type);
                    var response = await client.PostAsync(url, content);

                    var stringResponse = await response.Content.ReadAsStringAsync();

                    var code = JsonConvert.DeserializeObject<dynamic>(stringResponse).code;

                    if (code == "BadRequestImageFormat") {

                        TagName.Text = "There was an error with the request";
                    }
                    else {
                        IList<Prediction> predictions = JsonConvert.DeserializeObject<CustomVision>(stringResponse).predictions;
                        var date = JsonConvert.DeserializeObject<CustomVision>(stringResponse).created;

                        foreach (var item in predictions) {

                            if (predictions[0].probability > 0.7) {
                                TagName.Text = $"{predictions[0].tagName}";
                                Probability.Text = predictions[0].probability.ToString("P");
                            }
                            else {
                                TagName.Text = "I do not recognize the picture";
                                Probability.Text = "";
                            }
                        }
                    }
                }
            }
        }

        private void Reset(TextBlock textBlock) {

            textBlock.Text = "";
        }

        private async Task<byte[]> SerializerAsync(StorageFile file) {

            using (var inputStream = await file.OpenSequentialReadAsync()) {

                var readStream = inputStream.AsStreamForRead();
                var byteArray = new byte[readStream.Length];
                await readStream.ReadAsync(byteArray, 0, byteArray.Length);

                return byteArray;
            }
        }

        private async void clickItem_Async(object sender, TappedRoutedEventArgs e) {

            var dialog = new CreditsDialog();

            await dialog.ShowAsync();
        }
    }
}
