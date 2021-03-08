using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using NationalInstruments;
using NationalInstruments.Visa;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System.Threading;
using Ivi.Visa;
using System.IO.Ports;
using System.Windows.Media;
using System.Drawing.Drawing2D;
using DocumentFormat.OpenXml.InkML;
using iTextSharp.text.pdf.parser;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Matlab;

namespace Model63200ESeries
{
    public partial class ShowVCData : Form
    {
        private MessageBasedSession mbSession=null;
        private SerialPort serial;
        private LinearAxis _dataxAxis;
        private DateTimeAxis _dateAxis;//X轴
        private LinearAxis _valueAxis;//Y轴
        private PlotModel _myPlotModel;
        private IVisaAsyncResult asyncHandle = null;
        private bool flag = true;
        private bool kind;
        public ShowVCData()
        {
            InitializeComponent();
     
        }

        private void InitUI()
        {
            //对应的数据模型
            _myPlotModel = new PlotModel()
            {
                Title = "curr & volt",
                LegendTitle = "Legend",
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.Beige),
                LegendBorder = OxyColors.Black
            };
            //X轴

            _dateAxis = new DateTimeAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 80,
                IsZoomEnabled = false,
                IsPanEnabled = true,
                StringFormat = "ss"
            };


            _myPlotModel.Axes.Add(_dateAxis);

            //Y轴
            _valueAxis = new LinearAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 20,
                Angle = 60,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Maximum = 3,
                Minimum = -1
            };
            _myPlotModel.Axes.Add(_valueAxis);

            /*
            //添加标注线，温度上下限和湿度上下限
            var lineTempMaxAnnotation = new OxyPlot.Annotations.LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid,
                Y = 10,
                Text = "curr MAX:10"
            };
            _myPlotModel.Annotations.Add(lineTempMaxAnnotation);

            var lineTempMinAnnotation = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Y = 30,
                Text = "curr Min:30",
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid
            };
            _myPlotModel.Annotations.Add(lineTempMinAnnotation);

            var lineHumiMaxAnnotation = new OxyPlot.Annotations.LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid,
                //lineMaxAnnotation.MaximumX = 0.8;
                Y = 75,
                Text = "volt MAX:75"
            };
            _myPlotModel.Annotations.Add(lineHumiMaxAnnotation);

            var lineHumiMinAnnotation = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                Y = 35,
                Text = "volt Min:35",
                Color = OxyColors.Red,
                LineStyle = LineStyle.Solid
            };
            _myPlotModel.Annotations.Add(lineHumiMinAnnotation);
            */
            //添加两条曲线
            var series = new LineSeries()
            {
                Color = OxyColors.Green,
                //StrokeThickness = 2,
                //MarkerSize = 3,
                //MarkerStroke = OxyColors.DarkGreen,
                //MarkerType = MarkerType.Diamond,
                Title = "curr",

            };
            _myPlotModel.Series.Add(series);
            series = new LineSeries()
            {
                Color = OxyColors.Blue,
                //StrokeThickness = 2,
                //MarkerSize = 3,
                //MarkerStroke = OxyColors.BlueViolet,
                //MarkerType = MarkerType.Star,
                Title = "volt",

            };
            _myPlotModel.Series.Add(series);

            plotView1.Model = _myPlotModel;

            plotView1.Model = _myPlotModel;
            Thread thread = new Thread(new ThreadStart(ShowCurves));
            thread.Start();
        }



        private void InitUI1()
        {
            _myPlotModel = new PlotModel()
            {
                Title = "Temp & Humi",
                LegendTitle = "Legend",
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.Beige),
                LegendBorder = OxyColors.Black
            };
            //X轴

            _dateAxis = new DateTimeAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 80,
                IsZoomEnabled = false,
                IsPanEnabled = true,
                StringFormat = "ss"
            };


            _myPlotModel.Axes.Add(_dateAxis);

            //Y轴
            _valueAxis = new LinearAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IntervalLength = 20,
                Angle = 60,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Maximum = 10,
                Minimum = -1
            };
            _myPlotModel.Axes.Add(_valueAxis);

            //添加两条曲线
            var series = new LineSeries()
            {
                Color = OxyColors.Green,
                //StrokeThickness = 2,
                //MarkerSize = 3,
                //MarkerStroke = OxyColors.DarkGreen,
                //MarkerType = MarkerType.Diamond,
                Title = "Temp",

            };
            _myPlotModel.Series.Add(series);
            /*series = new LineSeries()
            {
                Color = OxyColors.Blue,
                //StrokeThickness = 2,
                //MarkerSize = 3,
                //MarkerStroke = OxyColors.BlueViolet,
                //MarkerType = MarkerType.Star,
                Title = "Humi",

            };
            _myPlotModel.Series.Add(series);*/

            plotView1.Model = _myPlotModel;


        }




        public void setKind(bool kind) {
            this.kind = kind;

            if (this.kind == true) {
                InitUI();
            }
            else
            {
                InitUI1();
                List<double> y1 = new List<double>();
                List<double> y2 = new List<double>();
                dataList.Add(y1);
                dataList.Add(y2);
                if (serial.IsOpen)
                {
                    serial.DataReceived += Serial_DataReceived;
                    Thread thread = new Thread(new ThreadStart(refresh_curve));
                    thread.Start();
                }
            }


        }
        


        private List<List<double>> dataList = new List<List<double>>();
        private bool ready = false;
        private double data1 = 0;
        private double data2 = 0;
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            String msg = serial.ReadLine();
            //String[] data = msg.Split(',');
            if (msg!="") {
                try
                {
                    data1 = Convert.ToDouble(msg) / 100;
                    
                }
                catch (Exception) { }
            }

            
            }


        private double minTemp = 0;
        private double maxTemp = 0;
        private double minHumi = 0;
        private double maxHumi = 0;

        
        private void refresh_curve()
        {

            minTemp = maxTemp = data1;
            
            while (flag) {

                if (data1 > maxTemp) {
                    maxTemp = data1;
                    richTextBox1.AppendText("最大温度"+((maxTemp-5)*20).ToString()+"\n");
                }

               

                var date = DateTime.Now;

                    _myPlotModel.Axes[0].Maximum = DateTimeAxis.ToDouble(date.AddMilliseconds(1));

                    var lineSer = plotView1.Model.Series[0] as LineSeries;
                    lineSer.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1));
                if (!checkBox1.Checked)
                {
                    if (lineSer.Points.Count > 250)
                    {
                        lineSer.Points.RemoveAt(0);
                    }

                }
                else {
                    if(dataList[0].Count<=50000)
                    dataList[0].Add(data1);
                }

               


                
                    _myPlotModel.InvalidatePlot(true);

                if (dataList[0].Count == 50000) {
                    flag = false;

                    var axis = plotView1.Model.Axes;
                    foreach (var o in axis)
                    {
                        o.IsPanEnabled = true;
                        o.IsZoomEnabled = true;
                    }
                }
                    ready = false;
                    Thread.Sleep(10);

                }

        }



        //异步写
        private void OnWriteComplete(IVisaAsyncResult result)
        {
            try
            {
                mbSession.RawIO.EndWrite(result);
            
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message,"提示");
            }
           
        }


        //异步读

        private void OnReadComplete(IVisaAsyncResult result)
        {
            try
            {
                string responseString = mbSession.RawIO.EndReadString(result);
                String[] resdata = responseString.Split(';');
                if (resdata.Length >= 2)
                {
                    double data1 = Convert.ToDouble(resdata[0]) / 10;
                    double data2 = Convert.ToDouble(resdata[1]) / 10;
                    var date = DateTime.Now;
                    _myPlotModel.Axes[0].Maximum = DateTimeAxis.ToDouble(date.AddSeconds(1));



                    var lineSer = plotView1.Model.Series[0] as LineSeries;
                    lineSer.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1));
                    if (!checkBox1.Checked)
                    {
                        if (lineSer.Points.Count > 250)
                        {
                            lineSer.Points.RemoveAt(0);
                        }
                    }

                    lineSer = plotView1.Model.Series[1] as LineSeries;
                    lineSer.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data2));
                    if (!checkBox1.Checked)
                    {
                        if (lineSer.Points.Count > 250)
                        {
                            lineSer.Points.RemoveAt(0);
                        }
                    }
                    _myPlotModel.InvalidatePlot(true);
                }

            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示");
            }
            
        }



        private void ShowCurves()
        {
            double pre_curr = 0;
            double pre_volt = 0;
            bool isfirst = true;
            while (flag)
            {
                try
                {
                    ////////////////////////////////////////////////////////////
                    ////
                    /*
                    asyncHandle = mbSession.RawIO.BeginWrite(
                        "FETC:CURR?\nFETC:VOLT?",
                        new VisaAsyncCallback(OnWriteComplete),
                        (object)"FETC:CURR?\nFETC:VOLT?".Length);
                    */
                    /*
                    asyncHandle = mbSession.RawIO.BeginWrite(
                        "FETC:VOLT?",
                        new VisaAsyncCallback(OnWriteComplete),
                        (object)"FETC:VOLT?".Length);
                    */
                    /*
                    asyncHandle = mbSession.RawIO.BeginRead(
                    1024,
                    new VisaAsyncCallback(OnReadComplete),
                    null);
                    */

                    
                    /*
                    asyncHandle = mbSession.RawIO.BeginRead(
                    1024,
                    new VisaAsyncCallback(OnReadComplete),
                    null);
                    */
                    ////////////////////////////////////////////////////////////

                    
                    //获取电流数据
                    mbSession.RawIO.Write("FETC:CURR?\nFETC:VOLT?");
                    String [] resdata = mbSession.RawIO.ReadString().Split(';');
                    Thread.Sleep(1);

                    if (resdata.Length >= 2)
                    {
                        double data1 = Convert.ToDouble(resdata[0]) / 10;
                        double data2 = Convert.ToDouble(resdata[1]) / 10;


                        var date = DateTime.Now;
                        
                        _myPlotModel.Axes[0].Maximum = DateTimeAxis.ToDouble(date.AddMilliseconds(1));

                        var lineSer = plotView1.Model.Series[0] as LineSeries;
                        lineSer.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data1));
                        if (!checkBox1.Checked)
                        {
                            if (lineSer.Points.Count > 250)
                            {
                                lineSer.Points.RemoveAt(0);
                            }
                        }

                        lineSer = plotView1.Model.Series[1] as LineSeries;
                        lineSer.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), data2));
                        if (!checkBox1.Checked)
                        {
                            if (lineSer.Points.Count > 250)
                            {
                                lineSer.Points.RemoveAt(0);
                            }
                        }
                        _myPlotModel.InvalidatePlot(true);

                    }
                    Thread.Sleep(1);
                    
                }
                catch (Exception)
                {

                }
            }
        }

        
        public void SetmbSession(MessageBasedSession mbSession)
        {
            this.mbSession = mbSession;
        }

        public void setSerial(SerialPort serial) {
            this.serial = serial;
        }

        private void ShowVCData_HelpButtonClicked(object sender, CancelEventArgs e)
        {

        }

        private void ShowVCData_FormClosing(object sender, FormClosingEventArgs e)
        {
            flag = false;
            this.Dispose();
        }

        private void Restart_btn_Click(object sender, EventArgs e)
        {
            flag = true;
            dataList[0].Clear();
            dataList[1].Clear();
            var line =  plotView1.Model.Series;
            for(int i = 0; i < line.Count; i++)
            {
                LineSeries series = line[i] as LineSeries;
                series.Points.Clear();
            }
            var axis = plotView1.Model.Axes;
            foreach (var o in axis)
            {
                o.IsPanEnabled = false;
                o.IsZoomEnabled = false;
                o.Reset();
            }
            plotView1.Invalidate(true);
            
            checkBox1.Checked = false;
            if (this.kind == false)
            {
                Thread thread = new Thread(new ThreadStart(refresh_curve));
                thread.Start();
            }
            else
            {
                Thread thread = new Thread(new ThreadStart(ShowCurves));
                thread.Start();
            }
        }

        private void Stop_btn_Click(object sender, EventArgs e)
        {
            flag = false;
            var axis = plotView1.Model.Axes;
            foreach (var o in axis){
                o.IsPanEnabled = true;
                o.IsZoomEnabled = true;
            }
        }
        //保存数据
        private void button1_Click(object sender, EventArgs e)
        {
            double[,] matdata = new double[2, dataList[0].Count];
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < dataList.ElementAt(i).Count; j++)
                {
                    matdata[i, j] = dataList.ElementAt(i).ElementAt(j);
                }
            }


            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matdata);

            String path = "D:\\TestDataFile\\thmat\\" + DateTime.Now.ToString("MM -dd-H-mm-ss_")  + ".mat";
            MatlabWriter.Write(path, matrix, "data");


        }
    }
}
