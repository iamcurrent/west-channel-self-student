using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using NationalInstruments.Visa;
using System.Windows;
using System.IO;
using System.Threading;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using FFT;
namespace Model63200ESeries
{

    public partial class Form1 : Form
    {

        private SerialPort serial = new SerialPort();
        private MessageBasedSession mbSession;
        
        private String device_name = "";
        public Form1()
        {
            InitializeComponent();
            InitPort();
            
        }


        private void InitPort()
        {
            AddInstruction();
            button3.Enabled = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            String[] portName = SerialPort.GetPortNames();
            foreach (String s in portName)
            {
                comboBox1.Items.Add(s);
            }

            comboBox1.SelectedItem = portName[0];
            
        }

        private void AddInstruction()
        {
            String filePath = Directory.GetCurrentDirectory();
           
            //D:\Model63200ESeries\Model63200ESeries\bin\x64\Debug
            // D:\Model63200ESeries\Model63200ESeries\Instructions.txt
            
            StreamReader reader = new StreamReader("D:\\Model63200ESeries\\Model63200ESeries\\Instructions.txt");
            String line = "";
            while((line = reader.ReadLine())!= ""){
                richTextBox3.AppendText(line+"\n");
            }
            reader.Close();
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            serial.Close();
            if (mbSession!=null&&!mbSession.IsDisposed)
            {
                mbSession.Dispose();
            }
            this.Dispose();
        }

        //选择设备之后进行打开
        private void button2_Click(object sender, EventArgs e)
        {
            SelectResource f = new SelectResource();
            f.StartPosition = FormStartPosition.CenterParent;
            
            DialogResult r = f.ShowDialog(this);
            if(r == DialogResult.OK)
            {
                device_name = f.ResourceName;
                Cursor.Current = Cursors.WaitCursor;
                using (var rmSession = new ResourceManager())
                {
                    try
                    {
                        mbSession = (MessageBasedSession)rmSession.Open(f.ResourceName);
                        button2.Enabled = false;
                        button3.Enabled = true;
                    }
                    catch (InvalidCastException)
                    {
                        System.Windows.Forms.MessageBox.Show("Resource selected must be a message-based session");
                    }
                    catch (Exception exp)
                    {
                        System.Windows.Forms.MessageBox.Show(exp.Message);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!mbSession.IsDisposed)
            {
                mbSession.Dispose();
                button2.Enabled = true;
                button3.Enabled = false;
            }
        }



        private string ReplaceCommonEscapeSequences(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private string InsertCommonEscapeSequences(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }


        //使用查询命令
        private void button5_Click(object sender, EventArgs e)
        {
            
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string textToWrite = ReplaceCommonEscapeSequences(textBox1.Text.ToString());
                mbSession.RawIO.Write(textToWrite);
                richTextBox2.AppendText(mbSession.RawIO.ReadString());
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        //对设备进行读命令
        private void button1_Click(object sender, EventArgs e)
        {
            
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                richTextBox2.AppendText(InsertCommonEscapeSequences(mbSession.RawIO.ReadString())+"\n");
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        //对设备进行写
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                //执行组合命令
                if (checkBox1.Checked)
                {
                    String str = richTextBox4.Text;
                    String[] instructions = str.Split('\n');
                    foreach(String s in instructions)
                    {
                        if (s != "")
                        {
                            string textToWrite = ReplaceCommonEscapeSequences(s);
                            mbSession.RawIO.Write(textToWrite);
                            richTextBox2.AppendText("对设备写" + s + "操作执行成功\n");
                        }
                    }
                    
                }
                else
                {
                    string textToWrite = ReplaceCommonEscapeSequences(textBox1.Text.ToString());
                    mbSession.RawIO.Write(textToWrite);
                    richTextBox2.AppendText("对设备写" + textBox1.Text.ToString() + "操作执行成功\n");
                }
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.Message);
            }
        }

        private void Clear1_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
        }

        private void Clear2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void OpenPort_Click(object sender, EventArgs e)
        {
            String selectPort = comboBox1.SelectedItem.ToString();
            try
            {
                serial.PortName = selectPort;
                serial.DataBits = 8;
                serial.BaudRate = 57600;
                serial.Parity = Parity.None;
                serial.StopBits = StopBits.One;
                //serial.DataReceived += Serial_DataReceived;
                serial.ErrorReceived += Serial_ErrorReceived;
                
                //serial.DtrEnable = true;
                //serial.RtsEnable = true;
                
                serial.Open();
                
                if (serial.IsOpen)
                {
                    richTextBox1.AppendText("开启串口成功\n");
                    
                    /*ShowVCData form = new ShowVCData();
                    form.setSerial(serial);
                    form.setKind(false);
                    form.Show();*/
                   
                    

                }




            }
            catch (Exception)
            {
                richTextBox1.AppendText("开启串口失败\n");
            }
        }

        private void Serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            richTextBox1.AppendText(serial.PortName+"串口出现异常!\n");
        }

        private Byte[] buffer = new Byte[4096];

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            String msg = serial.ReadExisting();
            Action<String> ays = delegate(String s){ richTextBox1.AppendText(s);if (richTextBox1.TextLength >= 1000) { richTextBox1.Clear(); }; };
            richTextBox1.Invoke(ays, new object[] { msg });
            


        }



        private void ClosePort_Click(object sender, EventArgs e)
        {
            if (serial.IsOpen)
            {
                serial.Close();
                richTextBox1.AppendText("串口关闭\n");
            }
        }

        private void SendData_Click(object sender, EventArgs e)
        {
            String data = richTextBox5.Text.ToString();
            data = data + "\n";
            Console.WriteLine(data);
            if (serial.IsOpen)
            {
                serial.Write(data);
            }
        }

        private void getCV_btn_Click(object sender, EventArgs e)
        {
            /*if (mbSession == null)
            {
                System.Windows.Forms.MessageBox.Show("当前没有连接到设备","提示信息",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else
            {
                ShowVCData form = new ShowVCData();
                form.SetmbSession(mbSession);
                form.setKind(true);
                form.Show();

            }*/

            double[] s = new double[500000];
            for (int i = 0; i < 500000; i++)
            {

                s[i] = new Random().NextDouble();

            }

            Class1 class1 = new Class1();
            MWNumericArray aaa = (MWNumericArray)s;
            MWNumericArray res = (MWNumericArray)class1.FFT(aaa);
            Console.WriteLine("OKKIKKKK");

        }

        private void LoadR_Click(object sender, EventArgs e)
        {
            if (serial != null)
            {
                try {
                    serial.Write("LOAD ON\n");
                    richTextBox1.AppendText("加载负载成功!\n");
                } catch (Exception exp) {
                    System.Windows.Forms.MessageBox.Show(exp.Message);
                }
                
            }
        }

        private void CloseR_Click(object sender, EventArgs e)
        {
            if (serial != null)
            {
                try
                {
                    serial.Write("LOAD OFF\n");
                    richTextBox1.AppendText("关闭负载成功!\n");
                }
                catch (Exception exp)
                {
                    System.Windows.Forms.MessageBox.Show(exp.Message);
                }

            }
        }

        private void RST_Click(object sender, EventArgs e)
        {
            if (mbSession != null)
            {
                try
                {
                    mbSession.RawIO.Write("*RST");
                    richTextBox2.AppendText("已清空设置，请设置参数!\n");
                }
                catch (Exception exp)
                {
                    System.Windows.Forms.MessageBox.Show(exp.Message);
                }

            }
        }

        private void NowMode_Click(object sender, EventArgs e)
        {
            if (serial != null)
            {
                try
                {
                    serial.Write("SYSTem:SETup:MODE?\nMODE?\n");
                    String s = serial.ReadLine();
                    richTextBox2.AppendText(s+"\n");
                }
                catch (Exception exp)
                {
                    System.Windows.Forms.MessageBox.Show(exp.Message);
                }

            }
        }

        private void DynamicD()
        {
            double start_rc = 6;


            OptControl optControl = new OptControl();
           // optControl.setMbSession(this.mbSession);
            optControl.setSerial(serial);
            optControl.StartPosition = FormStartPosition.CenterParent;
            optControl.ShowDialog();

            
        }

        

        private void StartD_Click(object sender, EventArgs e)
        {

            Thread thread = new Thread(new ThreadStart(DynamicD));
            thread.Start();
        }

        private void refresh_btn_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            String[] portName = SerialPort.GetPortNames();
            foreach (String name in portName) {
                comboBox1.Items.Add(name);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void btn_dc_Click(object sender, EventArgs e)
        {
            try {

                if (serial != null) {
                    serial.Write("SYSTem:SETup:MODE DC\n");
                
                }
            
            } catch (Exception) { }
        }

        private void btn_ac_Click(object sender, EventArgs e)
        {
            try
            {

                if (serial != null)
                {
                    serial.Write("SYSTem:SETup:MODE AC\n");

                }

            }
            catch (Exception) { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serial != null)
            {
                try {
                    //电压
                    serial.Write("MEAS:POW?\nMEAS:CURR?\n");
                    Thread.Sleep(10);
                    String s1 = serial.ReadLine();

                    serial.Write("RES:DC?\nMEAS:VOLT?\n");
                    Thread.Sleep(10);
                    String s2 = serial.ReadLine();


                    labv.Text = s2.Split(';')[1];
                    labi.Text = s1.Split(';')[1];
                    labp.Text = s1.Split(';')[0];
                    labr.Text = s2.Split(';')[0];

                } catch (Exception) { }
            
            
            }
        }

        private void labi_Click(object sender, EventArgs e)
        {

        }
    }

}
