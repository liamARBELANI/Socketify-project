using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MediaPlayerWPF
{
    /// <summary>
    /// Interaction logic for SecondWindow.xaml
    /// </summary>
    public partial class SecondWindow : Window
    {
        private Socket socket = null;
        private ASCIIEncoding asciiEnc = new ASCIIEncoding();
        string songname = "empty";
        public SecondWindow()
        {
            InitializeComponent();
        }

        public SecondWindow(Socket socket)
        {
            InitializeComponent();
            this.socket = socket;
            DownloadGallery();
        }

        private void DownloadGallery()
        {
            byte[] binDataOut;
            binDataOut = asciiEnc.GetBytes("2$names");
            socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
            byte[] binDataIn = new byte[255];
            int k = socket.Receive(binDataIn);
            string [] songNames = asciiEnc.GetString(binDataIn, 0, k).Split('$');
            songNames = songNames.Select(x => x.Replace("jpg", "wav")).ToArray();
            Console.WriteLine(songNames);

            List <ImageSource > lsImgs= DownloadAllImgs();
            CreateGridGrallery(songNames, lsImgs);
        }

        private void CreateGridGrallery(string[] songNames, List<ImageSource> lsImgs)
        {
            Grid grid = this.mygrid;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            int row = 0;
            for (int i = 0; i < songNames.Length; i+=2)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                int col = 0;
                for (int j = i; j < i+2; j++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    StackPanel sp = new StackPanel();
                    Image im = new Image();
                    im.Width = 100;
                    im.Height = 100;
                    im.Source = lsImgs[j];
                    Button b = new Button();
                    b.Content = im;
                    b.ToolTip = songNames[j];
                    b.Width = 150;
                    b.Height = 150;
                    b.Click += B_Click;
                    sp.Children.Add(b);

                    Label l = new Label();
                    l.Content = songNames[j];
                    
                    sp.Children.Add(l);
                    sp.Margin = new Thickness(10);
                    Grid.SetRow(sp, row);
                    Grid.SetColumn(sp, col++);

                    mygrid.Children.Add(sp);

                }
                row++;
                
                

            }
            
            
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            songname = (sender as Button).ToolTip.ToString();
            Close();
        }

        private List<ImageSource> DownloadAllImgs()
        {
            List<ImageSource> imgs = new List<ImageSource>();
            ImageSource src = null;
            byte[] binDataOut;
            do
            {
                byte[] binDataIn = new byte[255];
                binDataOut = asciiEnc.GetBytes("getnext");
                socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
                src = DownloadsingleImg();
                if (src != null)
                    imgs.Add(src);
                
            } while (src != null);

            binDataOut = asciiEnc.GetBytes("done");
            socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
            return imgs;

        }

        private ImageSource DownloadsingleImg()
        {
            try
            {
                List<byte> lsbytes = new List<byte>();
                Thread.Sleep(100);
                byte[] bufferIn = new byte[255];

                int k = socket.Receive(bufferIn, 0, 255, SocketFlags.None);
                //DONE!!
                if (k == 1)
                    return null;
                while (k >= 255)
                {
                    lsbytes.AddRange(bufferIn);
                    k = socket.Receive(bufferIn, 0, 255, SocketFlags.None);

                }

                byte[] temp = new byte[k];
                Array.Copy(bufferIn, temp, k);
                lsbytes.AddRange(temp);
                //myimage.Source = ToImage(lsbytes.ToArray());
                byte[] imgdata = lsbytes.ToArray();
                System.Drawing.Image tempImg = Helper.MyByteArrayToImage(imgdata);
                return Helper.MyToImageSource(tempImg);
            }
            catch (Exception)
            {

                return null;
            }
           
        }

        //public void VivalaVida(object sender, RoutedEventArgs e)
        //{
        //    songname = "Coldplay - Viva La Vida.wav";
        //    Close();
        //}
        //public void Stars(object sender, RoutedEventArgs e)
        //{
        //    songname = "Coldplay - a sky full of stars.wav";
        //    Close();
        //}
        //public void Bohemian(object sender, RoutedEventArgs e)
        //{
        //    songname = "Queen - Bohemian Rhapsody.wav";
        //    Close();
        //}
        //public void SevenYears(object sender, RoutedEventArgs e)
        //{
        //    songname = "Lukas Graham - 7 Years.wav";
        //    Close();
        //}
        //public void Melody(object sender, RoutedEventArgs e)
        //{
        //    songname = "original.wav";
        //    Close();
        //}
        //public void Radioactive(object sender, RoutedEventArgs ee)
        //{
        //    songname = "Imagine Dragons - Radioactive.wav";
        //    Close();
        //}

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.IsClosed = true;
        }

        public string GetSongName(object sender, RoutedEventArgs ee)
        {
            return songname;
        }
        public bool IsClosed { get; private set; }
    }
}