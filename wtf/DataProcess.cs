using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wtf
{
    class DataProcess
    {

        public enum crackDetectionType {
            CrackDetectionNoneCrack =0,
            CrackDetectionUpside=1,
            CrackDetectionDownside=2
        };


        public static double EuclideanDistance( double y1, double y2)
        {
            return Math.Sqrt((y1 - y2) * (y1 - y2));
        }

        public static double ComputeDistance(double[] distances, int i,  double[] P, double[] Q)
        {
            if (distances[i] > -1)
                return distances[i];

            if (i == 0)
                distances[i] = EuclideanDistance(P[0], Q[0]);
            else if (i > 0)
                distances[i] = Math.Max(ComputeDistance(distances, i - 1, P, Q),
                                           EuclideanDistance(P[i], Q[0]));
            else
                distances[i] = Double.PositiveInfinity;

            return distances[i];
        }
        static double[] distances = new double[100];
        public static double FrechetDistance(double[] P, double[] Q)
        {
            for (int y = 0; y < P.Length; y++)
                    distances[y] = -1;

            return ComputeDistance(distances, P.Length - 1,  P, Q);
        }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indata">输入信号</param>
    /// <param name="window">窗口大小</param>
    /// <param name="startPosition"> 尝试使用匹配的方法得到的检测边界所在的位置</param>
    /// <returns>滤波后的信号</returns>
    public static double[] movmeanFilter(double[] indata, int window, int filterStart, ref List<int> startPosition)
        {
           
            double[] r = new double[indata.Length];
            double average;
           //decimal[] indata = Array.ConvertAll(indatadouble, x => (decimal)x);
            if (window < indata.Length - 1 && window > 1)
            {
                int center = window / 2;
                double windowSum = 0;
                //Console.WriteLine("last" + filterStart + ", cuurent" + indatadouble.Length);
                int windowlength = center;
                for(int i=0; i<filterStart; i++)
                {
                    r[i] = indata[i];
                }
                //先加半个周期
                for(int m= filterStart; m< filterStart+ center; m++)
                {
                    windowSum += indata[m];
                    //Console.WriteLine("m:" + indata[m]);
                }
                r[0] = (windowSum / center);
                average = 0;
                //Console.WriteLine("first:"+windowSum);
                for (int i = filterStart+1; i < indata.Length; i++)
                {
                    //以i为中心，两侧取window/2宽度
                   if(i+center < indata.Length)
                    {
                        windowSum += indata[i + center];
                        windowlength++;
                    }
                    if (i > center+filterStart)
                    {
                        windowSum -= indata[i - center];
                        windowlength--;
                    }
                   
                    r[i] =(windowSum / windowlength) ;
                    if (i % 100 == 0)
                    {
                       // Console.WriteLine(i + ":" + (windowSum - 13) + "@length:"+windowlength+"@value:" +r[i]);
                    }
                    // Console.WriteLine(windowSum);
                    average = (average*(i-1) + Math.Abs(r[i]-r[0])) / i;
                    if (Math.Abs(r[i]-r[0]) >25*average)
                    {
                        //Console.WriteLine("find peak:{0}, with value:{1}, average is {2}",i, r[i] - r[0], average);
                        startPosition.Add(i);
                    }
                }
                
            }
            
            return r;

        }

        //const int detectWindowUnits = 8;
        public static double[] crackDetectPeaks(List<double> indata, int sampleFrequency, int signalFrequency)
        {
            
            //按基本单位返回
            int halfWaveLength = sampleFrequency / signalFrequency / 2;
            int totalHalfWave = indata.Count / halfWaveLength + 1;
            int detectThresholdWindow = halfWaveLength * 2;
            List<double> m = new List<double>();
            for (int i = 0; i < indata.Count; i = i + 4)
            {
                m.Add(indata.ElementAt(i));
            }
          
            var output = ZScore.StartAlgo(m, detectThresholdWindow,7, 0);
            return  output.signals.ToArray();            
        }

        //按1cm/s来算，10000um / sf（500） 为单个周期信号所占用的空间长度（20um），
        // sf(4k)/fs(500)=单个信号的采样点数（8）
        //即为了保证缺陷的识别精度在20um以上，以半波为基本单位（10um）进行检测，
        // 显示精度可以以采样点（20/8=2.5um）为基本单位, 
        //返回值以基本单位为基础间隔，值为该间隔下的具体数值
        const int detectWindowUnits = 8;
        public static double[] crackDetectFreqect(double[] indata, int sampleFrequency, int signalFrequency)
        {
            //按基本单位返回
            int halfWaveLength = sampleFrequency / signalFrequency/2 ;
            int totalHalfWave = indata.Length / halfWaveLength + 1;
            double[] r = new double[totalHalfWave];
            int detectThresholdWindow = halfWaveLength * 2;
            int detectWindow = halfWaveLength * detectWindowUnits; //相当于每两个波形进行一次检测
            r[0] = 0;
            double[] P = new double[detectThresholdWindow];
            double[] Q = new double[detectThresholdWindow];
            double localMaxValue = 0;
            for (int i=0; i< detectThresholdWindow; i++)
            {
                P[i] = Math.Sin(2 * Math.PI * i / detectThresholdWindow);
            }
            for (int i = 1; i < totalHalfWave-1; i++)
            {
                int start = (i - 1) * halfWaveLength;
                int end = (i+1) * halfWaveLength;
                localMaxValue = 0.001;
                Parallel.For(0, detectThresholdWindow, (m) =>
                {
                    if(indata[m] > localMaxValue)
                    {
                        localMaxValue = indata[m];
                    }
                });
                for(int j=0; j<detectThresholdWindow; j++)
                {
                    Q[j] = indata[start + j] / localMaxValue;
                }
                r[i] = FrechetDistance(P, Q);
            }
            r[totalHalfWave - 1] = 0;
            return r;
        }

        public static double[] crackDetect2(double[] indata)
        {
            double[] r = new double[indata.Length];
            return r;
        }
        public static double[] crackDetect(double[] indata, int sampleFrequency, int signalFrequency)
        {
            //按基本单位返回
            int halfWaveLength =sampleFrequency / signalFrequency / 2;
            int totalHalfWave = indata.Length / halfWaveLength + 1;
            double[] r = new double[totalHalfWave];
            double[] tmp = new double[indata.Length];
            int detectThresholdWindow = halfWaveLength *2;
            int detectWindow = halfWaveLength * detectWindowUnits; //相当于每两个波形进行一次检测
            // 1。 窗口内计算均值
            //2。 所有数据减均值后再abs
            //3.   连续性检测，如果数值连续<阈值min，说明没有信号段。。如果连续大于阈值Max，说明也是信号异常
             int windowStart;
            int windowEnd;
            double threshold;
            //double thresholdMax = 0;
            double average;
            double MaxAverage = -1000; // 以最大平均值为基准，当平均值明显异常于常规时，应该是缺陷
            int upCount =0, downCount=0;
            bool isDownCounting = true;
            double calTmp;
            bool needFillFront = true;

            for(int i =0; i< indata.Length/detectWindow + 1; i++)
            {
                windowStart = i * detectWindow;
                windowEnd = windowStart + detectWindow;
                if(windowStart >= indata.Length)
                {
                    break;
                }
                if(windowEnd >= indata.Length)
                {
                    windowEnd = indata.Length - 1;
                }

                //计算均值
                average = 0; //算峰峰值 ，不是均值 
                double max = -1, min = 100000;
                for(int j=windowStart; j<windowEnd; j++)
                {
                    //average += indata[j];
                    if(indata[j] > max)
                    {
                        max = indata[j];
                    } else if(indata[j] < min)
                    {
                        min = indata[j];
                    }
                   // tmp[j] = indata[j]; //临时存放，避免修改原数据
                }
                average = max - min;  //average / (windowEnd-windowStart);
                if(average > MaxAverage)
                {
                    MaxAverage = average;
                }
                //Console.WriteLine("average:{0}, maxAverage:{1}", average, MaxAverage);
                if(Math.Abs(average - MaxAverage) > MaxAverage * 0.8)
                {
                    for (int j = windowStart; j < windowEnd; j++)
                    {                       
                        if (j % halfWaveLength == (halfWaveLength - 1)) //不考虑最后一个unit不够的情况
                        {
                            r[j / halfWaveLength] = 1;// > 1? calTmp: -calTmp;
                        }
                    }

                } else
                {
                    //缺陷判断
                    average = min + average/2;
                    for (int j = windowStart; j < windowEnd; j++)
                    {
                        calTmp = indata[j];// (tmp[j]>average)?(tmp[j]-average): (average- tmp[j]);
                        tmp[j] = 0;//默认没有缺陷
                                   //连续性检测
                        if (calTmp < average)
                        {
                            if (isDownCounting)
                                downCount++;
                            isDownCounting = true;
                            if (downCount > halfWaveLength / 8)
                            //去除偶然点的影响，即当超过1/4窗口才认为连续性中断
                            {
                                upCount = 0;
                            }

                            if (downCount > detectThresholdWindow)
                            //如果满足连续性
                            {
                                tmp[j] = 1;
                                if (needFillFront)
                                {
                                    for (var n = 1; n < detectThresholdWindow; n++)
                                    {
                                        tmp[j - n] = 1;
                                    }
                                    needFillFront = false;
                                }
                            }
                            else
                            {
                                needFillFront = true;
                            }
                        }
                        else
                        {
                            if (!isDownCounting)
                                upCount++;
                            isDownCounting = false;
                            if (upCount > halfWaveLength / 8)
                            //去除偶然点的影响，即当超过1/4窗口才认为连续性中断
                            {
                                downCount = 0;
                            }

                            if (upCount > detectThresholdWindow)
                            //如果满足连续性
                            {
                                tmp[j] = 2;
                                if (needFillFront)
                                {
                                    for (var n = 1; n < detectThresholdWindow; n++)
                                    {
                                        tmp[j - n] = 2;
                                    }
                                    needFillFront = false;
                                }
                            }
                            else
                            {
                                needFillFront = true;
                            }
                        }
                    }

                    //每个detectwindow 包含<=detectWindowUnits(4)个基本窗口，
                    calTmp = 0; //临时用于缺陷计数
                    upCount = 0;//用于窗口计数
                     for (int j = windowStart; j < windowEnd; j++)
                     {
                         if (tmp[j] > 0) //无论是上还是下，都是一种缺陷
                         {
                             calTmp++;
                         }
                         if (j % halfWaveLength == (halfWaveLength - 1)) //不考虑最后一个unit不够的情况
                         {
                            r[j / halfWaveLength] = calTmp > 0 ? 1 : 0;// tmp[j];// > 1? calTmp: -calTmp;
                             calTmp = 0;
                         }
                     }
                    

                }
                
            }
            //r[100] = 1;
            return r;
        }
        public static void AlignDataSimple(ref double[,] heatvalue, double[,] peaksValue, int sampleFrequency, int signalFrequency)
        {

            if (heatvalue == null || peaksValue == null)
            {
                return;
            }
            int halfWaveLength = sampleFrequency / signalFrequency / 2;
            //int[] differ = new int[16];
            double[,] heatvaluecopy = heatvalue;
            int startPosition = 6400;// 16000/(1/Frequece4000)
            int endPostion = 14400;//32000/(1/Frequece4000)
            //for (var i = 0; i < 16; i++)
            int[] standPosition = new int[64];
            int minP = endPostion;
            for(int i =0; i<64; i++)
            {
                 //查找2.5附近的峰会
                 for (var m = startPosition; m < endPostion && m < peaksValue.GetUpperBound(1); m++)
                 {
                     if (peaksValue[ i,m ] - peaksValue[ i,0 ] > 0.8)
                     {
                         standPosition[i] = m;
                         if (minP > m)
                         {
                             minP = m;
                         }
                     }
                 }
             }
            //以最小的为准进行移动
            for (int i = 0; i < 64; i++)
            {
                int diff =(standPosition[i] - minP)/20;
                Console.WriteLine("differ" + diff);
                if (diff > 0)
                {
                    for (int j = 0; j < heatvaluecopy.GetUpperBound(0) - diff; j++)
                    {
                        heatvaluecopy[j, i ] = heatvaluecopy[j + diff, i ];
                    }
                    for (int j = heatvaluecopy.GetUpperBound(0) - diff; j < heatvaluecopy.GetUpperBound(0); j++)
                    {
                        heatvaluecopy[j, i] = 0;
                    }
                }
            }
            heatvalue = heatvaluecopy;
            return;
        }

        public static void AlignData(ref double[,] heatvalue,  int[] firstpeaks, int[] lastpeaks, int sampleFrequency, int signalFrequency)
        {

            if (heatvalue == null)
            {
                return;
            }
            int halfWaveLength = sampleFrequency / signalFrequency / 2;
            int[] differ = new int[16];
            double[,] heatvaluecopy = heatvalue;
            
            for(var i=0; i<16; i++)
            {
                //-----+-----+--- firstpeak
                //---+----+------lastpeak
                // ---------+..+---differ
                //
                differ[i] = (lastpeaks[i] - firstpeaks[i]) / halfWaveLength;
                if (differ[i] == 0)
                {
                    continue;
                }
                if(differ[i] > 0)
                {
                    for(int j =0; j < heatvaluecopy.GetUpperBound(0)-differ[i]; j++)
                    {
                        heatvaluecopy[j, i * 4] = heatvaluecopy[j+differ[i], i * 4];
                    }
                    for(int j = heatvaluecopy.GetUpperBound(0) - differ[i]; j < heatvaluecopy.GetUpperBound(0); j++)
                    {
                        heatvaluecopy[j, i * 4] = 0;
                    }
                } else
                {
                    differ[i] = -differ[i];
                    for (int j = 0; j < heatvaluecopy.GetUpperBound(0) - differ[i]; j++)
                    {
                        heatvaluecopy[j, i * 4+1] = heatvaluecopy[j + differ[i], i * 4+1];
                    }
                    for (int j = heatvaluecopy.GetUpperBound(0) - differ[i]; j < heatvaluecopy.GetUpperBound(0); j++)
                    {
                        heatvaluecopy[j, i * 4+1] = 0;
                    }
                }
            }
            heatvalue = heatvaluecopy;
            return;
        }
        public static double[] Butterworth(double[] indata, double deltaTimeinsec, double CutOff)
        {
            if (indata == null) return null;
            if (CutOff == 0) return indata;

            double Samplingrate = 1 / deltaTimeinsec;
            long dF2 = indata.Length - 1;        // The data range is set with dF2
            double[] Dat2 = new double[dF2 + 4]; // Array with 4 extra points front and back
            double[] data = indata; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++)
            {
                Dat2[2 + r] = indata[r];
            }
            Dat2[1] = Dat2[0] = indata[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = indata[dF2];

            const double pi = 3.14159265358979;
            double wc = Math.Tan(CutOff * pi / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = indata[0];
            for (long s = 2; s < dF2 + 2; s++)
            {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2]
                           + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD filter
            double[] DatZt = new double[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++)
            {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4]
                            + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++)
            {
                data[p] = DatZt[p];
            }

            return data;
        }
    }
}
