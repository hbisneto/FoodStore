using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoodStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DLCWindow : Page
    {
        public DLCWindow()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string fileUrl = "https://dl.dropboxusercontent.com/scl/fi/ognxe7tfif09fmj04z4o9/Database.db?rlkey=bpomf7i7ckmulv1dg4oymzm94&st=kwc5x6nn&dl=0";
        //https://www.dropbox.com/scl/fi/ognxe7tfif09fmj04z4o9/Database.db?rlkey=bpomf7i7ckmulv1dg4oymzm94&st=kwc5x6nn&dl=0
            string fileName = "Database.db";
            ProgressBar welcomeProgress = progbar_DownloadProgress;

            await DownloadFileAsync(fileUrl, fileName, welcomeProgress);
        }

        public async Task DownloadFileAsync(string fileUrl, string fileName, ProgressBar progressBar)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var progressHandler = new Progress<Windows.Web.Http.HttpProgress>(progressReport =>
                    {
                        if (progressReport.TotalBytesToReceive.HasValue)
                        {
                            double totalBytesToReceive = (double)progressReport.TotalBytesToReceive.Value;
                            progressBar.Value = progressReport.BytesReceived * 100 / totalBytesToReceive;
                        }
                    });

                    HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength;
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                    using (var inputStream = await response.Content.ReadAsStreamAsync())
                    using (var outputStream = await file.OpenStreamForWriteAsync())
                    {
                        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await outputStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            if (totalBytes.HasValue)
                            {
                                double totalBytesDouble = (double)totalBytes.Value;
                                progressBar.Value = (double)totalBytesRead / totalBytesDouble * 100;
                            }
                            int progressConvert = Convert.ToInt32(progressBar.Value);
                            txtBlock_DownloadMessage.Text = $"Configurando ambiente: {progressConvert}%";
                        }
                    }
                    txtBlock_DownloadMessage.Text = "Seu ambiente foi configurado.";
                    Console.WriteLine("Arquivo baixado e salvo com sucesso.");
                    Frame.Navigate(typeof(AuthWindow));
                }
            }
            catch (Exception ex)
            {
                txtBlock_DownloadMessage.Text = $"Erro ao baixar o arquivo: {ex.Message}";
                Console.WriteLine($"Erro ao baixar o arquivo: {ex.Message}");
                try
                {
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    IStorageItem dbFile = await localFolder.TryGetItemAsync("Database.db");

                    if (dbFile != null)
                    {
                        await dbFile.DeleteAsync();
                    }
                }
                catch { }
            }
        }
    }
}
