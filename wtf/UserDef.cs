using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace wtf
{
    class UserDef
    {
        public const UInt16 Threahold = 1;
        public static int Frequency= 1000000; // �ɼ��ź�Ƶ��
        public static int signalFrequency = 250; // �ź�Ƶ��
        public static double tolerance = 0.03; // �ź�Ƶ��
        public const String PathDir = @"D:\TestDataFile\";
        public static List<List<Double>> dataToSave = new List<List<Double>>() { new List<double>(), new List<double>(), new List<double>(), new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>()};

        public static List<List<Double>> dataCompute = new List<List<Double>>() { new List<double>(), new List<double>(), new List<double>(), new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>()};

        public static List<List<Double>> freq = new List<List<Double>>() { new List<double>(), new List<double>(), new List<double>(), new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>()};

        public static List<List<Double>> MAXVALUE = new List<List<Double>>() { new List<double>(), new List<double>(), new List<double>(), new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>(),
        new List<double>(), new List<double>(), new List<double>() , new List<double>()};

        public static List<List<Double>> freq11 = new List<List<Double>>()
        {new List<double>(), new List<double>() };

        public static bool timeOut = false;


        public static List<Double> xToSave = new List<Double>();
        public static  Double NowRes = 0;
        public static bool flag1 = true;
        public static bool flagRecord = false;
      
        
        
        // ͨ����Ϣ
        public struct CHANNEL_INFO
        {
            public Byte nChan;                        // ͨ����
            public UInt32 dwChanTotal;           // ��Ҫ�������ֽ���
            public UInt32 dwChanRecved;       // �Ѿ��յ���������
            public IntPtr pNext;
        }

        // �������Բ�������
        public struct CFGPARA
        {
           // public NET2991.NET2991_AI_PARAM AIParam;            // AI����
            public NET2991A.NET2991A_AI_PARAM AAIParam;            // AI����
            //public NET2991B.NET2991B_AI_PARAM BAIParam;            // AI����
            public CHANNEL_INFO pChanInfo;                                // ͨ����Ϣ����
            public CHANNEL_INFO pChanHead;

            public Int32 nReadOffset;                                                  // ��ȡƫ��
            public Int32 nReadLength;                                                 // ��ȡ����
            public Int32 bAllSameParam;                                             // �����忨�����ͬ,�ô˿��Խ��������忨����ͬ�໯

            public Int32 bSel;                                                               // ѡ��ɼ��˰忨����
            public Int32 bOnLine;                                                        // ����
            public Int32 bCalibration;                                                    // �Ƿ�ѡ��У׼
            public Int32 bDisplay;                                                         // �Ƿ�����/ͼ����ʾ,������ģʽ��ʹ��
            public Int32 bAIStatus;                                                       // �Ƿ�õ�״̬,����ģʽ����Ч
            public Int32 nGetStsFailCnt;                                               // �õ�״̬ʧ�ܴ���
            public Int32 bAIStsTimeOut;                                              // �õ�AI״̬��ʱ�Ƿ�ʱ
            public Int32 dwAIStsTimeOut;                                            // AI״̬��ʱʱ��
            public Int32 nSampStatus;                                                   // �ɼ����
            public Int32 dwChanCnt;                                                    // ʵ�ʲɼ���ͨ����
            public Int32 dwReadDataSize;                                             // ���ڶ�ȡ��������
            public Int32 dwRealReadLen;                                              // ʵ�ʶ�ȡ�����ݳ���
            public Int32 dwFrameLen;                                                   // ��ȡÿ�����ݵĳ���
            public IntPtr hFile;                                                               // 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
            public Byte[] svFileName;
            public IntPtr hDevice;                                                          // �豸���
        }

        // ��ȡ״̬
        public const Int32 CMD_UNCPT = 0;                            // ��ȡδ���
        public const Int32 CMD_CPT = 4;                                 // ��ȡ���

        // ���������ݵĿ�ʼλ��
        public const Int32 DATA_START = 8;
        public const Int32 LMT_FRMCNT = 1288;

        public class channleSetting : INotifyPropertyChanged
        {
            private int _max;
            private int _min;
            public int no { get; set; }
            public int max { get { return _max; }
                set {
                    if(_max != value)
                    {
                        _max = value;
                        if(_max > 0)
                        {
                            NotiFy("max");
                        }
                        
                    }
                    
                }
            }
            public int min
            {
                get { return _min; }
                set
                {
                    if(_min != value)
                    {
                        _min = value;
                        if(_min != 65536)
                        {
                            NotiFy("min");
                        }
                        
                    }
                    
                }
            }
            public int max_th { get; set; }
            public int min_th { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            public void NotiFy(string property)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
            }
        }
        public static ObservableCollection<channleSetting> d2991A = new ObservableCollection<channleSetting>();

    }

    public static class ArrayExt
    {
        public static T[] GetRow<T>(this T[,] array, int row)
        {
            if (!typeof(T).IsPrimitive)
                throw new InvalidOperationException("Not supported for managed types.");

            if (array == null)
                throw new ArgumentNullException("array");

            int cols = array.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            int size;

            if (typeof(T) == typeof(bool))
                size = 1;
            else if (typeof(T) == typeof(char))
                size = 2;
            else
                size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

            return result;
        }
    }
}
