using Neuronic.TimeFrequency;
using Neuronic.TimeFrequency.Transforms;
using Neuronic.TimeFrequency.Wavelets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wtf
{
    public partial class DWTForm : Form
    {   
        
        public ConcurrentQueue<List<Double>> data = new ConcurrentQueue<List<double>>();
        private int showType = 0;
        private bool start = false;
        private bool stop = false;
        
        public DWTForm()
        {
            InitializeComponent();
            this.button1.Click += Button1_Click;
            this.button2.Click += Button2_Click;
        }

        
        //概貌
        private void Button2_Click(object sender, EventArgs e)
        {
            showType = 1;
        }
        //细节
        private void Button1_Click(object sender, EventArgs e)
        {
            showType = 0;
        }

        public  void setData(List<Double> data)
        {
            lock (data)
            {
                if(!stop)
                this.data.Enqueue(data);
            }
            if (!start)
            {
                Thread thread = new Thread(new ThreadStart(DWT));
                thread.Start();
                start = true;
            }
        }

        private List<Double> changeValue(List<Double> list,double v,int level)
        {
            for(int i = 0; i < list.Count; i++)
            {
                list[i] = list.ElementAt(i) + v * level;
            }

            return list;
        }
        

        private void showGraph(DiscreteWaveletTransform rs,int level,int flag)
        {
            if (level >= 0&&flag==0)
            {

                formsPlot1.plt.PlotSignal(changeValue(rs.Detail.ToList<Double>(),0.1,level).ToArray(),label:"第"+level+"层的细节");
                showGraph(rs.UpperScale, level-1, flag);

            }else if(level >= 0 && flag == 1)
            {
                formsPlot1.plt.PlotSignal(changeValue(rs.Approximation.ToList<Double>(), 0.1, level).ToArray(),label: "第" + level + "层的概貌");
                
                showGraph(rs.UpperScale, level - 1, flag);
            }

        }

        private void DWT()
        {

            while (start)
            {
                lock (data)
                {
                    if (!data.IsEmpty&&!stop)
                    {
                        data.TryDequeue(out List<Double> res);
                        Signal<Double> sig = new Signal<double>(res.ToArray());
                        formsPlot1.plt.Clear();
                        try
                        {
                            int level = Convert.ToInt32(this.ResolveText.Text);
                            DiscreteWaveletTransform rs = DiscreteWaveletTransform.Estimate(sig, Wavelets.Daubechies(2), new ZeroPadding<Double>());
                            DiscreteWaveletTransform rs1 = rs.EstimateMultiscale(new ZeroPadding<Double>(), level);
                            
                            if (showType == 0)
                            {

                                //formsPlot1.plt.PlotSignal(rs.Detail.ToArray(), label: "第一层细节");
                                showGraph(rs1, level, showType);
                            }
                            else
                            {
                                //formsPlot1.plt.PlotSignal(rs1.Approximation.ToArray(), label: "第一层概貌");
                                showGraph(rs1, level, showType);
                            }
                            formsPlot1.plt.Legend();
                            formsPlot1.Render();

                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e.Message);
                        }

                    }

                }
            }
            

        }

        private void DWTForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            start = false;
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Stop_btn.Text.Equals("暂停"))
            {
                stop = true;
                Stop_btn.Text = "重启";
            }
            else
            {
                while (!data.IsEmpty)
                {
                    data.TryDequeue(out List<Double> result);
                }
                stop = false;
                Stop_btn.Text = "暂停";
            }
        }
    }
}
