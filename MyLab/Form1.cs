using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Threading;


namespace MyLab
{
    public partial class Form1 : Form
    {
        private delegate void ReadImage();
        private delegate void ProgressBar2();
        private delegate void ProgressBar3();

        private string[] files_in_folder;
        private List<string> JPEGFiles = new List<string>();

        private int progresBar2 = 0;
        private int progresBar3 = 0;

        private void ProgressBar2Changed()
        {
            progressBarReadFiles.Value = (progresBar2 + 1) * 100 / JPEGFiles.Count;
            progresBar2 += 1;
        }
        private void ProgressBar3Changed()
        {
            progressBarConvertation.Value = (progresBar3 + 1) * 100 / JPEGFiles.Count;
            if (progressBarConvertation.Value == 100)
            {
                MessageBox.Show("Конвертация завершена.");
            }
            progresBar3 += 1;
        }

        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                textBox1.Text = String.Empty;
                folderBrowserDialog1.ShowDialog();
                textBox1.AppendText(folderBrowserDialog1.SelectedPath);
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        //Поиск
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Regex reg = new Regex(@"\.(?:jpg|jpeg|JPG|JPEG)$");
            

            int files_count_jpgs_folder = 0;
            files_in_folder = Directory.GetFiles(textBox1.Text, "*.*", SearchOption.AllDirectories);
            files_count_jpgs_folder = files_in_folder.Length;

            for (int i = 0; i < files_count_jpgs_folder; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    int percentage = (i + 1) * 100 / files_count_jpgs_folder;
                    if (reg.IsMatch(files_in_folder[i]))
                    {
                        JPEGFiles.Add(files_in_folder[i]);
                    }   
                    worker.ReportProgress(percentage);
                }
            }
            if (JPEGFiles.Count != 0)
            {
                AsyncCallback readImageCallback = new AsyncCallback(ReadImageCallback);
                int count_jpgs = JPEGFiles.Count;
                for (int n = 0; n < count_jpgs; n++)
                {
                    int i = n;
                    ImageObject imageObj = new ImageObject();
                    imageObj.path_of_file = JPEGFiles[i];
                    FileStream fs = new FileStream(JPEGFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read, 1, true);
                    imageObj.filestream = fs;
                    int size = (int)fs.Length;
                    byte[] data = new byte[size + 1];
                    fs.BeginRead(data, 0, size, readImageCallback, imageObj);
                }
            }
            else
            {
                MessageBox.Show("Нет jpg файлов в директории.");
            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarSearch.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        public class ImageObject
        {
            public FileStream filestream;
            public string path_of_file;
        }

        private void Thread(object jpgFile)
        {
            ImageObject file = (ImageObject)jpgFile;
            FileStream jpg_file = file.filestream;
            String outputPath = Path.GetDirectoryName(file.path_of_file) + @"\" + 
                Path.GetFileNameWithoutExtension(file.path_of_file) + ".png";
            new Bitmap(jpg_file).Save(outputPath, ImageFormat.Png);

            this.Invoke(new ProgressBar3(this.ProgressBar3Changed));
        }
        public void ReadJPGFiles()
        {

        }

        public void ReadImageCallback(IAsyncResult asyncResult)
        {
            ImageObject image = (ImageObject)asyncResult.AsyncState;
            image.filestream.EndRead(asyncResult);
            this.Invoke(new ProgressBar2(this.ProgressBar2Changed));

            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread(image);
            });
        }
    }
}
