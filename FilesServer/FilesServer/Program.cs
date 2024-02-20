using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NAudio.Wave;
using System.Runtime.Serialization;
using System.Threading;

namespace FilesServer
{
    class Program
    {
        static Socket socket = null;
        static TcpListener myListener = null;
        static ASCIIEncoding asciiEnc = new ASCIIEncoding();

        //private Comunicator com=null;
        static void Main(string[] args)
        {
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            myListener = new TcpListener(ipAddr, 8001);
            myListener.Start();
            //com = new Comunicator();
            while (true)
            {
                try
                {
                   
                    Console.WriteLine("the server is running at port:" + myListener.LocalEndpoint);
                    Console.WriteLine("waiting for a connection");
                    socket = myListener.AcceptSocket();
                    Console.WriteLine("connection accepted");
                    byte[] binDataIn = new byte[1024];
                    Console.WriteLine("the message frome client side");
                    while (true)
                    {
                        int k = socket.Receive(binDataIn);
                        string[] msgDetails = asciiEnc.GetString(binDataIn, 0, k).Split('$');
                        
                        switch (msgDetails[0])
                        {
                            case "1":
                                HandleWavStream(msgDetails[1]);
                                break;
                            case "2":
                                HandleGallery(msgDetails[1]);
                                break;
                        }

                        

                    }

                    //socket.Close();
                }
                catch (Exception e)
                {
                    
                    continue;
                }
                
            }
        }

        private static void HandleGallery(string cmd)
        {
            string[] fileEntries = null;
            int i = 0;
            while (cmd != "done")
            {
                byte[] binDataIn = new byte[255];
                
               
               switch (cmd)
                {
                    case "names":
                        fileEntries = Directory.GetFiles("img");
                        
                        string songsname = String.Join("$", fileEntries.Select(x => x.Remove(x.IndexOf("img\\"), "img\\".Length)).ToArray());
                        Console.WriteLine();

                        byte[] bufferOut = asciiEnc.GetBytes(songsname);
                        socket.Send(bufferOut, 0, bufferOut.Length, SocketFlags.None);
                        break;
                    case "getnext":
                        Thread.Sleep(100);
                        if (i == fileEntries.Length)
                            bufferOut = new byte[1];
                        else
                            bufferOut = ImageToBytes(fileEntries[i++]);
                        socket.Send(bufferOut, 0, bufferOut.Length, SocketFlags.None);
                        break;

                }
                int k = socket.Receive(binDataIn);
                cmd = asciiEnc.GetString(binDataIn, 0, k);
            }
        }

        private static byte[] ImageToBytes(string imgname)
        {
            byte[] imgdata = System.IO.File.ReadAllBytes(imgname);
            return imgdata;
        }

        private static void HandleWavStream(string msg)
        {
            byte[] binDataIn = new byte[1024];
            string status = "";
            System.Threading.Thread.Sleep(500);
            string filePath = @msg;
            string outPath = @"tempmydir\";
            DirectoryInfo di = Directory.CreateDirectory(outPath);
            Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(outPath));
            int segements = SpliToSegemnts(filePath, outPath);
            byte[] binOut = asciiEnc.GetBytes(segements.ToString());
            socket.Send(binOut);
            binOut = asciiEnc.GetBytes(string.Format("strat#{0}", segements));
            socket.Send(binOut);
            System.Threading.Thread.Sleep(200);
            for (int i = 1; i <= segements; i++)
            {
                string segemntPath = string.Format("{0}{1}.wav", outPath, i);
                using (FileStream fs = new FileStream(segemntPath, FileMode.Open, FileAccess.Read))
                {
                    int length = Convert.ToInt32(fs.Length);
                    binOut = new byte[length];
                    fs.Read(binOut, 0, length);
                    fs.Close();
                    socket.Send(binOut);
                    binOut = asciiEnc.GetBytes("end");
                    System.Threading.Thread.Sleep(1500);
                    socket.Send(binOut);
                    int x = socket.Receive(binDataIn);
                    status = asciiEnc.GetString(binDataIn, 0, x);
                    Console.WriteLine(status);
                }
            }
            string[] files = Directory.GetFiles(@"tempmydir");
            foreach (string file in files)
                File.Delete(file);
        }



        public static int SpliToSegemnts(string filePath, string outPath)
        {

            using (var wfr = new WaveFileReader(filePath))
            {
               
                TimeSpan totalTime = wfr.TotalTime;
                var extra = totalTime.TotalMilliseconds % 1000;
                double num = totalTime.TotalMilliseconds;
                Console.WriteLine(num);
                int count = 2; int segment = 1;
                double start = 0, end = count * 1000;
                while (end < num)
                {
                    WavFileUtils.TrimWavFile(filePath, outPath, TimeSpan.FromMilliseconds(start), TimeSpan.FromMilliseconds(end), segment);
                    start = end;
                    count += 2;
                    end = count * 1000;
                    segment++;
                }
                WavFileUtils.TrimWavFile(filePath, outPath, TimeSpan.FromMilliseconds(start), TimeSpan.FromMilliseconds(num), segment);
                return segment;
            }
        }
        public static void PrintArr(byte[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                Console.Write(arr[i]);
            }
        }
        
    }
}
