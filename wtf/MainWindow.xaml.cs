using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static wtf.UserDef;
using System.Configuration;
using InteractiveDataDisplay.WPF;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using FileFolder = Microsoft.WindowsAPICodePack;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Matlab;
using WebSocketSharp;
using WebSocketSharp.Server;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;

using MathNet.Numerics.IntegralTransforms;
using System.Collections;
using Neuronic.TimeFrequency;
using Neuronic.TimeFrequency.Kernels;
using Neuronic.TimeFrequency.Transforms;
using Neuronic.TimeFrequency.Wavelets;
using System.IO.MemoryMappedFiles;
using RabbitMQ.Client;
using System.Diagnostics;

namespace wtf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        [DllImport("Ws2_32.dll")]
        public static extern int inet_addr(string ipaddr);
        private const int MaxTryCount = 5;

        public delegate void MsgUpdateDelegate(String msg);

        private static BackgroundWorker backgroundWorker_2991;
        private static BackgroundWorker backGroundWorkerDisplay;
        //private static BackgroundWorker backgroundWorker_2991B;
        private ulong channelSelected = 0;
        private bool isSelectiongChanging = false;
        //SC300 motionPlatform = new SC300();
        MLApp.MLApp matlab = null;

        private WebSocketServer webSocketServer;

        ConnectionFactory connectionFactory = null;
        IConnection connection = null;
        IModel channel = null;

        public static Process p = null;
        private static List<List<Double>> parameters = new List<List<Double>>() {
            new List<double>(){-1.6114303731727691e-13, 8.995789942798905e-08, -0.017776192617190895, 1374.2099070543757 },
            new List<double>() { -7.710499011023721e-10, - 0.000401885042517353, 152.19366498580783 },
            new List<Double>() {1.6937729313837746e-09, -0.0014028515618654488, 253.99085582499248 },  //24.5
            new List<Double>() {5.2e-09, -0.0028057469, 394.63929418733466 }, //24.4
            new List<Double>() {4.1e-09, -0.0023502101, 347.382012835228 }, //24.3
            new List<Double>() {2.8e-09, -0.0018277928, 293.1887642205437  },//24.2
            new List<Double>() {2.5e-09, -0.0016901039, 278.921710740231 },//24.1
            new List<Double>() {1.7e-09, -0.0013592452, 244.3985840598463 },//24
            new List<Double>() {9e-10, -0.0010559414, 212.5323502410578 },//23.9
            new List<Double>() {-8e-10, -0.0003374776, 137.4752573783787 },//23.8
            new List<Double>() {-1.7e-09, 6.31297e-05, 95.44041392958543 },//23.7
            new List<Double>() {-2.3e-09, 0.0003124412, 69.22205280574725 },//23.6
            new List<Double>() {-2.4e-09, 0.0003349066, 66.65911661644182 }}; //23.5
        /* MemoryMappedFile memoryFile = null;
         MemoryMappedViewAccessor accessor1;
         long capacity = 1 << 10 << 10;*/
       
        //private static volatile List<List<Double>> dataToSave = new List<List<Double>>();
        public MainWindow()
        {
            InitializeComponent();

            //连接消息队列
            try
            {
                connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = "localhost";
                connectionFactory.UserName = "admin";
                connectionFactory.Password = "admin";
                connectionFactory.VirtualHost = "/";//默认情况可省略此行
                connectionFactory.Port = 5672;
                connection = connectionFactory.CreateConnection();
                channel = connection.CreateModel();
            }catch(Exception e)
            {

            }
            //channel.QueueDeclare("SwapScope", true, false, true, null);
            //创建共享内存
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("testMmf", capacity, MemoryMappedFileAccess.ReadWrite);
            
                //通过MemoryMappedFile的CreateViewAccssor方法获得共享内存的访问器
            ViewAccessor = memoryMappedFile.CreateViewAccessor(0, capacity);

            Type matlabAppType = System.Type.GetTypeFromProgID("Matlab.Application");
            matlab = System.Activator.CreateInstance(matlabAppType) as MLApp.MLApp;
            string path_project = System.IO.Directory.GetCurrentDirectory();
            string path_matlabwork = "cd('"+ path_project +"')";
            matlab.Execute(path_matlabwork);

            webSocketServer = new WebSocketServer("ws://192.168.171.1:8083");
            webSocketServer.AddWebSocketService<socketHander>("/");
            webSocketServer.Start();


            for (int i = 0; i < 16; i++)
            {
                CheckBox tmp = new CheckBox();
                
                tmp.Tag = i;
                tmp.Checked += Tmp_Checked;
                tmp.Unchecked += Tmp_Checked;
                tmp.Width = 80;
                tmp.Margin = new Thickness(5, 10, 5, 10);
                tmp.Content = "通道" + i;
             
                if (i < 8)
                {
                    upStackPannel.Children.Add(tmp);
                }
                else
                {
                    downStackPannel.Children.Add(tmp);
                }
                List<Double> y = new List<Double>();
                List<int> m = new List<int>();
                
                data.Add(y);
              
                peaks.Add(m);
            }
        
            heatPlotWindow.plt.Title("频谱图");
            heatPlotWindow.plt.XLabel("频率(Hz)");
            heatPlotWindow.plt.YLabel("幅值");
            heatPlotWindow.plt.Ticks(numericFormatStringX: "F2");

            wavePlotWindow.plt.Title("实时波形");
            wavePlotWindow.plt.XLabel("时间(s)");
            wavePlotWindow.plt.YLabel("通道");

            

        }

       

        private void Tmp_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox tmp = (CheckBox)sender;
            addMsg(tmp.Tag + (tmp.IsChecked == true ? "checked" : "unchecked"));
            isSelectiongChanging = true;
            if (tmp.IsChecked == true)
            {
                channelSelected |= ((ulong)0x01 << Convert.ToInt32(tmp.Tag));
            } else
            {
                channelSelected &= (~((ulong)0x01 << Convert.ToInt32(tmp.Tag)));
            }
        }



        private void addMsg(String s)
        {
            textbox.Dispatcher.Invoke(new Action(delegate
            {
                String st = textbox.Text + s + '\r';

                textbox.Text = st.Substring(st.Length > 20000 ? st.Length - 20000 : 0, st.Length > 20000 ? 20000 : st.Length);
                textbox.ScrollToEnd();
            }));

        }
        NET2991A nET2991A = new NET2991A();
        private bool is2991enabled = true;
        List<double> x = new List<double>();
        List<List<Double>> data = new List<List<Double>>();
        
        

        int[] lastDataCount = new int[16];
        List<List<int>> peaks = new List<List<int>>();
        bool needPause = false;
        volatile  bool isContinue = false;
        volatile bool isChangeData = false;
        int continueCount = 0;
        List<int> channelNum = new List<int>();
        static Color[] colors = { Colors.Red, Colors.Blue, Colors.Black, Colors.Yellow, Colors.Purple, Colors.Orange, Colors.Chocolate, Colors.OrangeRed };
        bool isWavletMethod = true;
        //流程流程数据
        //包括3维数据
        //第一维，第i次检测数据结果，由于无法精确采集开始时间与位置数据对应，因此每次的数据在物理位置上可能不是对齐的
        //第二维17列数据 ，第一列为时间数据，其余为16通道数据
        //第三维数据都是一维double数组
        // List<double[][]> testData;// new List<List<List<Double>>>();

        //读取.ZIP文件
        private void ReadZip(String filePath)
        {

            using (FileStream file = File.OpenRead(filePath))
            {

                using (GZipStream gz = new GZipStream(file, CompressionMode.Decompress))
                {
                    //先读channlenum

                    byte[] by = new byte[sizeof(int)];
                    int r = gz.Read(by, 0, by.Length);
                    if (r > 0)
                    {
                        int channleCount = System.BitConverter.ToInt32(by, 0);
                        byte[] channelBytes = new byte[channleCount * sizeof(int)];
                        int[] channeldata = new int[channleCount];
                        r = gz.Read(channelBytes, 0, channelBytes.Length);
                        Buffer.BlockCopy(channelBytes, 0, channeldata, 0, r);

                        //再读x
                        gz.Read(by, 0, by.Length);
                        int xcount = System.BitConverter.ToInt32(by, 0);
                        byte[] xdataBytes = new byte[xcount * sizeof(double)];
                        double[] xdata = new double[xcount];
                        r = gz.Read(xdataBytes, 0, xdataBytes.Length);
                        Buffer.BlockCopy(xdataBytes, 0, xdata, 0, r);
                        /*
                        x.Clear();

                        foreach (double value in xdata)
                        {
                            x.Add(value);
                        }
                        */
                        Exist_Data_Channel.Clear();
                        // data.Clear();
                        //再读y
                        for (int m = 0; m < FileData.Count; m++)
                        {
                            gz.Read(by, 0, by.Length);
                            int ycount = System.BitConverter.ToInt32(by, 0);
                            if (ycount > 0)
                            {
                                Exist_Data_Channel.Add(m); //当前的通道，对当前的通道进行数据读取
                                List<double> y = FileData.ElementAt(m);
                                y.Clear();
                                byte[] ydataBytes = new byte[ycount * sizeof(double)];
                                double[] ydata = new double[ycount];
                                r = gz.Read(ydataBytes, 0, ydataBytes.Length);
                                Buffer.BlockCopy(ydataBytes, 0, ydata, 0, r);

                                foreach (double v in ydata)
                                {
                                    y.Add(v);
                                }
                                if (r <= 0)
                                {
                                    break;
                                }

                            }

                        }

                    }

                }
            }
        }
        
      
       

        private System.Threading.Timer timer2;

        //计算统计量
        private void showStatistics(object o) {
           /* UserDef.timeOut = false;
            //String filePath =  "compute"+ DateTime.Now.ToString("MM-dd-H-mm-ss_")+".txt";
            for (int i = 0; i < channelNum.Count; i++) {
                List<Double> list = UserDef.dataCompute.ElementAt(channelNum.ElementAt(i));
                if (list.Count > 0)
                {
                    double average = list.Average(value => value);
                    double max = list.Max();
                    double min = list.Min();
                    double std = list.Average(value => Math.Pow(value - average, 2));
                    using (StreamWriter sw = new StreamWriter("I:\\电源测试数据\\compute.txt", true))
                    {
                        String s = ((decimal)average).ToString() + "," + ((decimal)max).ToString() + "," + ((decimal)min).ToString() + "," + ((decimal)std).ToString();
                        sw.WriteLine(s);

                    }
                    UserDef.dataCompute.ElementAt(channelNum.ElementAt(i)).Clear();
                }
            }

            UserDef.timeOut = true;
        */
        }


        private double temp_value = 0;
        
        private int count_index = 0;
        private bool csvSave = false;
        private void timeOver(object a) {
            UserDef.flagRecord = false;


            if (channelNum.Count != 0 && UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count != 0)
            {

                int count = channelNum.Count;
                int save_index = 0;


                double[,] matdata = new double[count + 1, UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count];
                if (UserDef.xToSave.Count == 0)
                {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {

                        matdata[save_index, i] = UserDef.NowRes;
                    }

                }

                else if (UserDef.xToSave.Count * (UserDef.Frequency / 2) >= UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count)
                {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {
                        int n = i / (UserDef.Frequency / 2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }

                }
                else
                {

                    for (int i = 0; i < UserDef.xToSave.Count * (UserDef.Frequency / 2); i++)
                    {
                        int n = i / (UserDef.Frequency / 2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }



                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count - UserDef.xToSave.Count * (UserDef.Frequency / 2); i++)
                    {
                        matdata[save_index, i + UserDef.xToSave.Count * (UserDef.Frequency / 2)] = UserDef.xToSave.ElementAt(UserDef.xToSave.Count - 1);

                    }


                }

                save_index++;

                //StreamWriter sw = new StreamWriter("I:\\电源测试数据\\TimeOut\\log.logs",true);
                String s = "";
                for (int i = 0; i < count; i++)
                {

                    List<Double> list = UserDef.dataToSave.ElementAt(channelNum.ElementAt(i));
                    if (list.Count > 0)
                    {
                        double aver = list.Average(value => value); //均值
                        double max = list.Max(); //最大值
                        double min = list.Min(); //最小值
                        double std = list.Average(value => Math.Pow((aver - value), 2)); //方差
                        //s += "通道:" + channelNum.ElementAt(i).ToString() + "均值:" + aver.ToString() + "方差:" + std.ToString() + "最大最小值:" + max.ToString() + "," + min.ToString() + "\n";
                        s += ((decimal)aver).ToString() + "," + ((decimal)std).ToString() + "," + ((decimal)max).ToString() + "," + ((decimal)min).ToString();
                    }



                    for (var j = 0; j < UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Count; j++)
                    {
                        matdata[save_index, j] = UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).ElementAt(j);
                    }

                    save_index++;
                }


                Console.WriteLine(csvSave);
                if (csvSave)
                {
                    StreamWriter swt = new StreamWriter("D:\\CSVData\\" + DateTime.Now.ToString("MM -dd-H-mm-ss_") + ".csv");
                    for (int i = 0; i < matdata.GetLength(1); i++)
                    {
                        for (int j = 0; j < matdata.GetLength(0); j++)
                        {

                            swt.Write(matdata[j, i].ToString() + ",");

                        }

                        swt.Write("\r\n");
                    }
                    swt.Flush();
                    swt.Close();
                }

                else
                {
                    Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matdata);
                    String path = "I:\\电源测试数据\\TimeOut\\" + DateTime.Now.ToString("MM-dd-H-mm-ss_") + ".mat";
                    MatlabWriter.Write(path, matrix, "data");
                }


                for (int i = 0; i < UserDef.dataToSave.Count; i++)
                {
                    UserDef.dataToSave.ElementAt(i).Clear();
                }
                UserDef.xToSave.Clear();
                using (StreamWriter sw = new StreamWriter("I:\\电源测试数据\\TimeOut\\log.logs", true))
                {
                    //s = DateTime.Now.ToString("MM-dd-H-mm-ss_,") + s;
                    sw.WriteLine(s);
                }
                count_index++;
            }

            UserDef.flagRecord = true;
        }



        private void backgroundWorker_2991_doWork(object sender, DoWorkEventArgs e)
        {
            
            try
            {
                
                int tryCount = 0;
                while (!nET2991A.InitDevice("192.168.0.145", "9000", "8000", new MsgUpdateDelegate(addMsg)))
                {
                    addMsg("初始化2991设备失败!");
                    if (!is2991enabled)
                    {
                        return;
                    }
                    Thread.Sleep(5000);
                }
                tryCount = 0;
                while (!nET2991A.start())
                {
                   /* tryCount++;
                    if (tryCount > MaxTryCount)
                    {
                        is2991enabled = false;
                        return;
                    }*/
                    if (!is2991enabled)
                    {
                        return;
                    }
                    addMsg("启动2991设备失败!");
                    Thread.Sleep(5000);
                }
                addMsg("启动2991设备成功!");
                ulong currentCount = 0;
                while (true)
                {
                   
                    int readStep = 100000;
                    int step = 1;
                    if (!is2991enabled)
                    {
                        return;
                    }
                    if (needPause)
                    {
                        continue;
                    }
                    // cb_continueLog.IsChecked == true;
                    //Console.WriteLine("current count end " + currentCount + "max count"+ NET2991A.currentFrameCount);
                    if (currentCount < NET2991A.currentFrameCount)
                    {
                        Console.WriteLine("working");
                        if (isContinue)
                        {
                            continueCount++;
                            if (continueCount > 1000)
                            {
                                needPause = true;
                                isContinue = false;
                                continueCount = 0;
                            }
                        }
                        else
                        {
                            continueCount = 0;
                        }
                        if (needPause)
                        {
                            continue;
                        }
                        isChangeData = true;
                        //清除当前数据，显示下一帧数据
                        if (!isContinue || continueCount < 0)
                        {
                            Console.WriteLine("clear");
                            x.Clear();
                            Parallel.For(0, 16, (i) =>
                            {
                                data.ElementAt(i).Clear();
                                lastDataCount[i] = 0;
                            });
                        }

                        int displayFrame = (int)(NET2991A.currentFrameCount + 1) % 2;
                        int count = 0;
                        if (isSelectiongChanging)
                        {
                            channelNum.Clear();
                            for (var i = 0; i < 16; i++)
                            {
                                if (((channelSelected >> i) & (ulong)0x01) > 0)
                                {
                                    channelNum.Add(i);
                                    lastDataCount[i] = data.ElementAt(i).Count;
                                   
                                }

                               
                            }

                            for (int k = 0; k < channelNum.Count; k++)
                            {
                                UserDef.freq.ElementAt(channelNum.ElementAt(k)).Clear();

                            }


                            isSelectiongChanging = false;


                        }
                        for (var i = 0; i < 16; i++)
                        {
                            lastDataCount[i] = data.ElementAt(i).Count;
                        }
                        
                        int yindex = 0;

                        double lastTimeEnd = 0;
                        if (x.Count > 0)
                        {
                            lastTimeEnd = x.ElementAt(x.Count - 1);
                        }
                        //Console.WriteLine("time end is" + lastTimeEnd);
                        // Console.WriteLine("data leng is " + data.ElementAt(0).Count);
                        //一直读取UserDef.Frequency个数据点，
                        //i < readStep && 
                        for (int i = 0;  count < UserDef.Frequency; i = i + step)
                        {
                            x.Add(  + ((i + 1) * 1.0 / UserDef.Frequency));
                            
                        
                            //Console.WriteLine(UserDef.NowRes);
                            yindex = 0;
                            //Parallel.For(0, channelNum.Count, (j) =>
                             foreach (int n in channelNum)
                            {
                               // int n = channelNum.ElementAt(j);
                                //List<Double> y = data.ElementAt(n);      //当前的通道                        
                                //displayFrame 是需要展示的数据帧
                                temp_value = ((NET2991A.buffer[displayFrame, n, i] - 32768) * NET2991A.fPerLsb / 1000);
                                // y.Add(temp_value + 0.3 * yindex);
                                //+0.3 * yindex
                                data.ElementAt(n).Add(temp_value);
                               /* if (UserDef.flagRecord && UserDef.NowRes != 0)
                                {

                                    UserDef.dataToSave.ElementAt(n).Add(temp_value);

                                }
                                if (UserDef.timeOut)
                                {
                                    UserDef.dataCompute.ElementAt(n).Add(temp_value);

                                }*/


                                yindex++;
                            }

                            

                            count++;
                        }
                     
                        
                        // yindex = 0;
                        isChangeData = false;
                        wavePlotWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //displayLines();
                            playSelectLines();
                            //Console.WriteLine("ShowOver");
                        }));



                        // Define the output 



                        currentCount = NET2991A.currentFrameCount;
                    }


                }

                

            }
            catch (Exception ex)
            {
                is2991enabled = false;
                addMsg("failed!" + ex.Message);
            }
        }


        private System.Threading.Timer timer1;
      
        
        private void Start2991_Click(object sender, RoutedEventArgs e)
        {

             


            if (start2991.Content.Equals("启动2991"))
            {
                clearData();
                is2991enabled = true;
                backgroundWorker_2991 = new BackgroundWorker();
                backgroundWorker_2991.DoWork += backgroundWorker_2991_doWork;
                backgroundWorker_2991.RunWorkerCompleted += BackgroundWorker_2991_RunWorkerCompleted;
                backgroundWorker_2991.WorkerSupportsCancellation = true;
                backgroundWorker_2991.RunWorkerAsync();
                start2991.Content = "停止2991";
            } else
            {
                is2991enabled = false;
                backgroundWorker_2991.CancelAsync();
                backgroundWorker_2991 = null;
                nET2991A.stopDevice();
                start2991.Content = "启动2991";
                start2991.IsEnabled = false;
                if (timer1 != null) {
                    UserDef.flagRecord = false;
                    UserDef.NowRes = 0;
                    timer1.Dispose();
                }
                if (timer2 != null) {
                    UserDef.timeOut = false;
                    timer2.Dispose();
                }
                if(!timeOutSave.IsEnabled)
                timeOutSave.IsEnabled = true;
                changeCondition();
            }
           
        }
        private List<int> Exist_Data_Channel = new List<int>();
        private List<List<Double>> FileData = new List<List<double>>();
       
        
        private void BackgroundWorker_2991_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("complietetd");
            start2991.IsEnabled = true;
            start2991.Content = is2991enabled ? "停止2991" : "启动2991";
        }

        

        private void cb_save_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void bt_pause_Click(object sender, RoutedEventArgs e)
        {
            if(bt_pause.Content.ToString() == "暂停")
            {
                bt_pause.Content = "重启";
                needPause = true;
                isContinue = false;
               
            } else
            {
                clearData();
                bt_pause.Content = "暂停";
                needPause = false;
                isContinue = false;
                cb_continueLog.IsChecked = false;
            }
        }

        /*//保存成ZIP文件
        private void bt_save_Click(object sender, RoutedEventArgs e)
        {


            bt_save.Content = "保存中";
            bt_save.IsEnabled = false;
             
            String path = UserDef.PathDir +  DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text + ".zip";

            using (FileStream file = File.Create(path))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    using (BinaryWriter binaryWriter = new BinaryWriter(gzs))
                    {
                        //(use the reader as normal here)
                        binaryWriter.Write(channelNum.Count);
                        foreach(int value in channelNum)
                        {
                            binaryWriter.Write(value);
                        }
                        binaryWriter.Write(x.Count);
                        foreach (double value in x)
                        {
                            binaryWriter.Write(value);
                        }
                        foreach(List<double> y in data)
                        {
                            binaryWriter.Write(y.Count);
                            foreach (double value in y)
                            {
                                binaryWriter.Write(value);
                            }
                        }
                    }
                    byte[] cdata = ms.ToArray();
                    file.Write(cdata, 0, cdata.Length);
                }

                file.Close();

            }
            bt_save.Content = "保存数据";
            bt_save.IsEnabled = true;

        }*/
        
    
     /*   private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
           await  motionPlatform.move(Convert.ToInt32(tb_x.Text), Convert.ToInt32(tb_y.Text), Convert.ToInt32(tb_z.Text));
        }*/

    

       /* private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //await motionPlatform.move2Zero();
            await motionPlatform.move(0, -80000, 0);
        }*/

        /*private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int[] postion = motionPlatform.getCurrentPostion();
            lb_postion.Content = "x:" + postion[0] + ",   y:" + postion[1] + ",   z:" + postion[2];
        }*/

        private void saveTestCurrentData(String direction)
        {
            String path = UserDef.PathDir + DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text+"_"+direction  + ".zip";
            needPause = true;
            while (isChangeData) ;
            using (FileStream file = File.Create(path))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    using (BinaryWriter binaryWriter = new BinaryWriter(gzs))
                    {
                        //(use the reader as normal here)
                        binaryWriter.Write(channelNum.Count);
                        foreach (int value in channelNum)
                        {
                            binaryWriter.Write(value);
                        }
                        binaryWriter.Write(x.Count);
                        foreach (double value in x)
                        {
                            binaryWriter.Write(value);
                        }
                        foreach (List<double> y in data)
                        {
                            binaryWriter.Write(y.Count);
                            foreach (double value in y)
                            {
                                binaryWriter.Write(value);
                            }
                        }
                    }
                    byte[] cdata = ms.ToArray();
                    file.Write(cdata, 0, cdata.Length);
                }

                file.Close();

            }

            needPause = false;
        }

        private async void clearData()
        {
            needPause = true;
            
            x.Clear();
            Parallel.For(0, 16, (i) =>
            {
                List<Double> y = data.ElementAt(i);
                //List<Double> y1 = UserDef.freq.ElementAt(i);
                //y1.Clear();
                y.Clear();
                lastDataCount[i] = 0;
            });

            Parallel.For(0, 2, (i) => {
                List<Double> ls = UserDef.freq11.ElementAt(i);
                ls.Clear();

            });

            needPause = false;
        }
        /*private async void startTest()
        {
            //清空

            bt_start.IsEnabled = false;
            bt_start_interal.IsEnabled = false;
            bt_pause.Content = "暂停";
            needPause = false;
            testStep = 0;
            for (int xx = 0; xx < 4; xx++)
            {
                if (xx % 2 == 0)
                {
                   
                    motionPlatform.moveSingle(1, -50000);
                    //clearData();
                    isContinue = true;
                    //start recording
                    await Task.Delay(12000);
                    //save data       
                    isContinue = false;
                    testStep++;
                    saveTestCurrentData(xx.ToString());
                    //ignore path change 

                    await motionPlatform.move(0, 0, 4000);
                    await motionPlatform.move(3000, 0, 0);
                    await motionPlatform.move(0, 0, -4000);
                    testStep++;
                }
                else
                {
                    
                    
                    motionPlatform.moveSingle(1, 50000);
                   // clearData();
                    isContinue = true;
                    //start recording                   
                    await Task.Delay(12000);
                    //save data     
                    isContinue = false;
                    testStep++;
                    saveTestCurrentData(xx.ToString());
                    //ignore path change 

                    await motionPlatform.move(0, 0, 4000);
                    await motionPlatform.move(3000, 0, 0);
                    await motionPlatform.move(0, 0, -4000);
                    testStep++;
                    //await Task.Delay(250);
                }

            }
            testStep = 0;
            needPause = true;
            isContinue = false;
            Parallel.For(0, 16, (i) =>
            {
                List<Double> y = data.ElementAt(i);
                y.Clear();
                lastDataCount[i] = 0;
            });

            if (heatMapValue != null)
            {
                DataProcess.AlignDataSimple(ref heatMapValue, peakMapValue, UserDef.Frequency, UserDef.signalFrequency);
                await heatmap.Dispatcher.BeginInvoke(new Action(() =>
                {
                    heatmap.Plot(heatMapValue, heatMapX, heatMapY);
                }));
            }
            bt_pause.Content = "重启";
            bt_start.IsEnabled = true;
            bt_start_interal.IsEnabled = true;
            //   isContinue = false;
            //   await motionPlatform.move(0, -80000, 0);   
        }
*/

        


        /*private async void bt_start_Click_backup(object sender, RoutedEventArgs e)
        {
            //清空



            // 
            //
            // needPause = false;
            // isContinue = true;
            // continueCount = -2;
            // testStep =0;
            bt_start.IsEnabled = false;
            motionPlatform.moveSingle(1,-50000);
            bt_pause.Content = "暂停";
            testStep = 0;
            isContinue = true;
            needPause = false;
            //start recording
            await Task.Delay(12000);
            //save data          
            saveTestCurrentData("0");
            //ignore path change 
            isContinue = false;
            testStep++;
            await motionPlatform.move(0, 0, 4000);
            await motionPlatform.move(4000, 0, 0);
            await motionPlatform.move(0, 0, -4000);
            List<int[]> lastPeaksStart = new List<int[]>();
            int[] lastPeaks = new int[16];
            for (int i = 0; i < peaks.Count; i++)
            {
                List<int> p = peaks.ElementAt(i);
                lastPeaksStart.Add(p.ToArray());
                if(p.Count>0)
                    lastPeaks[i] = p.Last();
            }     
            
            motionPlatform.moveSingle(1, 50000);
            //start recording
            await Task.Delay(500);
            isContinue = true;
            testStep++;
            await Task.Delay(12000);
            //save
            saveTestCurrentData("1");
            List<int[]> lastPeaksEnd = new List<int[]>();
            int[] firstPeaks = new int[16];
            for (int i = 0; i < peaks.Count; i++)
            {
                lastPeaksEnd.Add(peaks.ElementAt(i).ToArray());
                List<int> p = peaks.ElementAt(i);
                lastPeaksEnd.Add(p.ToArray());
                if (p.Count > 0)
                {
                    firstPeaks[i] = peakMapValue.GetLength(1) - p[0];
                }
                
            }
            //end 
            testStep = 0;
            isContinue = false;
            needPause = true;

            await motionPlatform.move(0, 0, 4000);
            await motionPlatform.move(-4000, 0, 0);
            await motionPlatform.move(0, 0, -4000);
            await Task.Delay(3500);
            //  needPause = true;            
            bt_pause.Content = "重启";
            //根据peaks对齐heatvalue
            if(heatMapValue != null)
            {
                DataProcess.AlignData(ref heatMapValue, firstPeaks, lastPeaks, UserDef.Frequency, UserDef.signalFrequency);
                await heatmap.Dispatcher.BeginInvoke(new Action(() =>
                {
                    heatmap.Plot(heatMapValue, heatMapX, heatMapY);
                }));
            }
            bt_start.IsEnabled = true;
            //   isContinue = false;
            //   await motionPlatform.move(0, -80000, 0);            
        }
*/
        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void cb_continueLog_Checked(object sender, RoutedEventArgs e)
        {
            if(cb_continueLog.IsChecked == true)
            {
                isContinue = true;
            } else
            {
                isContinue = false;
            }
        }


        List<String> txtName = new List<String>();
        List<ListBoxItem> itemList = new List<ListBoxItem>();
        private int txtAmount = 0;
        private int pageAmount = 10000;
        static int pagePointAmount = 20;
        private String pathDir = null;
        private void bt_loadData_Click(object sender, RoutedEventArgs e)
        {
            if (start2991.Content.Equals("停止2991"))
            {
                Start2991_Click(null,null);
            }
           // start2991.Content = "启动";
            FileFolder.Dialogs.CommonOpenFileDialog dlg = new FileFolder.Dialogs.CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            dlg.ShowDialog();

            lb_dataList.Items.Clear();
            txtName.Clear();

            DirectoryInfo folder = new DirectoryInfo(dlg.FileName);
            pathDir = dlg.FileName.Clone().ToString();
            try
            {
                FileInfo[] files = folder.GetFiles("*.zip");
                Array.Sort(files, delegate (FileInfo x, FileInfo y) { return x.CreationTime.CompareTo(y.CreationTime); });
                foreach (FileInfo file in files)
                {
                    txtName.Add(file.Name);
                }
                txtAmount = txtName.Count;
                pageAmount = txtAmount / pagePointAmount;
                if (txtAmount > pageAmount * pagePointAmount)
                {
                    pageAmount++;
                }
                pageLabel.Content = "/" + pageAmount.ToString();
                currentPageText.Text = 1.ToString();
                lb_dataList.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < pagePointAmount && i < txtAmount; i++)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Content = txtName[i];
                        lb_dataList.Items.Add(item);
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            int currentPage = Convert.ToInt32(currentPageText.Text);
            if (currentPage < pageAmount)
            {
                currentPage++;
                currentPageText.Text = currentPage.ToString();
                lb_dataList.Items.Clear();
                int index = 0;
                for (int i = (currentPage - 1) * pagePointAmount; i < (currentPage - 1) * pagePointAmount + pagePointAmount && i < txtAmount; i++)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = txtName[i];
                    lb_dataList.Items.Add(item);
                    index++;
                }
            }
            else
            {
                //MessageBox.Show("超出上界");                               
            }
        }

        private void lastPageButton_Click(object sender, RoutedEventArgs e)
        {
            int currentPage = Convert.ToInt32(currentPageText.Text);
            if (currentPage > 1)
            {
                currentPage--;
                currentPageText.Text = currentPage.ToString();
                lb_dataList.Items.Clear();
                int index = 0;
                for (int i = (currentPage - 1) * pagePointAmount; i < (currentPage - 1) * pagePointAmount + pagePointAmount && i < txtAmount; i++)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = txtName[i];
                    lb_dataList.Items.Add(item);
                    index++;
                }
            }
            else
            {
                //MessageBox.Show("超出下界");
            }
        }
        private void currentPageText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int currentPage = Convert.ToInt32(currentPageText.Text);
                if (currentPage >= 1 && currentPage <= pageAmount)
                {
                    currentPageText.Text = currentPage.ToString();
                    if (lb_dataList.Items != null)
                    {
                        lb_dataList.Items.Clear();
                    }
                    int index = 0;
                    for (int i = (currentPage - 1) * pagePointAmount; i < (currentPage - 1) * pagePointAmount + pagePointAmount && i < txtAmount; i++)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Content = txtName[i];
                        lb_dataList.Items.Add(item);
                        index++;
                    }
                }
                else if (currentPage < 1)
                {
                    //MessageBox.Show("超出下界");
                }
                else
                {
                    //MessageBox.Show("超出上界");
                }
            }
        }

        double startTime = 0, endTime=0;
        int stepInterval = 1;
        const int extraFillLenght = 0;
        List<double> xdisplay = new List<double>();
        List<double> ydisplay = new List<double>();
        //const int xInteval = 1000, yInterval = 16;
        //double[] heatMapX = new double[xInteval+1];
        double[] heatMapY = new double[16*4];
        //double[,] heatMapValue = new double[xInteval, yInterval];
        //int gapThreahold = 2;
        enum CheckState
        {
            CountingZero,
            CountingNoneZero,
        };
        CheckState state = CheckState.CountingNoneZero;


      
        uint testStep = 0; //测试标记，0表示一趟，1表示暂停，2表示返回趟，即奇数不处理，偶数表示 数据
        ScottPlot.PlottableSignalXY sph;
        Random rand = new Random();
       
        int[] lineswidth = new int[5]{(int)(320/2.5), (int)(410 / 2.5), (int)(505 / 2.5), (int)(612 / 2.5), (int)(352 / 2.5) };
        int[] lineswidthInternal = new int[5] { (int)(331 / 2.5), (int)(445 / 2.5), (int)(502 / 2.5), (int)(617 / 2.5), (int)(674 / 2.5) };
        
        private void displayLines()
        {
            if(xdisplay.Count <= 0 && x.Count <=0)
            {
                return;
            }
            if (isSelectiongChanging)
            {
                return;
            }
            int halfWaveLength = UserDef.Frequency / UserDef.signalFrequency / 2;
            //Console.WriteLine("display lines");
            if(testStep%2 == 1) //奇数不处理
            {
                //清除原来的数据
                return;
            }
            //lines.Children.Clear();
            wavePlotWindow.plt.Clear();
            
            xdisplay.Clear();
            List<int> pointIndex = new List<int>();
            int stepCount = 0;
            if(stepInterval <= 0)
            {
                stepInterval = 1;
            }
            if(startTime !=0 && endTime > startTime)
            {
                for( int index =0; index<x.Count; index++)
                {
                    double v = x.ElementAt(index);
                    if(v > startTime && v < endTime)
                    {
                        if(stepCount%stepInterval == 0)
                        {
                            xdisplay.Add(v);
                            pointIndex.Add(index);
                        }
                        if(stepInterval > 1)
                        {
                            stepCount++;
                        }

                    }
                }
            }
            // 采样 率4k, 信号频率500Hz（100）， 一个波形是8（40）个点，由于做了abs，所以单个波包大约有
            // 4（20）个点, 所以阈值 采样率/信号频率/2/间隔/ + 容差
            //int checkThrehold = UserDef.Frequency / UserDef.signalFrequency / 2 / stepInterval + gapThreahold;
            //根据数据的长度，计算热力图的坐标位置
            
            
            foreach (int n in channelNum)
            {

                List<double> y = data.ElementAt(n);
                if (pointIndex.Count > 0)
                {
                    ydisplay.Clear();                    
                    foreach (int m in pointIndex)
                    {
                        ydisplay.Add(y.ElementAt(m));
                    }
                    // lg.Plot(xdisplay, ydisplay);
                    wavePlotWindow.plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", n));
                }
                else
                {
                    // lg.Plot(x, y);
                    wavePlotWindow.plt.PlotSignal(y.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", n));
                }
             
                
            }

            wavePlotWindow.plt.Legend();
            wavePlotWindow.Render();
            //heatPlotWindow.Render();
        
          
            
        }

        private void tb_startTime_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void tb_stepTime_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void tb_endTime_TextChanged(object sender, MouseEventArgs e)
        {
           
            
            
        }

        private void tb_stepTime_TextChanged(object sender, MouseEventArgs e)
        {
           
            
        }

        private void tb_startTime_TextChanged(object sender, MouseEventArgs e)
        {
            
           
        }

        private void tb_detect_threhold_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
               // gapThreahold = Convert.ToInt32(tb_detect_threhold.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void tb_endTime_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("change!!");
                endTime = Convert.ToDouble(tb_endTime.Text);
                displayLines();
            }
            catch (Exception ex)
            {

            }
        }

        private void tb_stepTime_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("change!!");
                int tmp = Convert.ToInt32(tb_stepTime.Text);
                if(UserDef.Frequency% tmp == 0)
                {
                    stepInterval = tmp;
                    displayLines();
                } else
                {
                    MessageBox.Show("步进数必须整除采样频率");
                }
                
            }
            catch (Exception ex)
            {

            }
        }

        private void tb_startTime_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("change!!");
                startTime = Convert.ToDouble(tb_startTime.Text);
                displayLines();
            }
            catch (Exception ex)
            {

            }
        }


        //数据保存成mat文件
     /*   private void bt_save_mat_Click(object sender, RoutedEventArgs e)
        {
            double[,] data = new double[2,xdisplay.Count];
            for(var i=0;i<xdisplay.Count; i++)
            {
                data[0, i] = xdisplay.ElementAt(i);
                data[1, i] = ydisplay.ElementAt(i);
            }

            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(data);

            String path = UserDef.PathDir + @"mat\"+ DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text + ".mat";
            MatlabWriter.Write(path, matrix, "data");
        }*/

        private void bt_save_all_mat_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "mat文件|*.mat|,csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = UserDef.PathDir;
            saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

                String fileName = saveFileDialog.FileName;
                double[,] matdata = new double[channelNum.Count, data.ElementAt(channelNum.ElementAt(0)).Count];
               
                /*for (var i = 0; i < x.Count; i++)
                {
                    matdata[0, i] = x.ElementAt(i);
                }*/
                int save_index = 0;
                for (int i = 0; i < channelNum.Count; i++) {
                    for (int j = 0; j < data.ElementAt(channelNum.ElementAt(i)).Count; j++) {

                        matdata[save_index, j] = data.ElementAt(channelNum.ElementAt(i)).ElementAt(j);
                    }
                    save_index++;
                
                }

                

                if (fileName.Contains(".csv"))
                {
                    StreamWriter swt = new StreamWriter(fileName);
                    for (int i = 0; i < matdata.GetLength(1); i++)
                    {
                        for (int j = 0; j < matdata.GetLength(0); j++)
                        {

                            swt.Write(matdata[j, i].ToString() + ",");

                        }

                        swt.Write("\r\n");
                    }
                    swt.Flush();
                    swt.Close();
                }
                else
                {

                    Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matdata);

                    String path = fileName;//UserDef.PathDir + @"mat\" + DateTime.Now.ToString("MM-dd-H-mm-ss_") + tb_comment.Text + ".mat";
                    MatlabWriter.Write(path, matrix, "data");
                   
                }

            }

           

           
        }

        private void tb_sampleRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UserDef.signalFrequency = Convert.ToInt32(tb_sampleRate.Text);
            }
            catch (Exception ex)
            {

            }
            
        }

        private void tb_cutoff_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UserDef.tolerance = Convert.ToDouble(tb_cutoff.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void heatPlotWindow_MouseMove(object sender, MouseEventArgs e)
        {
            (double mouseX, double mouseY) = heatPlotWindow.GetMouseCoordinates();
        
      

     
        }
       
        private static readonly object locker = new object();

        private void playSelectLines1()
        {
          
           
                    heatPlotWindow.plt.Clear();
                     for (int i = 0; i < channelNum.Count; i++)
                        {
                        if (freqflag)
                        {
                            double[] outArr = null;
                            
                            outArr = data.ElementAt(channelNum.ElementAt(i)).ToArray();
                           
                            outArr = outArr.Skip(0).Take((int)outArr.Length / 2).ToArray();
                            double[] x_axis = new double[outArr.Length];
                            for (int j = 0; j < x_axis.Length; j++)
                            {
                                x_axis[j] = j * (double)UserDef.Frequency / (double)data.ElementAt(channelNum.ElementAt(i)).Count;
                            }


                            
                            heatPlotWindow.plt.PlotSignalXY(x_axis, outArr, label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                               
                        }
                        else if (dwtflag)
                        {
                            Signal<Double> sig = new Signal<double>(data.ElementAt(channelNum.ElementAt(i)).ToArray());
                            DiscreteWaveletTransform rs = DiscreteWaveletTransform.Estimate(sig, Wavelets.Daubechies(2), new ZeroPadding<Double>());
                            DiscreteWaveletTransform rs1 = rs.EstimateMultiscale(new ZeroPadding<Double>(), 4);
                            heatPlotWindow.plt.PlotSignal(rs1.Approximation.ToArray(), (int)UserDef.Frequency / 2, startTime, label: String.Format("通道:{0}", channelNum.ElementAt(i)));

                        }

                }

           
           
            heatPlotWindow.plt.Legend();
            heatPlotWindow.Render();

        }


        //计算fft
        private Double[] computeFFT(Double [] data,int low,int countPoint) {
            object result = null;
            
            matlab.Feval("FFT", 1, out result, data);
            object[] res = result as object[];
            double[,] resdata = res[0] as double[,];
            double[] usedata = new double[(int)(resdata.GetLength(1)/2)];

            for (int j = 0; j < usedata.Length; j++)
            {
                usedata[j] = resdata[0, j];
            }
            usedata[0] = 0;
            try
            {

                double inter = Convert.ToDouble(textClasser.Text);
                double th = Convert.ToDouble(th_box.Text);

                List<Double> lis = usedata.ToList().GetRange(low, countPoint);
                object peeksArray = null;
                matlab.Feval("findPeeks", 1, out peeksArray, lis.ToArray(), inter, th);
                object[] peeksA = peeksArray as object[];
                Peeks = peeksA[0] as double[,];
                if (Peeks == null)
                {
                    try
                    {
                        singlePoint = (Double)peeksA[0];
                    }
                    catch (Exception e) { }
                }

            }
            catch (Exception e)
            {

            }
            return usedata;
        }


        private double computeRatio(double f)
        {

            //4635700.792617997 -1447480.768970162 276785.80297208356
          /*  double a = 4635700.792617997;
            double b = -1447480.768970162;
            double c = 276785.80297208356;
            double ct = c - f;

            double v = (-b - Math.Sqrt(b * b - 4 * a * ct)) / (2 * a);

            Double vol = 0;
            try
            {
                vol = Convert.ToDouble(textBox1.Text);
            }
            catch (Exception e)
            {

            }*/

            double p = parameters.ElementAt(0).ElementAt(0) * f * f + parameters.ElementAt(0).ElementAt(1)*f + parameters.ElementAt(0).ElementAt(2);

            // return vol*vol*v;
            return p;

        }

        private delegate void FlushClient();
        private static char[] classer = new char[2];
        private MemoryMappedFile memoryMappedFile=null;
        private MemoryMappedViewAccessor  ViewAccessor= null;
        private long capacity = 1 << 10 << 10;
        //获取分类的结果
        private void getClasserOut()
        {
            

            long capacity = 1 << 10 << 10;

            //创建或者打开共享内存
         
                //循环写入，使在这个进程中可以向共享内存中写入不同的字符串值
                while (freqflag)
                {
                 
                    ViewAccessor.Flush();
                    Byte[] charsInMMf = new Byte[100];
                //读取字符
                    ViewAccessor.ReadArray<Byte>(0, charsInMMf, 0, 100);

                    char[] cs = new char[100];
                    for (int i = 0; i < charsInMMf.Length; i++)
                    {

                        cs[i] = (char)charsInMMf[i];
                    }

                    classer = cs;

                    ViewAccessor.Flush();

                    
                    
                    /*if (charsInMMf[0] != 0 && charsInMMf[1] != 0)
                    {
                        this.textClasser.Dispatcher.Invoke(new Action(() =>
                        {
                            if (this.textClasser.LineCount > 20)
                            {

                                this.textClasser.Clear();
                            }



                            this.textClasser.AppendText("高峰:" + cs[0] + "电源" + "--" + "低峰:" + cs[1] + "电源\n");
                            this.textClasser.ScrollToEnd();

                        }));
                      
                    }*/
                    //Console.WriteLine(msg);
                    //Console.WriteLine("[{0}]", string.Join(", ", cs));

                    Thread.Sleep(5000);
                   
                }

            


        }

        private StringBuilder builder = new StringBuilder();
        private int intervalTh = 5000; 
        private List<Double> axis_f = new List<double>();
        private double highPower = 0,lowPower = 0;

        private void computePower(List<Double> lis,int lowf)
        {
            /////////////////////////////////////////////////////////////////////////////////
            double max1 = lis.Max();
            int index1 = lis.IndexOf(max1);

            try
            {
                intervalTh = Convert.ToInt32(textClasser.Text);

            }catch(Exception e)
            {

            }

            List<Double> left = lis.GetRange(intervalTh*4, index1 - intervalTh*5); //最大值左侧数据
            List<Double> right = lis.GetRange(index1 + intervalTh, lis.Count - index1 - intervalTh * 4); //最大值右侧数据

            double max_left = left.Max(); //左侧最大值
            double max_right = right.Max(); //右侧最大值
            int index2 = 0;

            if (max_left > max_right)
            {
                index2 = left.IndexOf(max_left)+intervalTh*4;
            }
            else
            {
                index2 = right.IndexOf(max_right) + index1 + intervalTh;
            }

            //计算峰值部分波形
            List<Double> f1;
            List<Double> f2;
            //如果上述两个峰值都不满足条件，则没必要进行下面的操作
            if (max_left <= 100 && max_right <= 100)
            {               
                
                highPower = computeRatio(index1 + lowf);
                f1 = lis.GetRange(index1 - 5000, 10000);              
                builder.Clear();
                for (int ns = 0; ns < 2 * f1.Count; ns++)
                {
                    builder.Append(Math.Round(f1.ElementAt(ns % f1.Count), 2).ToString() + ",");

                }
                channel.BasicPublish("", "SwapScope", null, Encoding.UTF8.GetBytes(builder.ToString()));

            }

            else
            {

                if (index1 < index2 && index1 + 5000 > index2)
                {
                    f1 = lis.GetRange(index1 - 5000, (int)((index2 - index1) / 2)+5000);
                    for (int k = 0; k < 5000 - (int)((index2 - index1) / 2); k++)
                    {
                        f1.Add(0);
                    }

                    f2 = lis.GetRange(index2 - (int)((index2 - index1) / 2), 5000 + (int)((index2 - index1) / 2));

                    for (int k = 0; k < 5000 - (int)((index2 - index1) / 2); k++)
                    {

                        f2.Insert(0, 0);
                    }

                        
                }
                else if (index1 > index2 && index1 - 5000 < index2)
                {
                    f1 = lis.GetRange(index1 - (int)((index1 - index2) / 2), 5000 + (int)((index1 - index2) / 2));
                    for (int k = 0; k < 5000 - (int)((index1 - index2) / 2); k++)
                    {

                        f1.Insert(0, 0);
                    }

                    f2 = lis.GetRange(index2 - 5000, (int)((index1 - index2) / 2) + 5000);
                    for (int k = 0; k < 5000 - (int)((index1 - index2) / 2); k++)
                    {
                        f2.Add(0);
                    }

                }
                else
                {
                    f1 = lis.GetRange(index1 - 5000, 10000);
                    f2 = lis.GetRange(index2 - 5000, 10000);
                }


                highPower = computeRatio(index1 + lowf);
                lowPower = computeRatio(index2 + lowf);
                /*textOut.AppendText(Math.Round(p1, 2) + "---" + Math.Round(p2, 2) + "\n");
                textOut.ScrollToEnd();*/

                builder.Clear();
                for (int ns = 0; ns < f1.Count; ns++)
                {
                    builder.Append(Math.Round(f1.ElementAt(ns), 2).ToString() + ",");
                }
                for (int ns = 0; ns < f2.Count; ns++)
                {
                    builder.Append(Math.Round(f2.ElementAt(ns), 2).ToString() + ",");
                }

                channel.BasicPublish("", "SwapScope", null, Encoding.UTF8.GetBytes(builder.ToString()));


            }

            //用电源进行计算
            String [] current = current_text.Text.Split('-');
            String Us = volBox.Text;
            try
            {
                double i1 = Convert.ToDouble(current[0]);
                double i2 = Convert.ToDouble(current[1]);
                List<Double> I1 = data.ElementAt(1);
                List<Double> I2 = data.ElementAt(4);
                double averI1 = I1.Average();
                double averI2 = I2.Average();
                i1 = (averI1-  i1) / 0.06;
                i2 = (averI2 - i2) / 0.06;
                double U = Convert.ToDouble(Us);
                Double P1 = i1 * U;
                Double P2 = i2 * U;
                richTextBox.AppendText(Math.Round(P1,2)+","+Math.Round(P2,2)+"\n");
                richTextBox.ScrollToEnd();

            }
            catch(Exception e)
            {

            }



            if (classer[0] != 0 && classer[1] != 0)
            {
                int c1 = Convert.ToInt32(classer[0].ToString())+1;
                int c2 = Convert.ToInt32(classer[1].ToString())+1;

                heatPlotWindow.plt.PlotAnnotation("高峰:" + c1 + "号电源,功率:" + Math.Round(highPower,2) + "--" + "低峰:" + c2 + "号电源,功率:" + Math.Round(lowPower,2), fontSize: 20);

            }
            else
            {
                heatPlotWindow.plt.PlotAnnotation("电源功率分别是:" + Math.Round(highPower,2) + "," + Math.Round(lowPower,2), fontSize: 20);
            }
        }


        private void computeHighValue(List<Double> list,int lowf,int highf,int i)
        {
            double[] ds = new double[list.Count];
            for (int j = 0; j < ds.Length; j++)
            {
                ds[j] = list.ElementAt(j) - 0.3 * i;
            }

            Double[] usedata = computeFFT(ds,lowf,highf);


            //获取指定频段范围
            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);

            double max = lis.Max();
            double index = (double)(lis.IndexOf(max));

            freq.ElementAt(channelNum.ElementAt(i)).Add(index+lowf + 0.3 * i);


            if (freq.ElementAt(channelNum.ElementAt(i)).Count >= 2000)
            {
                freq.ElementAt(channelNum.ElementAt(i)).RemoveAt(0);
            }
            //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
            heatPlotWindow.plt.PlotAnnotation("当前最大值为:"+(index+lowf),fontSize:20);
            heatPlotWindow.plt.PlotSignal(freq.ElementAt(channelNum.ElementAt(i)).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
        }


        private void computeMaxValue(List<Double> list,int lowf,int highf,int i)
        {
            double[] ds = new double[list.Count];
            for (int j = 0; j < ds.Length; j++)
            {
                ds[j] = list.ElementAt(j) - 0.3 * i;
            }

            Double[] usedata = computeFFT(ds,lowf,highf);
            //获取指定频段范围
            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
            double max = lis.Max();
            UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).Add(max);
            if (UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).Count > 2000)
            {
                UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).RemoveAt(0);
            }
            heatPlotWindow.plt.PlotSignal(MAXVALUE.ElementAt(channelNum.ElementAt(i)).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
        }

        private List<int> getPeeks()
        {

            List<int> list = new List<int>();
            if (Peeks != null)
            {
                int row = Peeks.GetLength(0);
                int col = Peeks.GetLength(1);
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        list.Add((int)Peeks[i,j]);
                       
                    }
                }
                
            }
            Console.WriteLine("找到的峰值个数:"+list.Count);
            return list;
        }


        private void computePowerModif(List<Double> F,int lowf)
        {
            List<int> list = getPeeks();
            StringBuilder builder = new StringBuilder();
            List<Double> Power = new List<double>();

            if (singlePoint != 0&&list.Count==0)
            {
                int peekf = (int)singlePoint + lowf;
                richTextBox.Dispatcher.BeginInvoke(new Action(() =>
                {

                    richTextBox.AppendText(peekf.ToString() + ",");
                }));
                Power.Add(parameters.ElementAt(0).ElementAt(0) * peekf * peekf*peekf
                    + parameters.ElementAt(0).ElementAt(1) * peekf*peekf + parameters.ElementAt(0).ElementAt(2)*peekf+parameters.ElementAt(0).ElementAt(3));
                List<Double> tmp = F.GetRange((int)singlePoint - 5000, 10000);
                tmp.ForEach(s => builder.Append(s.ToString() + ","));

            }


            //List<List<Double>> peekData = new List<List<double>>();
            for (int i = 0; i < list.Count; i++)
            {
                int peekf = list.ElementAt(i) + lowf;

                richTextBox.Dispatcher.BeginInvoke(new Action(() =>
                {

                    richTextBox.AppendText(peekf.ToString() + ",");
                }));

                Power.Add(parameters.ElementAt(0).ElementAt(0) * peekf * peekf*peekf
                    + parameters.ElementAt(0).ElementAt(1) * peekf*peekf + parameters.ElementAt(0).ElementAt(2)*peekf+parameters.ElementAt(0).ElementAt(3));

                if (list.Count == 1)
                {
                    List<Double> tmp = F.GetRange(list.ElementAt(i) - 5000, 10000);
                    tmp.ForEach(v => builder.Append(v.ToString() + ","));
                    //peekData.Add(tmp);
                }
                else if (i == 0)
                {
                    if (list.ElementAt(i) - 5000 <= 0)
                    {
                        continue;
                    }

                    if (5000 + list.ElementAt(i) > list.ElementAt(i + 1))
                    {
                        List<Double> tmp = F.GetRange(list.ElementAt(i) - 5000, 5000 + (list.ElementAt(i+1) - list.ElementAt(i)) / 2);
                        int last = tmp.Count - 1;
                        for (int k = 0; k < 5000 - (list.ElementAt(i+1) - list.ElementAt(i)) / 2; k++)
                        {
                            tmp.Insert(last, 0);
                        }
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                    else
                    {
                        List<Double> tmp = F.GetRange(list.ElementAt(i) - 5000, 10000);
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                }
                else if (i == list.Count-1)
                {

                    if (list.ElementAt(i) + 5000 > F.Count)
                    {
                        return;
                    }

                    if (list.ElementAt(i) - 5000 < list.ElementAt(i - 1))
                    {

                        List<Double> tmp = F.GetRange((list.ElementAt(i)+list.ElementAt(i-1))/2,5000+(list.ElementAt(i)-list.ElementAt(i-1))/2);
                        for (int k = 0; k < 5000 - (list.ElementAt(i) - list.ElementAt(i-1)) / 2; k++)
                        {
                            tmp.Insert(0, 0);
                        }
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                    else
                    {
                        List<Double> tmp = F.GetRange(list.ElementAt(i) - 5000, 10000);
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }

                }
                else
                {   //处理其他的峰值部分


                    if (list.ElementAt(i) - 5000 > list.ElementAt(i - 1) && list.ElementAt(i) + 5000 < list.ElementAt(i + 1))
                    {
                        List<Double> tmp = F.GetRange(list.ElementAt(i) - 5000, 10000);
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(F.GetRange(list.ElementAt(i) - 5000, 10000));
                    }
                    else if (list.ElementAt(i) - 5000 > list.ElementAt(i - 1) && list.ElementAt(i) + 5000 > list.ElementAt(i + 1))
                    {
                        List<Double> tmp = F.GetRange(list.ElementAt(i)-5000,5000+(list.ElementAt(i+1)-list.ElementAt(i))/2);
                        int last = tmp.Count;
                        for (int k = 0; k < 5000 - (list.ElementAt(i + 1) - list.ElementAt(i)) / 2; k++)
                        {
                            tmp.Insert(last, 0);
                        }
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                    else if (list.ElementAt(i) - 5000 < list.ElementAt(i - 1) && list.ElementAt(i) + 5000 < list.ElementAt(i + 1))
                    {
                        List<Double> tmp = F.GetRange((list.ElementAt(i)+list.ElementAt(i-1))/2, 5000 + (list.ElementAt(i) - list.ElementAt(i-1)) / 2);
                        for (int k = 0; k < 5000 - (list.ElementAt(i) - list.ElementAt(i - 1)) / 2; k++)
                        {
                            tmp.Insert(0, 0);
                        }
                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                    else
                    {

                        List<Double> tmp = F.GetRange((list.ElementAt(i) + list.ElementAt(i - 1)) / 2, (list.ElementAt(i) - list.ElementAt(i - 1)) / 2
                            + (list.ElementAt(i + 1) - list.ElementAt(i)) / 2);

                        for (int k = 0; k < 5000 - (list.ElementAt(i) - list.ElementAt(i - 1)) / 2; k++)
                        {
                            tmp.Insert(0, 0);
                        }
                        int last = tmp.Count;
                        for (int k = 0; k < 5000 - (list.ElementAt(i + 1) - list.ElementAt(i)) / 2; k++)
                        {
                            tmp.Insert(last, 0);
                        }

                        tmp.ForEach(v => builder.Append(v.ToString() + ","));
                        //peekData.Add(tmp);
                    }
                }

            }

            if (builder.Length > 0)
            {

                richTextBox.Dispatcher.BeginInvoke(new Action(() =>
                {

                    richTextBox.AppendText("\n");
                    richTextBox.ScrollToEnd();
                }));

                channel.BasicPublish("", "SwapScope", null, Encoding.UTF8.GetBytes(builder.ToString()));
                builder.Clear();
                if (classer.Length > 0)
                {
                    for(int i = 0; i < Power.Count && classer[i]!=0; i++)
                    {
                        builder.Append("电源"+(Convert.ToInt32(classer[i].ToString())+1)+"的输出功率为:"+Power[i]+"\n");
                    }
                    heatPlotWindow.plt.PlotAnnotation(builder.ToString(), fontSize: 20);
                }
            }

            


        }


        private double[,] Peeks = null;
        private Double singlePoint = 0;
        private void playSelectLines() {
            if (xdisplay.Count <= 0 && x.Count <= 0)
            {
                return;
            }
            if (isSelectiongChanging)
            {
                
                return;
            }
            int halfWaveLength = UserDef.Frequency / UserDef.signalFrequency / 2;
            //Console.WriteLine("display lines");
            if (testStep % 2 == 1) //奇数不处理
            {
                //清除原来的数据
                return;
            }
            //lines.Children.Clear();
            wavePlotWindow.plt.Clear();
            heatPlotWindow.plt.Clear();
            xdisplay.Clear();
            List<int> pointIndex = new List<int>();
            int stepCount = 0;
            if (stepInterval <= 0)
            {
                stepInterval = 1;
            }
            if (startTime != 0 && endTime > startTime)
            {
                for (int index = 0; index < x.Count; index++)
                {
                    double v = x.ElementAt(index);
                    if (v > startTime && v < endTime)
                    {
                        if (stepCount % stepInterval == 0)
                        {
                            xdisplay.Add(v);
                            pointIndex.Add(index);
                        }
                        if (stepInterval > 1)
                        {
                            stepCount++;
                        }

                    }
                }
            }




            try
            {
                intervalTh = Convert.ToInt32(textClasser.Text);

            }
            catch (Exception e) { 
            
            
            }



            for (int i = 0; i < channelNum.Count; i++)
            {


                    List<double> y = data.ElementAt(channelNum.ElementAt(i));
                    if (pointIndex.Count > 0)
                    {
                        ydisplay.Clear();
                        foreach (int m in pointIndex)
                        {
                            ydisplay.Add(y.ElementAt(m)+0.3*i);
                        }
                        if (ydisplay.Count > 0)
                        {

                            int lowf = 0;
                            int highf = ydisplay.Count / 2;
                            if (textBox.Text != "0")
                            {
                                String[] ts = textBox.Text.Split('-');
                                try
                                {
                                    lowf = Convert.ToInt32(ts[0]);
                                    highf = Convert.ToInt32(ts[1]);

                                }
                                catch (Exception) { }

                            }

                            wavePlotWindow.plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", channelNum.ElementAt(i)));
                            
                            if (freqflag&& channelNum.ElementAt(i) != 13 && channelNum.ElementAt(i) != 14)
                            {

                                double[] ds = new double[ydisplay.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {


                                    ds[j] = ydisplay.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds,lowf,highf);
                                


                            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);




                            if (computeFlag)
                            {
                                //computePower(lis, lowf);
                                computePowerModif(lis, lowf);
                            }

                                ////////////////////////////////////////////////////////////////////////////////////////////


                                UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Clear();

                                for (int c = 0; c < lis.Count; c++)
                                {
                                    if (channelNum.ElementAt(i) == 12)
                                    {
                                        lis[c] = lis[c] + i;
                                        UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Add(lis[c]);
                                    }
                                    else
                                    {
                                        lis[c] = lis[c];
                                        UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Add(lis[c]);
                                    }
                                }

                                //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                                axis_f.Clear();
                                for (int ns = 0; ns < lis.Count; ns++)
                                {
                                    axis_f.Add(ns + lowf);
                                }

                                /*List<Double> res = new List<double>();
                                for (int j = 0; j < lis.Count; j++)
                                {
                                    res.Add(lis.ElementAt(j));
                                    for (int k = 0; k < UserDef.Frequency/ydisplay.Count-1; k++)
                                    {
                                        res.Add(0);
                                    }
                                }*/

                                //heatPlotWindow.plt.PlotSignal(lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                                heatPlotWindow.plt.PlotSignalXY(axis_f.ToArray(), lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                            }
                            else if (computeHigh&& channelNum.ElementAt(i) != 13 && channelNum.ElementAt(i) != 14)
                            {  //计算最大频率点

                                computeHighValue(ydisplay, lowf, highf, i);

                            }
                            else if (computeMax&& channelNum.ElementAt(i) != 1 && channelNum.ElementAt(i) != 4) //计算最大幅值
                            {
                                computeMaxValue(ydisplay, lowf, highf, i);
                            }
                            else if (dwtflag&& channelNum.ElementAt(i) != 1 && channelNum.ElementAt(i) != 4)
                            {
                                if (form != null)
                                    form.setData(ydisplay);

                            }


                        }
                    }

                    else
                    {
                        if (y.Count > 0)
                        {
                            ydisplay.Clear();
                            for(int n = 0; n < y.Count; n++)
                            {
                                ydisplay.Add(y.ElementAt(n)+0.3*i);
                            }


                            int lowf = 0;
                            int highf = ydisplay.Count / 2;
                            if (textBox.Text != "0")
                            {
                                String[] ts = textBox.Text.Split('-');
                                try
                                {
                                    lowf = Convert.ToInt32(ts[0]);
                                    highf = Convert.ToInt32(ts[1]);

                                }
                                catch (Exception) { }

                            }

                            wavePlotWindow.plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", channelNum.ElementAt(i)));

                            if (freqflag && channelNum.ElementAt(i) != 13 && channelNum.ElementAt(i) != 14)
                            {

                                //double[] res = Filter(y.ToArray(), i);

                                double[] ds = new double[ydisplay.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {
                                    ds[j] = ydisplay.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds,lowf,highf);
                                


                            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                           



                            if (computeFlag)
                                {

                                //computePower(lis, lowf);
                                computePowerModif(lis, lowf);

                            }
                                ////////////////////////////////////////////////////////////////////////////////////////////



                                UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Clear();


                                for (int c = 0; c < lis.Count; c++)
                                {
                                    if (channelNum.ElementAt(i) == 12)
                                    {
                                        lis[c] = lis[c] + i;
                                        UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Add(lis[c]);
                                    }
                                    else
                                    {
                                        lis[c] = lis[c];
                                        UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Add(lis[c]);
                                    }

                                }
                                //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                                //heatPlotWindow.plt.PlotSignal(lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                                axis_f.Clear();
                                for (int ns = 0; ns < lis.Count; ns++)
                                {
                                    axis_f.Add(ns + lowf);
                                }

                                /*   List<Double> res = new List<double>();
                                   for (int j = 0; j < lis.Count; j++)
                                   {
                                       res.Add(lis.ElementAt(j));
                                       for (int k = 0; k < UserDef.Frequency / y.Count-1; k++)
                                       {
                                           res.Add(0);
                                       }
                                   }*/

                                //heatPlotWindow.plt.PlotSignal(lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                                heatPlotWindow.plt.PlotSignalXY(axis_f.ToArray(), lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));

                            }
                            else if (computeHigh && channelNum.ElementAt(i) != 1 && channelNum.ElementAt(i) != 4)
                            {

                                computeHighValue(ydisplay, lowf, highf, i);
                            }
                            else if (computeMax && channelNum.ElementAt(i) != 1 && channelNum.ElementAt(i) != 4)
                            {
                                computeMaxValue(ydisplay, lowf, highf, i);
                            }
                            else if (dwtflag && channelNum.ElementAt(i) != 1 && channelNum.ElementAt(i) != 4)
                            {
                                if (form != null)
                                    form.setData(ydisplay);


                            }



                        }
                    }

                }

                wavePlotWindow.plt.Legend();
                heatPlotWindow.plt.Legend();

                wavePlotWindow.Render();
                heatPlotWindow.Render();
               
        }


        
        

        private bool freqflag = false;
        private bool dwtflag = false;
        private bool computeHigh = false;
        private bool computeMax = false;

        private void changeCondition()
        {
            freqflag = false;
            dwtflag = false;
            computeFlag = false;
            computeHigh = false;
        }

        private void bt_display_mode_Click(object sender, RoutedEventArgs e)
        {
            
            if (freqflag == false)
            {
                freqflag = true;
                new Thread(new ThreadStart(getClasserOut)).Start();
            }
            else
            {

                freqflag = false;
            }
          
        }

       

        private void cb_Check_All_Click(object sender, RoutedEventArgs e)
        {
            if (cb_Check_All.IsChecked  == false)
            {
                cb_Check_All.IsChecked = false;
                cb_Check_All.Content = "全不选";
                for(var i=0; i< upStackPannel.Children.Count; i++) {
                    CheckBox tmp = upStackPannel.Children[i] as CheckBox;
                    tmp.IsChecked = false;
                }
                for (var i = 0; i < downStackPannel.Children.Count; i++)
                {
                    CheckBox tmp = downStackPannel.Children[i] as CheckBox;
                    tmp.IsChecked = false;
                }
            }
            else
            {
                cb_Check_All.IsChecked = true;
                cb_Check_All.Content = "全选";
                for (var i = 0; i < upStackPannel.Children.Count; i++)
                {
                    CheckBox tmp = upStackPannel.Children[i] as CheckBox;
                    tmp.IsChecked = true;
                }
                for (var i = 0; i < downStackPannel.Children.Count; i++)
                {
                    CheckBox tmp = downStackPannel.Children[i] as CheckBox;
                    tmp.IsChecked = true;
                }
            }
        }

        private void cb_method_static_Click(object sender, RoutedEventArgs e)
        {
            isWavletMethod = cb_method_static.IsChecked==false;
        }

        


        private void csv_Ckeck(object sender, RoutedEventArgs e) {
            CheckBox ch = (CheckBox)sender;

            if (ch.IsChecked == true)
            {
                csvSave = true;

            }
            else {
                csvSave = false;
            }
        }



        private void timeOutSave_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            timer1 = new Timer(new TimerCallback(timeOver), null, 10000, 10000);

            UserDef.flagRecord = true;
        }

        private bool computeFlag = false;


        private void ComputeThread()
        {
            if (p == null)
            {
                p = new Process();
                String path = Directory.GetCurrentDirectory();
                path = "I:\\west_tubecopy\\west_tubecopy\\wtf\\bin\\x86\\Debug\\ClasserFiles\\sparate.py";
                //path = "python " + path + "\\ClasserFiles\\sparate.py";
                //p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.FileName = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python37\\python.exe";
                p.StartInfo.Arguments = path;
                //是否使用操作系统shell启动
                p.StartInfo.UseShellExecute = false;
                // 接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardInput = true;
                //输出信息
                p.StartInfo.RedirectStandardOutput = true;
                // 输出错误
                p.StartInfo.RedirectStandardError = true;
                //不显示程序窗口
                p.StartInfo.CreateNoWindow = true;
                //启动程序
                bool fs = p.Start();
                //p.StandardInput.WriteLine(path);
                //p.StandardInput.WriteLine("exit");
                //p.StandardInput.AutoFlush = true;
                if (fs)
                {
                    
                    textBox.Dispatcher.BeginInvoke(new Action(() => {
                        textbox.AppendText("后台计算程序已启动！！\n");

                    }));
                    computeFlag = true;
                    /*p.WaitForExit();
                    p.Close();*/
                }
                else
                {
                    textBox.Dispatcher.BeginInvoke(new Action(() => {
                        textbox.AppendText("计算程序启动失败！！\n");

                    }));
                    
                }
                



            }
            //textbox.AppendText("后台计算程序已启动！！\n");
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {

            

            
            //ClasserFiles
            if (freqflag == true)
            {

                if (computeFlag == false)
                {
                    computeFlag = true;

                    //new Thread(new ThreadStart(ComputeThread)).Start();
                    //ComputeThread();
                    /*if (p == null)
                    {
                        p = new Process();
                        String path = Directory.GetCurrentDirectory();
                        path = path + "\\ClasserFiles\\sparate.py";
                        //path = "python " + path + "\\ClasserFiles\\sparate.py";
                        //p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.FileName = "C:\\Users\\Admin\\AppData\\Local\\Programs\\Python\\Python37\\python.exe";
                        p.StartInfo.Arguments = path;
                        //是否使用操作系统shell启动
                        p.StartInfo.UseShellExecute = false;
                        // 接受来自调用程序的输入信息
                        p.StartInfo.RedirectStandardInput = true;
                        //输出信息
                        p.StartInfo.RedirectStandardOutput = true;
                        // 输出错误
                        p.StartInfo.RedirectStandardError = true;
                        //不显示程序窗口
                        p.StartInfo.CreateNoWindow = true;
                        //启动程序
                        bool fs = p.Start();
                        //p.StandardInput.WriteLine(path);
                        p.StandardInput.WriteLine("exit");
                        p.StandardInput.AutoFlush = true;
                        if (fs)
                        {

                            textBox.Dispatcher.BeginInvoke(new Action(() => {
                                textbox.AppendText("后台计算程序已启动！！\n");

                            }));
                            computeFlag = true;
                            *//*p.WaitForExit();
                            p.Close();*//*
                        }
                        else
                        {
                            textBox.Dispatcher.BeginInvoke(new Action(() => {
                                textbox.AppendText("计算程序启动失败！！\n");

                            }));

                        }


                    }*/




                }
                else
                {

                    computeFlag = false;
                }

            }
        }
        private DWTForm form = null;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            

            if (dwtflag == false)
            {
                dwtflag = true;
                form = new DWTForm();
                form.Show();
            }
            else {
                dwtflag = false;
                if (form != null)
                {
                    form.Dispose();
                }
            
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (computeHigh)
            {
                computeHigh = false;

            }
            else {
                computeHigh = true;
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (computeMax) {
                computeMax = false;
            }
            else {
                computeMax = true;
            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (p != null)
            {
                Process[] processes = Process.GetProcessesByName("python");
                foreach(Process o in processes)
                {
                    o.Kill();
                }
                /*p.WaitForExit();
                p.Close();*/
            }
        }

       

        //保存特征
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "csv文件|*.csv";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = "I:\\频谱数据\\";
            saveFileDialog.FileName = DateTime.Now.ToString("MM-dd-H-mm-ss_");
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                String fileName = saveFileDialog.FileName;
                double[,] matdata = new double[channelNum.Count, x.Count];
                int save_index = 0;
                for (int i = 0; i < channelNum.Count; i++)
                {
                    for (int j = 0; j < UserDef.dataToSave.ElementAt(channelNum.ElementAt(i)).Count; j++)
                    {

                        matdata[save_index, j] = dataToSave.ElementAt(channelNum.ElementAt(i)).ElementAt(j);
                    }
                    save_index++;

                }



                if (fileName.Contains(".csv"))
                {
                    StreamWriter swt = new StreamWriter(fileName);
                    for (int i = 0; i < matdata.GetLength(1); i++)
                    {
                        for (int j = 0; j < matdata.GetLength(0); j++)
                        {

                            swt.Write(matdata[j, i].ToString() + ",");

                        }

                        swt.Write("\r\n");
                    }
                    swt.Flush();
                    swt.Close();
                }
            }

        }

        private void lb_dataList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)lb_dataList.SelectedItem;
            if (item != null)
            {
                List<int> txtPointAmout = new List<int>();

                string filePath = System.IO.Path.Combine(pathDir, item.Content.ToString());
                Console.WriteLine(filePath);
                using (FileStream file = File.OpenRead(filePath))
                {

                    using (GZipStream gz = new GZipStream(file, CompressionMode.Decompress))
                    {
                        //先读channlenum

                        byte[] by = new byte[sizeof(int)];
                        int r = gz.Read(by, 0, by.Length);
                        if(r > 0)
                        {
                           int channleCount = System.BitConverter.ToInt32(by, 0);
                            byte[] channelBytes = new byte[channleCount * sizeof(int)];
                            int[] channeldata = new int[channleCount];
                            r = gz.Read(channelBytes, 0, channelBytes.Length);
                            Buffer.BlockCopy(channelBytes, 0, channeldata, 0, r);

                            //再读x
                            gz.Read(by, 0, by.Length);
                            int xcount = System.BitConverter.ToInt32(by, 0);
                            byte[] xdataBytes = new byte[xcount * sizeof(double)];
                            double[] xdata = new double[xcount];
                            r = gz.Read(xdataBytes, 0, xdataBytes.Length);
                            Buffer.BlockCopy(xdataBytes, 0, xdata, 0, r);

                            x.Clear();
                            
                            foreach (double value in xdata)
                            {
                                x.Add(value);
                            }
                            channelNum.Clear();
                            // data.Clear();
                            //再读y
                            for (int m= 0; m < data.Count; m++)
                            {
                                gz.Read(by, 0, by.Length);
                                int ycount = System.BitConverter.ToInt32(by, 0);
                                if(ycount > 0)
                                {
                                    channelNum.Add(m);
                                    List<double> y = data.ElementAt(m);
                                    y.Clear();
                                    byte[] ydataBytes = new byte[ycount * sizeof(double)];
                                    double[] ydata = new double[ycount];
                                    r = gz.Read(ydataBytes, 0, ydataBytes.Length);
                                    Buffer.BlockCopy(ydataBytes, 0, ydata, 0, r);
                                    /*if (cb_lowpassFilter.IsChecked == true)
                                    {
                                        double timesnap = 1 / Convert.ToDouble(tb_sampleRate.Text);
                                        double cutoff = Convert.ToDouble(tb_cutoff.Text);
                                        ydata = DataProcess.Butterworth(ydata, timesnap, cutoff);
                                    }*/
                                    foreach (double v in ydata)
                                    {
                                        y.Add(v);
                                    }
                                    if (r <= 0)
                                    {
                                        break;
                                    }

                                }
                               
                            }

                            displayLines();
                           
                        }                        

                    }
                }
            }
        }
       
    }


    public class socketHander : WebSocketBehavior
    {

        protected override void OnMessage(MessageEventArgs e)
        {
            String data = e.Data;
            Console.WriteLine(data);
            try {
                if (UserDef.flagRecord)
                {
                    UserDef.NowRes = Convert.ToDouble(data);
                    UserDef.xToSave.Add(UserDef.NowRes);
                }
                } catch (Exception) { }
            
            Thread.Sleep(500);
            Send("OK");

        }

        protected override void OnOpen()
        {
            try
            {
                
            }
            catch (Exception) { }
        }

    }
}
