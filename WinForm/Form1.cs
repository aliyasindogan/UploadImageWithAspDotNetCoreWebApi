using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;

namespace WinForm
{
    public partial class Form1 : Form
    {
        #region Form1

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataGridViewFill();

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

        #endregion Form1

        #region UploadImage

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
                        DataGridViewFill();
                        MessageBox.Show("Resim gönderildi!");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Resim gönderilemedi!");
                }
            }
        }

        #endregion UploadImage

        #region DataGridView

        private void DataGridViewFill()
        {
            var directory = VisualStudioProvider.TryGetSolutionDirectoryInfo();
            List<AddFileInfo> addFileInfoList = new List<AddFileInfo>();
            string[] files = Directory.GetFiles(directory.FullName + @"\WebAPI\wwwroot\Upload");
            foreach (string file in files)
            {
                if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png"))
                {
                    string[] data = file.Split('\\');
                    string fileName = "";
                    foreach (var item in data.ToList())
                    {
                        if (item.EndsWith(".jpg") || item.EndsWith(".jpeg") || item.EndsWith(".png"))
                        {
                            fileName = item;
                            break;
                        }
                    }
                    addFileInfoList.Add(new AddFileInfo()
                    {
                        FileName = fileName,
                        FileUrl = file,
                        Picture = Image.FromFile(file)
                    });
                }
            }
            if (addFileInfoList.Count > 0)
            {
                dataGridView1.DataSource = addFileInfoList;
                dataGridView1.Columns[1].Visible = false;
                for (int i = 0; i < addFileInfoList.Count; i++)
                {
                    DataGridViewColumn column = dataGridView1.Columns[2];
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    ((DataGridViewImageColumn)dataGridView1.Columns[2]).ImageLayout = DataGridViewImageCellLayout.Stretch;
                    dataGridView1.Rows[i].Height = 50;
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string selectedImagePath = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
            pictureBox1.ImageLocation = selectedImagePath;
        }

        #endregion DataGridView
    }
}