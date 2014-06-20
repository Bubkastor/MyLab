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



namespace MyLab
{
    public partial class Form1 : Form
    {
        private string[] files_in_folder;
        private List<string> JPEGFiles = new List<string>();
        private List<Image> image = new List<Image>();
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;

            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.WorkerSupportsCancellation = true;

            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.WorkerSupportsCancellation = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((backgroundWorker1.IsBusy != true) || (backgroundWorker2.IsBusy != true) || (backgroundWorker3.IsBusy != true))
            {
                textBox1.Text = String.Empty;
                folderBrowserDialog1.ShowDialog();
                textBox1.AppendText(folderBrowserDialog1.SelectedPath);
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if ((backgroundWorker1.IsBusy != true) || (backgroundWorker2.IsBusy != true) || (backgroundWorker3.IsBusy != true))
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        //Поиск
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Regex reg = new Regex(@"\.(?i:)(?:jpg|jpeg|JPG|JPEG)$");
            

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

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarSearch.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (JPEGFiles.Count != 0)
            {
                backgroundWorker2.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Нет jpg файлов в директории.");
            }
        }

        //Чтение
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            int count_jpgs = JPEGFiles.Count;
            for (int n = 0; n < count_jpgs; n++)
            {
                image.Add(Image.FromFile(JPEGFiles[n]));
                int i = n;
                //ImageStateObject state = new ImageStateObject();
                //state.imageNum = n;
                //state.path_of_file = JPEGFiles[i];
                //FileStream fs = new FileStream(JPEGFiles[i], FileMode.Open, FileAccess.Read, FileShare.Read, 1, true);
                //state.filestm = fs;
                //int size = (int)fs.Length;
                //byte[] data = new byte[size + 1];
                //fs.BeginRead(data, 0, size,null,state);
                //fs.BeginRead(data, 0, size, readImageCallback, state);
                worker.ReportProgress((n + 1) * 100 / count_jpgs);
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarReadFiles.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker3.RunWorkerAsync();
        }

        //Обработка
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            int count_jpgs = image.Count;
            for (int i = 0; i <= count_jpgs; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    image[i].Save(Path.GetDirectoryName(JPEGFiles[i]) + @"\" + Path.GetFileNameWithoutExtension(JPEGFiles[i])+ ".png", ImageFormat.Png);
                    image[i].Dispose();
                    worker.ReportProgress((i + 1) * 100 / count_jpgs);
                }
            }
        }

        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarConvertation.Value = e.ProgressPercentage;
        }

        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Обработка завершена");
        }
    }
}
