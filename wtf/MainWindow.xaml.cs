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
        
        //private static volatile List<List<Double>> dataToSave = new List<List<Double>>();
        public MainWindow()
        {
            InitializeComponent();
            Type matlabAppType = System.Type.GetTypeFromProgID("Matlab.Application");
            matlab = System.Activator.CreateInstance(matlabAppType) as MLApp.MLApp;
            string path_project = System.IO.Directory.GetCurrentDirectory();
            string path_matlabwork = "cd('"+ path_project +"')";
            matlab.Execute(path_matlabwork);

            webSocketServer = new WebSocketServer("ws://192.168.171.1:8089");
            webSocketServer.AddWebSocketService<socketHander>("/");
            webSocketServer.Start();
            
            /*string init2991Values = ConfigurationManager.AppSettings["maxth9001"];
            if(init2991Values == null)
            {
                init2991Values = "";
            }
            string[] init2991v = init2991Values.Split(',');

            string initValuesStr = ConfigurationManager.AppSettings["threaholdValues"];
            int[] initValues = new int[65];
            if (initValuesStr == null)
            {
                initValuesStr = "";
            } 
            string[] splitArr = initValuesStr.Split(',');
            try
            {
                for (int i = 0; i < 16; i++)
                {
                    initValues[i*4] = splitArr.Length > 63 ? Convert.ToInt32(splitArr[i*4]) : 33000;
                    initValues[i*4 + 1] = splitArr.Length > 63 ? Convert.ToInt32(splitArr[i*4 + 1]) : 31000;
                    initValues[i*4 + 2] = splitArr.Length > 63 ? Convert.ToInt32(splitArr[i*4 + 2]) : 33000;
                    initValues[i*4 + 3] = splitArr.Length > 63 ? Convert.ToInt32(splitArr[i*4 + 3]) : 31000;
                }
            } catch(Exception e)
            {

            }
            


            if (d2991A.Count <= 0)
            {
                for (int i = 0; i < 16; i++)
                {
                   
                    UserDef.d2991A.Add(new channleSetting()
                    {
                        no = i,
                        max = 0,
                        max_th = initValues[i*4],
                        min_th = initValues[i*4+1]

                    });
                }
            }*/
            //dg_2991.ItemsSource =UserDef.d2991;
            //dg_9003.ItemsSource = UserDef.d9003;
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
           // tb_sampleRate.Text = UserDef.Frequency.ToString();
            /*Type matlabAppType = System.Type.GetTypeFromProgID("Matlab.Application");
            matlab = System.Activator.CreateInstance(matlabAppType) as MLApp.MLApp;
            string path_project = System.IO.Directory.GetCurrentDirectory();
            //string path_matlab = "cd('C:\\Users\\wuxji\\CloudStation\\Drive\\works\\UESTC\\测厚仪\\2020\\dataprocess')";
            string path_matlab = "cd('C:\\ART\\west_tubecopy\\wtf')";
            matlab.Execute(path_matlab);
            matlab.Execute("clear all");
            matlab.Visible = 1;*/

            /*for (int i = 0; i <= xInteval; i++)
                heatMapX[i] = i*10;*/

            /*for (int j = 0; j < 16 * 4; j++)
                heatMapY[j] = j;*/
            heatPlotWindow.plt.Title("频谱图");
            heatPlotWindow.plt.XLabel("频率(Hz)");
            heatPlotWindow.plt.YLabel("通道");
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
            UserDef.timeOut = false;
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
        
        }


        private double temp_value = 0;
        
        private int count_index = 0;
        private bool csvSave = false;
        private void timeOver(object a) {
            UserDef.flagRecord = false;


            if (channelNum.Count!=0&& UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count != 0)
            {

                int count = channelNum.Count;
                int save_index = 0;
               

                double[,] matdata = new double[count + 1, UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count];
                if (UserDef.xToSave.Count == 0) {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {
                       
                        matdata[save_index, i] = UserDef.NowRes;
                    }

                }

                 else  if (UserDef.xToSave.Count * (UserDef.Frequency/2) >= UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count)
                {

                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count; i++)
                    {
                        int n = i / (UserDef.Frequency/2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }

                }
                else
                {

                    for (int i = 0; i < UserDef.xToSave.Count * (UserDef.Frequency/2); i++)
                    {
                        int n = i / (UserDef.Frequency/2);
                        matdata[save_index, i] = UserDef.xToSave.ElementAt(n);
                    }



                    for (int i = 0; i < UserDef.dataToSave.ElementAt(channelNum.ElementAt(0)).Count - UserDef.xToSave.Count * (UserDef.Frequency/2); i++)
                    {
                        matdata[save_index, i + UserDef.xToSave.Count * (UserDef.Frequency/2)] = UserDef.xToSave.ElementAt(UserDef.xToSave.Count - 1);

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
                if (csvSave) {
                    StreamWriter swt = new StreamWriter("D:\\CSVData\\" + DateTime.Now.ToString("MM -dd-H-mm-ss_") + ".csv");
                    for (int i = 0; i < matdata.GetLength(1); i++) {
                        for (int j = 0; j < matdata.GetLength(0); j++) {

                            swt.Write(matdata[j, i].ToString()+",");
                        
                        }

                        swt.Write("\r\n");
                    }
                    swt.Flush();
                    swt.Close();
                }
             
                else {
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
                                List<Double> y = data.ElementAt(i);
                                y.Clear();
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

                            for (int k = 0; k < channelNum.Count; k++) {
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
                                List<Double> y = data.ElementAt(n);      //当前的通道                        
                                //displayFrame 是需要展示的数据帧
                                temp_value = ((NET2991A.buffer[displayFrame, n, i] - 32768) * NET2991A.fPerLsb / 1000);
                                y.Add(temp_value + 0.3 * yindex);
                                if (UserDef.flagRecord && UserDef.NowRes!=0) {
                                    
                                    UserDef.dataToSave.ElementAt(n).Add(temp_value);
                                   
                                }
                                if (UserDef.timeOut) {
                                    UserDef.dataCompute.ElementAt(n).Add(temp_value);
                                
                                }

                                
                                yindex++;
                            }

                            

                            count++;
                        }
                       /* if (channelNum.Count > 0)
                        {
                            List<Double> ym = data.ElementAt(channelNum.ElementAt(0));
                            if (ym.Count > 0)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    Console.WriteLine("first elelment:" + i + ":" + ym.ElementAt(i));
                                }

                            }
                        }*/
                        
                        // yindex = 0;
                        isChangeData = false;
                        wavePlotWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //displayLines();
                            playSelectLines();
                          
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
                List<Double> y1 = UserDef.freq.ElementAt(i);
                y1.Clear();
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
                double[,] matdata = new double[channelNum.Count + 1, x.Count];
               
                for (var i = 0; i < x.Count; i++)
                {
                    matdata[0, i] = x.ElementAt(i);
                }
                int save_index = 1;
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
        
            //sph.HighlightClear();
            //var (x, y, index) = sph.HighlightPointNearest(mouseX, mouseY);
           // heatPlotWindow.Render();

     
        }
        //导出数据
        /* private void bt_export_Result_Click(object sender, RoutedEventArgs e)
         {
             String current_directory = Environment.CurrentDirectory;
             String[] para = current_directory.Split('\\');
             String file_name = para[0] + "\\" + para[1] + "\\" + "defectInfo.txt";
             if (File.Exists(file_name) == false)
             {
                 FileStream fs = File.Create(file_name);
                 fs.Close();
             }
             else
             {
                 File.Delete(file_name);
                 FileStream fs = File.Create(file_name);
                 fs.Close();
             }

             using (StreamWriter sw = new StreamWriter(file_name, true))
             {

                 int dim1 = peakMapValue.GetLength(0);
                 int dim2 = peakMapValue.GetLength(1);
                 for (int i = 0; i < dim1; i++)
                 {
                     int channel = i / 4;
                     List<double> rs = new List<double>();
                     for (int k = 0; k < dim2; k++)
                     {
                         rs.Add((double)peakMapValue.GetValue(i, k));
                     }
                     double Max_Value = rs.Max();
                     double Min_Value = rs.Min();
                     if (Max_Value - Min_Value < 1)
                     {
                         sw.WriteLine("通道" + channel + "-" + i % 4 + "无缺陷");
                         sw.Flush();
                     }
                     else
                     {
                         List<int> cor = new List<int>();
                         for (int j = 0; j < rs.Count - 1; j++)
                         {
                             if (rs[j].Equals(Max_Value))
                             {
                                 cor.Add(j);
                             }
                         }

                         int count = 0;
                         for (int k = 0; k < cor.Count - 1; k++)
                         {
                             if ((cor[k] + 1) == cor[k + 1])
                             {
                                 count++;
                             }
                             else
                             {

                                 sw.Write("通道" + channel + "-" + i % 4+ ":坐标>" + (cor[k - count + 1]) + "," + cor[k] + "脉冲宽度>" + ((count)*2.5) + "\n");
                                 sw.Flush();
                                 count = 0;
                             }
                             if (k == cor.Count - 2)
                             {

                                 sw.Write("通道" + channel + "-" + i % 4+ ":坐标>" + (cor[k - count + 1]) + "," + cor[k] + "脉冲宽度>" + ((count) * 2.5) + "\n");
                                 sw.Flush();
                                 break;
                             }
                         }

                     }
                 }

             }
         }*/
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
        private Double[] computeFFT(Double [] data) {
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

            return usedata;
        }



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
        
            
            
           

            
            for (int i = 0; i < channelNum.Count; i++) {
                List<double> y = data.ElementAt(channelNum.ElementAt(i));
                if (pointIndex.Count > 0)
                {
                    ydisplay.Clear();
                    foreach (int m in pointIndex)
                    {
                        ydisplay.Add(y.ElementAt(m));
                    }
                    if (ydisplay.Count > 0)
                    {

                        int lowf = 0;
                        int highf = ydisplay.Count/2-1;
                        if (textBox.Text != "0") {
                            String[] ts = textBox.Text.Split('-');
                            try {
                                lowf = Convert.ToInt32(ts[0]);
                                highf = Convert.ToInt32(ts[1]);
                            
                            } catch (Exception) { }
                        
                        }

                        wavePlotWindow.plt.PlotSignal(ydisplay.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", channelNum.ElementAt(i)));
                        if (freqflag)
                        {

                            double[] ds = new double[ydisplay.Count];
                            for (int j = 0; j < ds.Length; j++)
                            {
                                

                                ds[j] = y.ElementAt(j) - 0.3 * i;
                            }

                            Double[] usedata = computeFFT(ds);

                            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                            
                            for (int c = 0; c < lis.Count; c++) {
                                if (channelNum.ElementAt(i) == 12) {
                                    lis[c] = lis[c] +i;

                                }else
                                lis[c] = lis[c];
                            
                            }

                            //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                            heatPlotWindow.plt.PlotSignal(lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));

                        } else if (computeHigh) {

                            if (channelNum.ElementAt(i) == 15)
                            {
                                double[] ds = new double[ydisplay.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {
                                    ds[j] = y.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds);


                                //获取指定频段范围
                                List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                                lis.Sort();
     
                                double max1 = lis[lis.Count - 1];
                                double max2 = lis[lis.Count - 2];

                                int index1 = usedata.ToList<Double>().IndexOf(max1);
                                int index2 = usedata.ToList<Double>().IndexOf(max2);

                                UserDef.freq11.ElementAt(0).Add(index1);
                                UserDef.freq11.ElementAt(1).Add(index2);

                                if (freq11.ElementAt(0).Count >= 2000)
                                {
                                    freq11.ElementAt(0).RemoveAt(0);
                                    freq11.ElementAt(1).RemoveAt(0);
                                }

                                heatPlotWindow.plt.PlotSignal(freq11.ElementAt(0).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                                heatPlotWindow.plt.PlotSignal(freq11.ElementAt(1).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));

                            }
                            else
                            {
                                double[] ds = new double[ydisplay.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {
                                    ds[j] = y.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds);


                                //获取指定频段范围
                                List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);



                                double max = lis.Max();
                                double index = (double)(lis.IndexOf(max));
                                freq.ElementAt(channelNum.ElementAt(i)).Add(index + 0.3 * i);


                                if (freq.ElementAt(channelNum.ElementAt(i)).Count >= 2000)
                                {
                                    freq.ElementAt(channelNum.ElementAt(i)).RemoveAt(0);
                                }
                                //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                                heatPlotWindow.plt.PlotSignal(freq.ElementAt(channelNum.ElementAt(i)).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                            }
                            }
                        else if (computeMax)
                        {
                            double[] ds = new double[ydisplay.Count];
                            for (int j = 0; j < ds.Length; j++)
                            {
                                ds[j] = y.ElementAt(j) - 0.3 * i;
                            }

                            Double[] usedata = computeFFT(ds);
                            //获取指定频段范围
                            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                            double max = lis.Max();
                            UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).Add(max);
                            if (UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).Count > 2000) {
                                UserDef.MAXVALUE.ElementAt(channelNum.ElementAt(i)).RemoveAt(0);
                            }
                            heatPlotWindow.plt.PlotSignal(MAXVALUE.ElementAt(channelNum.ElementAt(i)).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                        }
                        else if (dwtflag)
                        {
                            Signal<Double> sig = new Signal<double>(ydisplay.ToArray());
                            DiscreteWaveletTransform rs = DiscreteWaveletTransform.Estimate(sig, Wavelets.Daubechies(2), new ZeroPadding<Double>());
                            DiscreteWaveletTransform rs1 = rs.EstimateMultiscale(new ZeroPadding<Double>(), 4);
                            heatPlotWindow.plt.PlotSignal(rs1.Approximation.ToArray(), (int)UserDef.Frequency / 2, startTime, label: String.Format("通道:{0}", channelNum.ElementAt(i)));

                        }


                    }
                }

                else
                {
                    if (y.Count > 0)
                    {

                        int lowf = 0;
                        int highf = y.Count/2-1;
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

                        wavePlotWindow.plt.PlotSignal(y.ToArray(), UserDef.Frequency, startTime, label: String.Format("通道 {0}", channelNum.ElementAt(i)));
                        if (freqflag)
                        {

                            //double[] res = Filter(y.ToArray(), i);

                            double[] ds = new double[y.Count];
                            for (int j = 0; j < ds.Length; j++) {
                                ds[j] = y.ElementAt(j) - 0.3 * i;
                            }

                            Double[] usedata = computeFFT(ds);
                            List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                            
                            for (int c = 0; c < lis.Count; c++)
                            {
                                if (channelNum.ElementAt(i) == 12)
                                {
                                    lis[c] = lis[c] + i;

                                }
                                else
                                    lis[c] = lis[c];

                            }
                            //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                            heatPlotWindow.plt.PlotSignal(lis.ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));


                        } else if (computeHigh) {

                            if (channelNum.ElementAt(i) == 15)
                            {
                                double[] ds = new double[y.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {
                                    ds[j] = y.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds);


                                //获取指定频段范围
                                List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);
                                lis.Sort();
                   

                                double max1 = lis[lis.Count - 1];
                                double max2 = lis[lis.Count - 2];

                                int index1 = usedata.ToList<Double>().IndexOf(max1);
                                int index2 = usedata.ToList<Double>().IndexOf(max2);

                                UserDef.freq11.ElementAt(0).Add(index1);
                                UserDef.freq11.ElementAt(1).Add(index2);

                                if (freq11.ElementAt(0).Count >= 2000)
                                {
                                    freq11.ElementAt(0).RemoveAt(0);
                                    freq11.ElementAt(1).RemoveAt(0);
                                }

                                heatPlotWindow.plt.PlotSignal(freq11.ElementAt(0).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                                heatPlotWindow.plt.PlotSignal(freq11.ElementAt(1).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));


                            }
                            else
                            {
                                double[] ds = new double[y.Count];
                                for (int j = 0; j < ds.Length; j++)
                                {
                                    ds[j] = y.ElementAt(j) - 0.3 * i;
                                }

                                Double[] usedata = computeFFT(ds);

                                List<Double> lis = usedata.ToList<Double>().GetRange(lowf, highf);

                                double max = lis.Max();
                                double index = (double)(lis.IndexOf(max));

                                //(double)index/ (Double)UserDef.Frequency
                                freq.ElementAt(channelNum.ElementAt(i)).Add(index + 0.3 * i);

                                if (freq.ElementAt(channelNum.ElementAt(i)).Count >= 2000)
                                {
                                    freq.ElementAt(channelNum.ElementAt(i)).RemoveAt(0);
                                }

                                //freq.ElementAt(channelNum.ElementAt(i)).ToArray()
                                heatPlotWindow.plt.PlotSignal(freq.ElementAt(channelNum.ElementAt(i)).ToArray(), label: String.Format("通道:{0}", channelNum.ElementAt(i)));
                            }
                        }
                        else if (computeMax)
                        {
                            double[] ds = new double[y.Count];
                            for (int j = 0; j < ds.Length; j++)
                            {
                                ds[j] = y.ElementAt(j) - 0.3 * i;
                            }

                            Double[] usedata = computeFFT(ds);
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
                        else if (dwtflag)
                        {
                            Signal<Double> sig = new Signal<double>(y.ToArray());
                            DiscreteWaveletTransform rs = DiscreteWaveletTransform.Estimate(sig, Wavelets.Daubechies(2), new ZeroPadding<Double>());
                            DiscreteWaveletTransform rs1 = rs.EstimateMultiscale(new ZeroPadding<Double>(), 4);
                            heatPlotWindow.plt.PlotSignal(rs1.Approximation.ToArray(), (int)UserDef.Frequency / 2, startTime, label: String.Format("通道:{0}", channelNum.ElementAt(i)));

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
            computeHigh = false;
        }

        private void bt_display_mode_Click(object sender, RoutedEventArgs e)
        {
            
            if (freqflag == false)
            {
                freqflag = true;
            }
            else
            {

                freqflag = false;
            }
            /*Thread thread = new Thread(new ThreadStart(computeFFT));
            thread.Start();*/


            /*if (heatDiplay.IsVisible)
            {
                heatDiplay.Visibility = Visibility.Hidden;
                heatPlotWindow.Visibility = Visibility.Visible;
                bt_display_mode.Content = "C扫图";
            } else
            {
                heatDiplay.Visibility = Visibility.Visible;
                heatPlotWindow.Visibility = Visibility.Hidden;
                bt_display_mode.Content = "C曲线";
            }*/
            //int len = data.ElementAt(channelNum.ElementAt(0)).Count;
           /* if (1 > 0)
            {
       

                
                double[] s = new double[463201];
                double[] s1 = new double[463202];
                for (int i = 0; i < 463202; i++)
                {
                    if(i<s.Length)
                    s[i] = rand.NextDouble();
                    if(i<s1.Length)
                    s1[i] = rand.NextDouble();

                }

              
                heatPlotWindow.plt.Clear();

                double[] x_axis = new double[s.Length];
                double[] x_axis1 = new double[s1.Length];
                for (int j = 0; j < x_axis1.Length; j++)
                {   
                    if(j<x_axis.Length)
                    x_axis[j] = j * (double)UserDef.Frequency / (double)s.Length;
                    if(j<x_axis1.Length)
                    x_axis1[j] = j * (double)UserDef.Frequency / (double)s1.Length;

                }
                heatPlotWindow.plt.PlotSignalXY(x_axis, s);
                heatPlotWindow.plt.PlotSignalXY(x_axis1, s1);
                heatPlotWindow.plt.Legend();
                heatPlotWindow.Render();

            }*/
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
            if (dwtflag == false)
            {
                dwtflag = true;
            }
            else {
                dwtflag = false;
            
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
        // motionPlatform.move(0, -10000, 0);

        // motionPlatform.move(0, 0, 6000);
        // motionPlatform.move(0, 100000, 0);
        //  motionPlatform.move(0, 0, -6000);
        /* AxZOLIXSC300Lib.AxZolixSC300 sc300 = new AxZOLIXSC300Lib.AxZolixSC300();

         ((System.ComponentModel.ISupportInitialize)(sc300)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(sc300)).EndInit();
         sc300.ComPort = 3;
         if (sc300.Open())
         {
             Console.WriteLine("connected");

         } else
         {
             Console.WriteLine("cannot conneted");
         }*/

        // }
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
