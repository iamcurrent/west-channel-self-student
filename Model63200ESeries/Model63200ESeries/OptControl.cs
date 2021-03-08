using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.IO.Ports;
namespace Model63200ESeries
{



    public partial class OptControl : Form
    {
        public static double returnVaslue = 0;
        private List<double> resData = new List<double>();
        private  MessageBasedSession mbSession;
        private SerialPort serial;
        private int csIndex = 0;
        private List<double[]> csArr = new List<double[]>() { new double[] {2,16,32,16 }, new double[] { 2, 32, 64, 32 }, new double[] { 2, 64, 128, 64  }, 
        new double[] { 2, 128, 256, 128 }
        ,new double[] {2, 256, 500, 256 },new double[] {2,500 }};

        private double[] powerArr = { 2, 4, 8, 16, 32, 64, 128, 256, 500 };
        //private double[] powerArr = { 2,  500 };

        private WebSocketServer service;
        private static WebSocket socket;
        private System.Threading.Timer timer;
        private int arrIndex = 0;
        private int LoopIndex = 0;
        private int loop = 0;
        private bool randflag = false;
        public OptControl()
        {
            
            InitializeComponent();
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            
            
            //throw new NotImplementedException();
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {

            if (randflag == true)
            {
                if (LoopIndex < loop)
                {
                    Random random = new Random();
                    double value = 2 + random.NextDouble() * 10;
                    Action<String> syn = delegate (String s1) { richTextBox1.AppendText(s1); richTextBox1.ScrollToCaret(); };
                    richTextBox1.Invoke(syn, new object[] { "当前发送值:" + value.ToString() + "\n" });
                    serial.Write("RES:DC " + value.ToString() + "\n");
                    socket.Send(value.ToString());
                    LoopIndex++;

                }

            }
            else if (LoopIndex < loop*powerArr.Length)
            {
                try
                {
                  
                  
                        //mbSession.RawIO.Write("RES:STAT:L1 " + powerArr.ElementAt(arrIndex).ToString());
                        String s = powerArr.ElementAt(arrIndex).ToString() + "\n";
                        Action<String> syn = delegate (String s1) { richTextBox1.AppendText(s1); richTextBox1.ScrollToCaret(); };
                        richTextBox1.Invoke(syn, new object[] { "当前发送值:" + s });
                        serial.Write("RES:DC " + s);
                        socket.Send(powerArr.ElementAt(arrIndex).ToString());
                    
                }
                catch (Exception)
                {
                }
                arrIndex++;
                if (arrIndex >= powerArr.Count())
                {
                    arrIndex = 0;
                    /*Action<String> syn = delegate (String s) { richTextBox1.AppendText(s); };
                    richTextBox1.Invoke(syn, new object[] { (LoopIndex/powerArr.Length).ToString()+"\n" });*/
                }

                LoopIndex++;
            }
         
         

        }

        public List<double> getValue() {

            return this.resData;
        }

        public void setMbSession(MessageBasedSession mbSession) {

            this.mbSession = mbSession;
            try {
                this.mbSession.RawIO.Write("RES:STAT:L1?");
                String msg = this.mbSession.RawIO.ReadString();
                returnVaslue = Convert.ToDouble(msg);

                socket = new WebSocket("ws://192.168.171.1:8089");
                socket.OnMessage += Socket_OnMessage;
                socket.OnOpen += Socket_OnOpen;
                socket.Connect();
                if (socket.IsAlive) {
                    richTextBox1.AppendText("连接服务器成功\n");
                
                }

            } catch (Exception) { }
            
            
        }

        public void setSerial(SerialPort serial) {
            this.serial = serial;
            serial.Write("RES:DC?");
            String s = serial.ReadExisting();
            try
            {
                returnVaslue = 50;
                socket = new WebSocket("ws://192.168.171.1:8089");
                socket.OnMessage += Socket_OnMessage;
                socket.OnOpen += Socket_OnOpen;
                socket.Connect();
                if (socket.IsAlive)
                {
                    richTextBox1.AppendText("连接服务器成功\n");

                }

            }
            catch (Exception) { }
        }



        //反向
        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                randflag = false;
                loop = Convert.ToInt32(LoopNum.Text);
                LoopIndex = 0;
                

                double start =    Convert.ToDouble(startP.Text);
                double end   =    Convert.ToDouble(endP.Text);
                double interal =     Convert.ToDouble(inter.Text);
                int inter_time = Convert.ToInt32(s_inter.Text);

                int pointNum = (int)((start - end) / interal);
                double[] arr = new double[pointNum];
                for (int i = 0; i < pointNum; i++) {
                    arr[i] = start + i * interal;
                }
                powerArr = arr;
                socket.Send(returnVaslue.ToString());

                /*if (start > end &&end >=2.4) {
                    double start_rc = start;

                    while (start_rc >= end)
                    {
                        //mbSession.RawIO.Write("RES:STAT:L1 " + start_rc.ToString());
                        serial.Write("RES:DC "+start_rc.ToString()+"\n");

                        Thread.Sleep(inter_time);
                        start_rc -= interal;
                    }

                }*/

            }
            catch (Exception m) {
                
                MessageBox.Show(m.Message, "提示");
                
            }
            
        }

        //正向
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                randflag = false;
                loop = Convert.ToInt32(LoopNum.Text);
                LoopIndex = 0;

                double start = Convert.ToDouble(startP.Text);
                double end = Convert.ToDouble(endP.Text);
                double interal = Convert.ToDouble(inter.Text);
                int inter_time = Convert.ToInt32(s_inter.Text);

                int pointNum = (int)((end - start) / interal);
                double[] arr = new double[pointNum];
                for (int i = 0; i < pointNum; i++)
                {
                    arr[i] = start + i * interal;
                }
                powerArr = arr;

                socket.Send(returnVaslue.ToString());

            }
            catch (Exception m)
            {
            
                MessageBox.Show(m.Message, "提示");
            }
            
        }

        private int currentIndex = 0;

        private void btn_chanage2_Click(object sender, EventArgs e)
        {
            try {

                randflag = false;
                LoopIndex = 0;
                socket.Send(returnVaslue.ToString());
                int num = Convert.ToInt32(LoopNum.Text);
                loop = num;
         

            } catch (Exception) { }
           
        }

        private void btn_changecs_Click(object sender, EventArgs e)
        {
            try {
                randflag = false;
                int num = Convert.ToInt32(LoopNum.Text);
                loop = num;
                LoopIndex = 0;
                if (csIndex == 0)
                {
                    
                    powerArr = csArr[csIndex];
                    
                    socket.Send(returnVaslue.ToString());
                    
                    csIndex++;
                }
                else if (csIndex == 1)
                {
                  
                    powerArr = csArr[csIndex];
                    
                    socket.Send(returnVaslue.ToString());
                    

                    csIndex++;
                }
                else if (csIndex == 2)
                {
                    powerArr = csArr[csIndex];

                    socket.Send(returnVaslue.ToString());
                    
                    csIndex++;
                }
                else if (csIndex == 3)
                {

                    powerArr = csArr[csIndex];
                  
                    socket.Send(returnVaslue.ToString());
                    
                    csIndex++;

                }
                else if (csIndex == 4)
                {

                    powerArr = csArr[csIndex];

                    socket.Send(returnVaslue.ToString());
                    
                    csIndex++;
                }
                else {

                    powerArr = csArr[csIndex];
                    
                    socket.Send(returnVaslue.ToString());
                    
                    csIndex = 0;
                }

              


            } catch (Exception) { }

        }

        public void TimerOut(object o) {

            List<int> t = o as List<int>;

            if (t[2] == 20)
            {
                timer.Dispose();

            }
            else {

                try {

                    mbSession.RawIO.Write("RES:STAT:L1 " + csArr.ElementAt(t[0]).ElementAt(t[1]).ToString());

                } catch (Exception) { }
            }
        
        }

        private void OptControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (socket!=null&&socket.IsAlive) {
                socket.Close();            
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void AnyVal_Click(object sender, EventArgs e)
        {
            LoopIndex = 0;
            loop = Convert.ToInt32(LoopNum.Text);
            randflag = true;
            socket.Send(returnVaslue.ToString());
        }

        private void ucNumTextBox1_NumChanged(object sender, EventArgs e)
        {

            Console.WriteLine(ucNumTextBox1.Num);
            serial.Write("RES:DC "+ucNumTextBox1.Num.ToString()+"\n");
        }
    }


}
