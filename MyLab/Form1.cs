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
        private List<string> PNGFiles = new List<string>();
        public class ImageObject
        {
            public Image image { get; set; }
            public String oldPath { get; set; }
            public String newPath { get; set; }
            
        }
        private List<ImageObject> filesImage = new List<ImageObject>();
        private bool isCancled = false;

        private List<Task> task = new List<Task>();
        private int progresBar3 = 0;

        private static CancellationTokenSource cts2 = new CancellationTokenSource();
        private CancellationToken token2 = cts2.Token;

        private void ProgressBar3Changed()
        {
            progressBarConvertation.Value = (progresBar3 + 1) * 100 / PNGFiles.Count;
            if (progressBarConvertation.Value == 100)
            {
                MessageBox.Show("Конвертация завершена.");
            }
            progresBar3 += 1;
        }
        private void Complited()
        {
            task = new List<Task>(); ;
            filesImage = new List<ImageObject>(); ;
            PNGFiles = new List<string>();
            isCancled = false;
            progressBarSearch.Value = 0;
            progressBarConvertation.Value = 0;
            progresBar3 = 0;
            backgroundWorker1.WorkerSupportsCancellation = true;
            cts2 = new CancellationTokenSource();
            token2 = cts2.Token;
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
                Complited();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs b)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Regex reg = new Regex(@"\.(?:png|PNG)$");


            int files_count_jpgs_folder = 0;
            files_in_folder = Directory.GetFiles(textBox1.Text, "*.*", SearchOption.AllDirectories);
            files_count_jpgs_folder = files_in_folder.Length;

            for (int i = 0; i < files_count_jpgs_folder; i++)
            {
                if (worker.CancellationPending == true)
                {
                    b.Cancel = true;
                    break;
                }
                else
                {
                    int percentage = (i + 1) * 100 / files_count_jpgs_folder;
                    if (reg.IsMatch(files_in_folder[i]))
                    {
                        PNGFiles.Add(files_in_folder[i]);
                    }   
                    worker.ReportProgress(percentage);
                }
            }

            if (PNGFiles.Count != 0)
            {

                int count_jpgs = PNGFiles.Count;
                for (int n = 0; n < count_jpgs; n++)
                {
                    if (worker.CancellationPending == true)
                    {
                        b.Cancel = true;
                        break;
                    }
                    else
                    {
                        ImageObject temp = new ImageObject();
                        temp.image = Image.FromFile(PNGFiles[n]);
                        temp.oldPath = PNGFiles[n];
                        filesImage.Add(temp);

                    }
                }

                filesImage.ForEach(delegate(ImageObject obj)
                {
                    task.Add(Task.Factory.StartNew(() =>
                    {
                        if (token2.IsCancellationRequested)
                        {
                            token2.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            obj.newPath = Path.GetDirectoryName(obj.oldPath) + @"\" + Path.GetFileNameWithoutExtension(obj.oldPath) + ".jpg";
                            this.Invoke(new ProgressBar3(this.ProgressBar3Changed));
                            obj.image.Save(obj.newPath, ImageFormat.Jpeg);
                        }
                    }, token2));

                });

                try
                {
                    Task.WaitAll(task.ToArray());
                }
                catch { } 
                filesImage.ForEach(delegate(ImageObject obj)
                {
                    obj.image.Dispose();
                });
            }
            else
            {
                MessageBox.Show("Нет jpg файлов в директории.");
            }

        }
        private long GetSizeFile(String path)
        {
            var fileName = path;
            FileInfo fi = new FileInfo(fileName);
            return fi.Length;
        }

        private String SizeSave()
        {
            String result;
            long oldSize = 0;
            long newSize = 0;
            filesImage.ForEach(delegate(ImageObject obj)
            {
                oldSize += GetSizeFile(obj.oldPath);
                newSize += GetSizeFile(obj.newPath);
            });
            result = (oldSize - newSize).ToString();
            return result;
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarSearch.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!isCancled)
            {
                SaveBite.Text = SizeSave();
            }
            Complited();
            //C:\Users\Albert\Pictures\NewPNG
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
            }
            isCancled = true;
            cts2.Cancel();
            MessageBox.Show("Отменено.");
        }
    }
}
