using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace wtf
{
    class SC300
    {
        private SerialPort sp = new SerialPort();
        private volatile int x, y, z;
        private  const int x_zero=-3754, y_zero=-337963, z_zero=4817;
        public SC300()
        {
            try
            {
                sp.PortName = "COM3";
                sp.BaudRate = 19200;
                sp.DataBits = 8;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
               
                sp.DataReceived += new SerialDataReceivedEventHandler(port_DataRecieved);
                sp.Open();
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
            } catch(Exception e)
            {
                MessageBox.Show("打开串口COM3失败");
            }
           
            // sendcmd("VX,30000");
            // sendcmd("VY,30000");
            //  sendcmd("VZ,30000");
            //  sendcmd("HX");由于外加了东西，所以无法归零到机械位置
            //  sendcmd("HY");
            //  sendcmd("HZ");
           //sendcmd("+Z, 10");
        }
        public int[] getCurrentPostion()
        {
            sendcmd("?X");
            Thread.Sleep(100);
            sendcmd("?Y");
            Thread.Sleep(100);
            sendcmd("?Z");
            Thread.Sleep(100);

            return new int[3] { x, y, z };
        }
        private void ReadDataFun()
        {
            UTF8Encoding uTF8 = new UTF8Encoding();
            Byte[] readData = new Byte[sp.BytesToRead];
            sp.Read(readData, 0, readData.Length);
            String recieved = uTF8.GetString(readData);
            string[] rArr = recieved.Split(',');
            try
            {
                if (rArr.Length > 1)
                {
                    switch (rArr[0][1])
                    {
                        case 'X':
                            x = Convert.ToInt32(rArr[1]);
                            break;
                        case 'Y':
                            y = Convert.ToInt32(rArr[1]);
                            break;
                        case 'Z':
                            z = Convert.ToInt32(rArr[1]);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            Console.WriteLine(recieved);
        }
        private void port_DataRecieved(object sender, EventArgs e)
        {
            Thread.Sleep(10);
            Thread tDataRead = new Thread(ReadDataFun);
            tDataRead.Start();
        }
        public void sendcmd(string cmd)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] data = utf8.GetBytes(cmd + "\r");
            sp.Write(data, 0, data.Length);

        }

        public async Task move2Zero()
        {
            getCurrentPostion();
            await move( x_zero-x,  y_zero - y,  z_zero - z);
        }
        public async Task move(int x= 0, int y=0, int z = 0)
        {
            if (sp.IsOpen)
            {
                sendcmd((x>0?"+":"-") + "X,"+Math.Abs( x));
                if(x != 0)
                {
                    await Task.Delay(2000 + Math.Abs(x / 30));
                }
                
                sendcmd((y > 0 ? "+" : "-") + "Y," + Math.Abs(y));
                if (y != 0)
                    await Task.Delay(2000 + Math.Abs(y/30));
                sendcmd((z > 0 ? "+" : "-") + "Z," + Math.Abs(z));
                if (z != 0)
                    await Task.Delay(2000 + Math.Abs(z/30));
            } 

        }

        public void moveSingle(int direction, int value)
        {
            String cmd = (value > 0 ? "+" : "-");
            switch (direction)
            {
                case 0:
                    cmd += "X,";
                    break;
                case 1:
                    cmd += "Y,";
                    break;
                case 2:
                    cmd += "Z,";
                    break;
                default:
                    cmd = null;
                    break;
            }
            if(cmd == null)
            {
                return;
            }
            cmd += +Math.Abs(value);
            sendcmd(cmd);
        }


    }
}
