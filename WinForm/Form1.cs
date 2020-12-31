using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnUploadImage_ClickAsync(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            //Çoklu ve Tekli Gönderim Resim Yükleme
            //Multiple and Single Send Image Upload
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    foreach (String file in openFileDialog.FileNames)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        byte[] fileContents = File.ReadAllBytes(fileInfo.FullName);
                        MultipartFormDataContent multiPartContent = new MultipartFormDataContent();
                        ByteArrayContent byteArrayContent = new ByteArrayContent(fileContents);
                        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
                        multiPartContent.Add(byteArrayContent, "\"files\"", string.Format("\"{0}\"", fileInfo.Name));
                        string imgExtension = fileInfo.Extension;
                        HttpResponseMessage response = await httpClient.PostAsync(txtWebAPIBaseAddress.Text, multiPartContent);
                        string data = await response.Content.ReadAsStringAsync();
                        ListBoxFill();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListBoxFill();

            // Set the file dialog to filter for graphics files.
            // Dosya iletişim kutusunu grafik dosyalarını filtrelemek için ayarlayın.
            this.openFileDialog.Filter =
                //"Images (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;.PNG|" +
                "All files (*.*)|*.*";

            // Allow the user to select multiple images.
            // Kullanıcının birden çok görüntü seçmesine izin verin.
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Title = "Yüklenecek dosyalara göz atın.";
        }

        private void ListBoxFill()
        {
            listBox1.Items.Clear();
            var directory = VisualStudioProvider.TryGetSolutionDirectoryInfo();
            string[] files = Directory.GetFiles(directory.FullName + @"\WebAPI\wwwroot\Upload");
            foreach (string file in files)
            {
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png"))
                    listBox1.Items.Add(file);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string imagePath = listBox1.SelectedItem.ToString();
            pictureBox1.ImageLocation = imagePath;
        }

        public static class VisualStudioProvider
        {
            public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
            {
                var directory = new DirectoryInfo(
                    currentPath ?? Directory.GetCurrentDirectory());
                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }
                return directory;
            }
        }
    }
}