using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace MediaPlayerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int counter=0;
        private ASCIIEncoding asciiEnc = new ASCIIEncoding();
        private Queue<List<byte>> segmentsQueue = new Queue<List<byte>>();
        //private System.Media.SoundPlayer soundPlayer;
        private bool finish = false;
        //BackgroundWorker worker = new BackgroundWorker();
        private bool mediaPlayerIsPlaying = false;
        //private bool userIsDraggingSlider = false;
        private Socket socket = null;
        private IPAddress hostAddress = null;
        IPEndPoint hostEndPoint = null;
        SecondWindow window;
        private bool IsPause = true;
        private ParameterizedThreadStart downloadThreadStart = null;
        private Thread downloadThread = null;
        private ThreadStart playThreadStart = null;
        private Thread playThread = null;
        private bool Isplaying = false;
        //private bool CanContinue = true;
        private int next = 0;
        //private TcpListener listener = null;
        public MainWindow()
        {
            InitializeComponent();

            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(1);
            //timer.Tick += timer_Tick;
            //timer.Start();
            

            try
            {
                hostAddress = IPAddress.Parse("127.0.0.1");
                hostEndPoint = new IPEndPoint(hostAddress, 8001);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(hostEndPoint);
                window = new SecondWindow(socket);

                //Console.WriteLine("Conneting...");

            }
            catch (Exception e)
            {
                socket.Close();
                //Console.WriteLine("Error..." + e.StackTrace);
            }

        }
        private void OnSecondWindowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (window == null || window.IsClosed)
            {
                window = new SecondWindow(socket);
            }
            window.WindowState = WindowState.Maximized;
            window.Show();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            //if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            //{
            //    sliProgress.Minimum = 0;
            //    sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
            //    sliProgress.Value = mePlayer.Position.TotalSeconds;
            //}
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    mePlayer.Source = new Uri(openFileDialog.FileName);

            //}
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
            //Application.Current.Shutdown();
        }
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
            e.CanExecute = true;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            //userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //userIsDraggingSlider = false;
            //mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //    lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void DownloadAsync(Object obj)
        {
            using (Socket socket = obj as Socket)
            {
                object sender = new object();
                RoutedEventArgs e = new RoutedEventArgs();
                byte[] binDataOut;
                string str = window.GetSongName(sender, e);
                binDataOut = asciiEnc.GetBytes("1$" + str);
                socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
                byte[] binDataIn = new byte[255];
                int k = socket.Receive(binDataIn);
                int songLength = Int32.Parse(asciiEnc.GetString(binDataIn, 0, k));
                binDataIn = new byte[255];
                List<byte> lsbytes = new List<byte>();
                Thread.Sleep(150);
                k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
                string info = asciiEnc.GetString(binDataIn, 0, k);
                int segments = int.Parse(info.Split('#')[1]);
                bool stop = false;
                for (int i = 0; i < segments && !stop; i++)
                {
                    if (next > 1)
                    {
                        next = 1;
                        finish = true;
                        //CanContinue = false;
                        //return;
                    }
                    k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
                    while (k >= 255)
                    {
                        lsbytes.AddRange(binDataIn);
                        k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
                    }
                    while (k != 3)
                    {
                        lsbytes.AddRange(binDataIn);
                        k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
                    }


                    segmentsQueue.Enqueue(new List<byte>(lsbytes));
                    //Console.WriteLine(lsbytes.ToArray().Length);
                    lsbytes.Clear();
                    string status= "end of segment " + (i + 1) + " from " + segments;
                    byte [] binData = asciiEnc.GetBytes(status);
                    socket.Send(binData);
                    //Console.WriteLine("end of segment " + (i + 1) + " from " + segments);
                }
            }

            finish = false;
        }

        private void Play()
        {
            counter = 0;
            while (!finish)
            {
                while (segmentsQueue.Count > 0)
                {
                    if (IsPause)
                    {
                        List<byte> lsbytes = segmentsQueue.Dequeue();
                        using (var ms = new MemoryStream(lsbytes.ToArray()))
                        {
                            System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer(ms);
                            if (!finish) soundPlayer.PlaySync(); //can also use soundPlayer.PlaySync()
                        }
                        counter++;
                        Debug.WriteLine("end playing of segment "+ counter);
                    }
                }
                finish = false;
                counter = 0;
            }
        }
        private void DoPause(object sender, RoutedEventArgs e)
        {
            IsPause = !IsPause;
        }
        //private void Playlist(object sender, RoutedEventArgs e)
        //{
        //    playall = true;
        //}
        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (window.GetSongName(sender, e) == "empty")
        //        {
        //            return;
        //        }
        //        if (Isplaying)
        //        {
        //            socket.Close();
        //            //socket.Shutdown(SocketShutdown.Send);
        //            downloadThread.Abort();
        //            playThread.Abort();
        //        }
        //        next++;
        //        Isplaying = true;
        //        IsPause = true;
        //        //if (socket.Connected)
        //        //{
        //        //    socket.Disconnect(true);
        //        //}
        //        //if (threadflag)
        //        //{
        //        //    playThread.Interrupt();
        //        //    downloadThread.Interrupt();
        //        //}
        //        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        socket.Connect(hostEndPoint);
        //        //if (socket.Connected) Console.WriteLine("Connected");
        //        if (next <= 1)
        //        {
        //            downloadThreadStart = new ParameterizedThreadStart(DownloadAsync);
        //            downloadThread = new Thread(downloadThreadStart);
        //        }

        //        downloadThread.Start(socket);
        //        playThreadStart = new ThreadStart(Play);
        //        if (next > 1)
        //        {
        //            next = 1;
        //            downloadThreadStart = new ParameterizedThreadStart(DownloadAsync);
        //            downloadThread = new Thread(downloadThreadStart);
        //            playThreadStart = new ThreadStart(Play);
        //        }
        //        playThread = new Thread(playThreadStart);
        //        playThread.Start();
        //    }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
                //Trace.AutoFlush = true;
                //Trace.Indent();
                //Debug.WriteLine("blabla");
                //Trace.Unindent();
                counter = 0;
                next++;
                if (window.GetSongName(sender, e) == "empty")
                {
                    return;
                }
                if (Isplaying)
                {
                    socket.Close();
                    //socket.Shutdown(SocketShutdown.Send);
                    downloadThread.Abort();
                    playThread.Abort();
                }
                Isplaying = true;
                IsPause = true;
                //if (socket.Connected)
                //{
                //    socket.Disconnect(true);
                //}
                //if (threadflag)
                //{
                //    playThread.Interrupt();
                //    downloadThread.Interrupt();
                //}
                 //if (socket.Connected) Console.WriteLine("Connected");
                downloadThreadStart = new ParameterizedThreadStart(DownloadAsync);
                //if (CanContinue)
                //{
                downloadThread = new Thread(downloadThreadStart);
                downloadThread.Start(socket);
                playThreadStart = new ThreadStart(Play);
                playThread = new Thread(playThreadStart);
                playThread.Start();
                //}
                //CanContinue = true;
            }
            catch (Exception ex)
            {

            }
        }
        //private void Window_ContentRendered(object sender, RoutedEventArgs e)
        //{
        //    Button_Click(sender, e);
        //    BackgroundWorker worker = new BackgroundWorker();
        //    worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        //    worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        //    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        //    worker.WorkerReportsProgress = true;
        //    worker.RunWorkerAsync();
        //}
        //void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    //do the code when bgv completes its work
        //}

        //void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    int total = songLength;

        //    for (int i = 0; i <= total; i++) //some number (total)
        //    {
        //        if (worker.CancellationPending)
        //        {
        //            e.Cancel = true;
        //            break;
        //        }
        //        int percents = (i * 100) / total;
        //        (sender as BackgroundWorker).ReportProgress(percents, i);
        //        System.Threading.Thread.Sleep(100);

        //    }
        //}

        //void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    pbStatus.Value = e.ProgressPercentage;
        //}
        //void worker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    //int total = songLength;
        //    for (int i = 0; i < (int)e.Argument; i++)
        //    {
        //        if (worker.CancellationPending)
        //        {
        //            e.Cancel = false;
        //            break;
        //        }
        //        //ProgressChangedEventArgs ee = new ProgressChangedEventArgs(i / 100, pbStatus);
        //        //worker_ProgressChanged(sender, ee);
        //        worker.ReportProgress((i * (int)e.Argument) / 100);
        //        Thread.Sleep(10);
        //    }
        //}
        //void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    pbStatus.Value = e.ProgressPercentage;
        //    //Parameters parameter = e.UserState as Parameters;
        //    //txtInterationCounter.Text = e.ProgressPercentage.ToString() + " %";
        //}
        //void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //        MessageBox.Show("BackgroundWorker canceled");
        //    }
        //    else
        //    {
        //        MessageBox.Show("BackgroundWorker ended successfully");
        //    }
        //}
    }
}