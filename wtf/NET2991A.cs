using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.IO.Compression;
using static wtf.MainWindow;

namespace wtf
{
    static class NativeMethods
    {
        [DllImport("kernel64.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel64.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel64.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
    class NET2991A
    {
        public const Int32 STATUS_RUNNING = 1;           // ���ڲɼ�
        public const Int32 STATUS_STOPED = 2;              // �ɼ���ʼ��ɼ����

        /////////////////////////////////////////////////////////////////////////
        public const Int32 NET2991A_MAX_CHANNELS = 17;                   // �������֧��17·ģ����������������
        public const Int32 NET2991A_AI_MAX_CHANNELS = 16;             // �������֧��16·ģ������������ͨ��
        public const Int32 NET2991A_DI_MAX_PORTS = 1;                       // �������֧��1���������˿�
        public const Int32 NET2991A_DIO_PORT0_MAX_LINES = 16;      // �������˿�0֧��16����
        public const Int32 NET2991A_MAX_DEVICENAME_LEN = 32;    // �豸���ֳ���

        //#################### AIӲ������USB2851_AI_PARAM���� #####################
        // ͨ�������ṹ��
        public struct NET2991A_CH_PARAM
        {
            public UInt32 bChannelEn;             // BYTE[3:0] ͨ��ʹ��
            public UInt32 nSampleRange;         // BYTE[7:4] ������Χ(Sample Range)��λѡ��������泣������
            public UInt32 nRefGround;             // BYTE[11:8] �زο���ʽ��	0:��������; 1:�������
            public UInt32 nReserved0;             // BYTE[15:12] ����(��δ����)
            public UInt32 nReserved1;             // BYTE[19:16] ����(��δ����)
            public UInt32 nReserved2;             // BYTE[23:20] ����(��δ����)
        }

        // AIӲ�������ṹ��NET2991A_CH_PARAM��NET2991A_CH_PARAM�ṹ�������е�nSampleRange������Χ��ʹ�õ�ѡ��
        public const UInt32 NET2991A_AI_SAMPRANGE_N10_P10V = 0;               // ��10V
        public const UInt32 NET2991A_AI_SAMPRANGE_N5_P5V = 1;                   // ��5V
        public const UInt32 NET2991A_AI_SAMPRANGE_N2D5_P2D5V = 2;          // ��2.5V
        public const UInt32 NET2991A_AI_SAMPRANGE_N1D25_P1D25V = 3;       // ��1.25V

        // AIӲ�������ṹ��NET2991A_CH_PARAM��NET2991A_CH_PARAM�ṹ�������е�nRefGround�ο���ģʽ��ʹ�õ�ѡ��
        public const UInt32 NET2991A_AI_REFGND_RSE = 0;              // �ӵزο�����(Referenced Single Endpoint)
        public const UInt32 NET2991A_AI_REFGND_DIFF = 1;             // ���(Differential)

        public struct NET2991A_AI_PARAM
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public SByte[] szDevName;                                               // [31:0]  ���Ʋ�����15�����֣���30���ֽ�
            public UInt32 nDeviceIP;                                                   // [35:32] �豸IP��ַ
            public UInt16 nDevicePort;                                                // [37:36] �豸�˿ں�
            public UInt16 nLocalPort;                                                  // [39:38] �����˿ں�
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public NET2991A_CH_PARAM[] CHParam;                       // [447:40] ͨ����������, 0-15��ӦAI0-AI15, 16��ӦDI

            public double fSampleRate;                                                // [455:448] ��������[0.00931sps, 1000000sps]
            public UInt32 nSampleMode;                                             // [459:456] ����ģʽ  2:���޲���, 3:�������� 
            public UInt32 nSampsPerChan;                                          // [463:460] �豸ÿ��ͨ����������(Ҳ��ÿͨ������ȡ����)(��λ:��)
            public UInt32 nClockSource;                                              // [467:464] ʱ��Դ 0:��ʱ��; 1:�ⲿ�ο�10Mʱ��; 2:����ʱ��; 3:�ⲿת��ʱ��; 
            public UInt32 nReserved0;                                                 // [471:468] ����(��δ����)

            public UInt32 nTriggerSource;                                            // [475:472] ����Դѡ�� 0:�������; 1:�ⲿ����������; 2:�ⲿģ��������; 3:ͬ������ 
            public UInt32 nTriggerDir;                                                  // [479:476] �������� 0:�½��ش���; 1:�����ش���; 2/3:�����½�������(�仯)
            public Single fTriggerLevel;                                                // [483:480] ������ƽ(V)
            public Int32 nDelaySamps;                                                 // [487:484] �����ӳٵ���(��λ:��)
            public UInt32 nReTriggerCount;                                         // [491:488] �ظ��������� �������޲����ĺ󴥷���Ӳ����ʱ����ģʽ��

            public UInt32 bMasterEn;                                                  // [495:492] ���豸ʹ�� 0:���豸; 1:���豸

            public UInt32 nReserved1;                                                 // [499:496] ����(��δ����)
            public UInt32 nReserved2;                                                 // [503:500] ����(��δ����)
        }

        // AIӲ�����ݴ���ķ���
        public const Int32 NET2991A_AI_TRANDIR_CLIENT = 0;             // ���ݴ��䷽��Ϊ�ͻ���
        public const Int32 NET2991A_AI_TRANDIR_SERVER = 1;            // ���ݴ��䷽��Ϊ������

        // AIӲ�������ṹ��NET2991A_AI_PARAM�е�nSampleMode����ģʽ��ʹ�õ�ѡ��
        public const Int32 NET2991A_AI_SAMPMODE_FINITE = 2;                              // ���޲���
        public const Int32 NET2991A_AI_SAMPMODE_CONTINUOUS = 3;                 // ��������

        // AIӲ�������ṹ��NET2991A_AI_PARAM�е�nTriggerSource����Դ�ź���ʹ�õ�ѡ��
        public const Int32 NET2991A_AI_TRIGSRC_NONE = 0;                         // �޴���(�ȼ����������)
        public const Int32 NET2991A_AI_TRIGSRC_DIGITAL = 1;                     // �ⲿ����������
        public const Int32 NET2991A_AI_TRIGSRC_ANALOG = 2;                    // �ⲿģ��������
        public const Int32 NET2991A_AI_TRIGSRC_SYNC = 3;                          // ͬ������

        // AIӲ�������ṹ��NET2991A_AI_PARAM�е�nTriggerDir����������ʹ�õ�ѡ��
        public const Int32 NET2991A_AI_TRIGDIR_FALLING = 0;                       // �½��ش���
        public const Int32 NET2991A_AI_TRIGDIR_RISING = 1;                          // �����ش���
        public const Int32 NET2991A_AI_TRIGDIR_CHANGE = 2;                       // �仯(�����½�������)

        // AIӲ�������ṹ��NET2991A_AI_PARAM�е�nClockSourceʱ��Դ��ʹ�õ�ѡ��
        public const Int32 NET2991A_AI_CLOCKSRC_LOCAL = 0;                      // ����ʱ��(ͨ��Ϊ���ؾ���ʱ��OSCCLK),Ҳ���ڲ�ʱ��
        public const Int32 NET2991A_AI_CLOCKSRC_CLKIN_10M = 1;              // �ⲿ�ο�10Mʱ�Ӷ�ʱ����
        public const Int32 NET2991A_AI_CLOCKSRC_MAIN_BOARD = 2;         // ����ʱ��
        public const Int32 NET2991A_AI_CLOCKSRC_CLKIN = 3;                       // �ⲿ����ʱ��

        // ����AI������Ӳ������״̬
        public struct NET2991A_AI_STATUS
        {
            public UInt32 bTaskDone;                                                          // [3:0]   AI���������Ƿ������1:��ʾ�ѽ���; 0:��ʾδ����
            public UInt32 bTriggered;                                                           // [7:4]   AI�Ƿ񱻴�����1:��ʾ�ѱ�����; 0:��ʾδ������(Ĭ��)
            public UInt32 bFreq10M;                                                           // [11:8]  �ⲿ�����ź��Ƿ�10M
            public UInt32 nClockInFreq;                                                      // [15:12] �ⲿʱ���ź�Ƶ��, ��λHz

            public UInt32 nSampTaskState;                                                  // [19:16] ��������״̬, =1:����, ����ֵ��ʾ���쳣���
            public UInt32 nAvailSampsPerChan;                                          // [23:20] ÿͨ����Ч������ֻ�������ڵ�ǰָ����������ʱ���ܵ���AI_ReadAnalog()������ȡָ�����ȵĲ�������
            public UInt32 nMaxAvailSampsPerChan;                                    // [27:24] ��AI_StartTask()��ÿͨ�����ֹ��������Ч������״ֵ̬��Χ[0, nBufSampsPerChan],����Ϊ���ɼ�������ܶ��ṩ�������ֵԽ������1�����ʾ��ζ������Խ�ߣ�Խ���׳����������Ŀ���
            public UInt32 nBufSampsPerChan;                                             // [31:28] ÿͨ����������С(��������)
            public Int64 nSampsPerChanAcquired;                                       // [39:32] ÿͨ���Ѳ�������(������AI_StartTask()֮���������ĵ���)�����ֻ�Ǹ��û���ͳ������

            public UInt32 nHardOverflowCnt;                                              // [43:40] Ӳ���������(�ڲ��������º����0)
            public UInt32 nSoftOverflowCnt;                                               // [47:44] ����������(�ڲ��������º����0)
            public UInt32 nInitTaskCnt;                                                       // [51:48] ��ʼ����������Ĵ���(������AI_InitTask()�Ĵ���)
            public UInt32 nReleaseTaskCnt;                                                // [55:52] �ͷŲ�������Ĵ���(������AI_ReleaseTask()�Ĵ���)
            public UInt32 nStartTaskCnt;                                                     // [59:56] ������������Ĵ���(������AI_StartTask()�Ĵ���)
            public UInt32 nStopTaskCnt;                                                      // [63:60] ֹͣ��������Ĵ���(������AI_StopTask()�Ĵ���)
            public UInt32 nTransRate;                                                         // [67:64] ��������, ��ÿ�봫�����(sps)����ΪUSB��Ӧ������������ܵļ����Ϣ

            public UInt32 nReserved0;                                                         // [71:68] �����ֶ�(��δ����)
            public UInt32 nReserved1;                                                         // [75:72] �����ֶ�(��δ����)
            public UInt32 nReserved2;                                                         // [79:76] �����ֶ�(��δ����)
            public UInt32 nReserved3;                                                         // [83:80] �����ֶ�(��δ����)
            public UInt32 nReserved4;                                                         // [87:84] �����ֶ�(��δ����)
        }

        // �豸���������Ϣ�ṹ��
        public struct DEVICE_NET_INFO
        {
            public UInt32 nDeviceIP;                       // [3:0]   IP��ַ  192.168.0.1
            public UInt16 nDevicePort;                    // [5:4]   �˿ں� ���65535
            public UInt16 nReserved0;                     // [7:6]   �����ֶ�(��δ����)
            public UInt64 nMAC;                            // [15:8]  ���������ַ,�û�һ�㲻�ɸ���
            public UInt32 bOneline;                         // [19:16] ����״̬��1:����; 0:����(����)
            public UInt32 nSubnetMask;                  // [23:20] ��������, "255.255.255.0"
            public UInt32 nGateway;                       // [27:24] ����, "192.168.0.1"
            public UInt32 nReserved1;                     // [31:28] �����ֶ�(��δ����)
        }

        // �õ�dll�ڲ��豸����״̬
        public struct DEVICE_SAMP_STS
        {
            public UInt32 nDevIP;                               // [0:3]�豸IP
            public Int32 nSampSts;                               // [4:7]�豸״̬

            public Int32 nChan;                                    // [8:11]��ǰͨ���ţ�����ʹ��
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public SByte[] szFileName;                        // [12:267]�ļ���
            public UInt32 nReserved0;                          // [268:271]����δ��
            public UInt64 ullFileSize;                             // [272:279]�ļ���С
        }

        // �豸���ݶ�ȡ�����Ϣ�ṹ��
        public struct DEVICE_SAMP_CMP
        {
            public UInt32 nDevIP;                   // [0:3]�豸IP
            public UInt32 bSampCmp;             // [4:7]�豸���ݶ�ȡ��ɱ�־ 0��δ���; 1�����
        }

        // ����״̬���� �ڲ�����,��ռ��0--100,�û���Ҫ��������״̬���101��ʼ
        public const Int32 NET2991A_AI_SAMPSTS_NONE = 0;                                       // �޲���״̬
        public const Int32 NET2991A_AI_SAMPSTS_REDUCESPEED_FAIL = 1;              // ����ʧ�ܣ� ����ʱ����
        public const Int32 NET2991A_AI_FILE_READCH = 2;                                           // �����ļ�ʱ���ڶ�ȡ��ͨ����,����ʱ����
        public const Int32 NET2991A_AI_SAMPSTS_SAVEFILE = 3;                                 // ���ڱ����ļ�
        public const Int32 NET2991A_AI_SAMPSTS_SAVEFILE_COMPLETE = 4;           // �����ļ����
        public const Int32 NET2991A_AI_SAMPSTS_START_PROCESSFFILE = 5;           // ��ʼ�ļ�����
        public const Int32 NET2991A_AI_SAMPSTS_FINISH_PROCESSFFILE = 6;          // ����ļ�����

        // ################################ DEV�豸��������� ###################################
        
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_Init(UInt32 nReadBufSize);        // ���豸�ɼ���һЩ��ʼ����

        [DllImport("NET2991_32.DLL")]
        public static extern IntPtr NET2991_DEV_Create(                                                  // �����豸������(hDevice), �ɹ�����ʵ�ʾ��,ʧ���򷵻�INVALID_HANDLE_VALUE(-1)
                                                                                    UInt32 nDeviceIP,                      // �豸IP
                                                                                    UInt16 nDevicePort,                   // �豸�˿ں�
                                                                                    UInt16 nLocalPort,                      // ���ض˿ں�
                                                                                    UInt16 nDataTranDir,                  // ���ݴ��䷽�� 0:�ͻ��˷���,1:����������
                                                                                    Double fSendTimeout,                  // ���ͳ�ʱ(��λ:��)
                                                                                    Double fRecvTimeout,                 // ���ճ�ʱ(��λ:��)
                                                                                    Double fFiniteWaitTimeOut);        // ����ģʽ�����ݵȴ���ʱ

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_Release(IntPtr hDevice);                       // �ͷ��豸����

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_IsLink(IntPtr hDevice);                       // �ж��豸�Ƿ���������

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_SetSendSpeed(Single fSendSpeed);                       // �����豸�������ݵ��ٶ�, �豸�������ݵ��ٶ�,1---127,1��죬127����

        // ################################ DEV�豸������������� ################################
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_SetNetInfo(                                                                           // ����������Ϣ
                                                                                        IntPtr hDevice,                                                      // �豸������,����DEV_Create()��������
                                                                                        ref DEVICE_NET_INFO pNetInfo);                     // ������Ϣ�ṹ��

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_GetNetInfo(                                                                           // �õ�������Ϣ
                                                                                        IntPtr hDevice,                                                      // �豸������,����DEV_Create()��������
                                                                                        ref DEVICE_NET_INFO pNetInfo);                     // ������Ϣ�ṹ��

        // ################################ AIģ��������ʵ�ֺ��� ###################################
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_InitTask(                                                                           // �õ�������Ϣ
                                                                                 IntPtr hDevice,                                                      // �豸������,����DEV_Create()��������
                                                                                 ref NET2991A_AI_PARAM pAIParam,                  // AI��������, �����ڴ˺����о���Ӳ����ʼ״̬�͸�����ģʽ
                                                                                 IntPtr pSampEvent);                                             // ���ز����¼�������,���豸�г��ֿɶ����ݶ�ʱ�ᴥ�����¼�������=NULL,��ʾ����Ҫ���¼����

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_ClearBuffer(IntPtr hDevice);                                             // ���AIӲ������

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_StartTask(IntPtr hDevice);                                                 // �����ɼ�����

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SendSoftTrig(IntPtr hDevice);                                            // ������������¼�(Send Software Trigger),�������Ҳ��ǿ�ƴ���

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_GetStatus(                                                                           // ȡ��AI����״̬
                                                                                    IntPtr hDevice,                                                    // �豸������,����DEV_Create()��������
                                                                                    ref NET2991A_AI_STATUS pAIStatus);                // AI״̬�ṹ��

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_GetClockStatus(                                                                   // �õ�ʱ��״̬,�ⲿ10Mʱ��ʱ��Ч
                                                                                    IntPtr hDevice,                                                    // �豸������,����DEV_Create()��������
                                                                                    ref NET2991A_AI_STATUS pAIStatus);                // AI״̬�ṹ��

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReadOffsetAndLength(                                                  // ���ö�ȡ���ݵ�ƫ��λ�úͶ�ȡ����(����ʱģʽ��ʹ��)
                                                                                    IntPtr hDevice,                                                    // �豸������,����DEV_Create()��������
                                                                                    Int32 nReadOffset,                                               // �������ݵ�ƫ��λ�ã��ο����������������е�0λ��(��λ:��)
                                                                                    Int32 nReadLength);                                            // �������ݵĳ���(��λ:��)

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_ReadBinary(                                                                       // ��ȡ��������(������ԭ����������)
                                                                                    IntPtr hDevice,                                                    // �豸������,����DEV_Create()��������
                                                                                    ref UInt32 pReadChannel,                                       // ���صĶ�ȡ���ݵ�ͨ����(����ͨ����),ȡֵ��Χ[0, 16], 0-15��ӦAI0-AI15, 16��ӦDI,����ģʽʹ�ã�����ģʽ==NULL
                                                                                    UInt16[] nBinArray,                                             // ģ����������(������ԭ������),���ڷ��ز����Ķ�����ԭ�����ݣ�ȡֵ�����ɸ�ͨ������ʱ�Ĳ�����Χ����(��λ:V)
                                                                                    UInt32 nReadSampsPerChan,                               // ÿͨ�������ȡ�ĵ���(��λ����)
                                                                                    ref UInt32 pSampsPerChanRead,                         // ����ÿͨ��ʵ�ʶ�ȡ�ĵ���(��λ����), =NULL,��ʾ���뷵��
                                                                                    ref UInt32 pAvailSampsPerChan,                         // �����л����ڵĿɶ�����, =NULL,��ʾ���뷵��
                                                                                    Double fTimeout);                                               // ��ʱʱ�䣬��λ����, -1:�޳�ʱ

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReadComplete(IntPtr hDevice);                                        // ĳ�豸�������

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_StopTask(IntPtr hDevice);                                                     // ĳ�豸�������

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_IsSaveFile(                                                                           // �Ƿ񱣴��ļ�
                                                                                    IntPtr hDevice,                                                     // �豸������,����DEV_Create()��������
                                                                                    UInt32 bSaveFile);                                                // 0���������ļ�(ͼ��������ʾ)��1�������ļ�

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReReadData(                                                                  // �Ƿ񱣴��ļ�
                                                                                    IntPtr hDevice,                                                     // �豸������,����DEV_Create()��������
                                                                                    ref NET2991A_AI_PARAM pAIParam,                 // AI��������, �����ڴ˺����о���Ӳ����ʼ״̬�͸�����ģʽ
                                                                                    ref UInt32 pLastSampsPerChan,                           // ��ʷ����������
                                                                                    UInt32 bReRead);                                                // �����Ƿ����¶�ȡ

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_GetDevSampleStatus(ref DEVICE_SAMP_STS pDevSampSts);      // �õ�dll���豸������״̬

        [DllImport("Ws2_32.dll")]
        public static extern int inet_addr(string ipaddr);

        private static UserDef.CFGPARA cfgPara;
        private static bool needStop = false;

        //static UInt16[,] k_buffer = new UInt16[2, 8000100];
        static UInt32 k_length = 0;
        static DateTime k_lastSavedFileTime = DateTime.Now;
        static UInt16 k_last_pattern_check = 0;
        static UInt32 k_timer_threahold = 0;
        static MsgUpdateDelegate mstd;

        public void stopDevice()
        {
            needStop = true;
        }
        public void releaseDevice()
        {
            if (cfgPara.hDevice != (IntPtr)(-1))
            {
                NET2991A.NET2991_AI_StopTask(cfgPara.hDevice);
                NET2991A.NET2991_DEV_Release(cfgPara.hDevice);
                cfgPara.hDevice = (IntPtr)(-1);
            }
        }

        public bool isReleased()
        {
            return cfgPara.hDevice == (IntPtr)(-1);
        }

        public NET2991A()
        {
            cfgPara.AAIParam.szDevName = new SByte[32];
            cfgPara.AAIParam.CHParam = new NET2991A.NET2991A_CH_PARAM[17];
            NET2991A.NET2991_DEV_Init(4096);
            
        }

       /* static void saveFile()
        {

            mstd("2991A is writting");
            String path = UserDef.PathDir + cfgPara.AAIParam.nDevicePort +"_" + DateTime.Now.ToString("MM-dd-H-mm-ss-ffff") + ".zip";


            using (FileStream file = File.Create(path))
            {


                byte[] buffer = new byte[k_length * sizeof(UInt16)];
                Buffer.BlockCopy(k_buffer, 0, buffer, 0, buffer.Length);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzs.Write(buffer, 0, buffer.Length);
                        gzs.Close();
                    };
                    byte[] cdata = ms.ToArray();
                    file.Write(cdata, 0, cdata.Length);
                }

                file.Close();

            }

        }*/

     
        public static UInt64 currentFrameCount = 0;
        //�������Ĵ�С
        public static UInt16[,,] buffer = new UInt16[2, 16, 1000000];//16 channel, 500k/s
        public static Single fPerLsb = 0.0F;
        static void ReadDataFun()
        {
            UInt16[] nAIArray = new UInt16[2048];
            UInt32 dwReadChan = 0;
            //UInt16[] threaholdMarker = new UInt16[16]; // mark is need to save
            UInt64 bufferCount = 0;
            UInt32 indexPerChannel = 0;
            UInt32 dwReadSampsPerChan = 0;
            UInt32 dwSampsPerChanRead = 0;
            UInt32 dwAvailSampsPerChan = 0;
            double fTimeOut = -1.0;
            Int32 nIndex = 0;
            Single fVoltData = 0.0F;
            
            Int16 tmp = 0;
            switch (cfgPara.AAIParam.CHParam[nIndex].nSampleRange)
            {
                case NET2991A.NET2991A_AI_SAMPRANGE_N10_P10V:
                    fPerLsb = 20000.0F / 65536;
                    break;
                case NET2991A.NET2991A_AI_SAMPRANGE_N5_P5V:
                    fPerLsb = 10000.0F / 65536;
                    break;
                case NET2991A.NET2991A_AI_SAMPRANGE_N2D5_P2D5V:
                    fPerLsb = 5000.0F / 65536;
                    break;
                case NET2991A.NET2991A_AI_SAMPRANGE_N1D25_P1D25V:
                    fPerLsb = 2500.0F / 65536;
                    break;
                default:
                    break;
            }
            // �����û����õĶ�ȡ��������ÿͨ�����ݵĶ�ȡ����
            UInt32 nChanSize = (uint)(cfgPara.nReadLength * 2);
            // ������

           
            while (true)
            {
                try
                {
                    
                    if (needStop)
                        break;
                    dwReadSampsPerChan = 1024;

                    //long t1 = System.DateTime.Now.Ticks;
                    // �����ɼ�ģʽ����������˳��Ϊ16·DI��һ�������㡢CH0��һ�������㡢CH1��һ�������㡢CH2��һ������.....CH15��һ�������㡢16·DI�ڶ���������...
                    if (NET2991A.NET2991_AI_ReadBinary(cfgPara.hDevice, ref dwReadChan, nAIArray, dwReadSampsPerChan, ref dwSampsPerChanRead, ref dwAvailSampsPerChan, fTimeOut) == 0)
                    {
                      
                        if (needStop)
                            break;
                        continue;
                    }
                    /*long t2 = System.DateTime.Now.Ticks;
                    Console.WriteLine("wwwwwwwwww  " + (t2 - start) / 10000000);
                    start = t2;*/
                    if (needStop)
                        break;
                    //16ͨ�����ɼ���1�������ڴ���
                    //�Ƚ����ݷŽ����棬����ȷ��ÿ�ζ�����������ԶС��1�����ݣ����Բ��������
                    for (UInt16 n = 0; n < 1000; n++)
                    {
                        if (((n + 1) << 4) > dwSampsPerChanRead)
                        {
                            break;
                        }
                        int bn = n << 4;

                        for (int i = 0; i < 16; i++)
                        {
                            buffer[currentFrameCount % 2, i, indexPerChannel] = nAIArray[bn + i];
                        }
                        indexPerChannel++;
                    }

                    bufferCount += dwSampsPerChanRead;
                    
                    if (bufferCount < (ulong)UserDef.Frequency*16 + 1 - dwSampsPerChanRead)// 500kps*16
                    { //���������һ֡����
                        continue;
                    }
                    else
                    {
                      
                        bufferCount = 0;
                        indexPerChannel = 0;
                        currentFrameCount++;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            if (cfgPara.hDevice != (IntPtr)(-1))
            {
                Thread.Sleep(500);
                NET2991A.NET2991_AI_StopTask(cfgPara.hDevice);
                NET2991A.NET2991_DEV_Release(cfgPara.hDevice);
                cfgPara.hDevice = (IntPtr)(-1);
            }
        }

        public bool InitDevice(string strIP, string strDevPort, string strLocalPort, MsgUpdateDelegate std)
        {
            Int32 nIndex = 0;

            Int32 ipaddr = inet_addr(strIP);
            cfgPara.AAIParam.nDeviceIP = (UInt32)(System.Net.IPAddress.NetworkToHostOrder(ipaddr));
            cfgPara.AAIParam.nDevicePort = Convert.ToUInt16(strDevPort);
            cfgPara.AAIParam.nLocalPort = Convert.ToUInt16(strLocalPort);
            cfgPara.AAIParam.CHParam[16].bChannelEn = 0;
            for (nIndex = 0; nIndex < 16; nIndex++)
            {
                cfgPara.AAIParam.CHParam[nIndex].bChannelEn = 1;
                cfgPara.AAIParam.CHParam[nIndex].nSampleRange = NET2991A.NET2991A_AI_SAMPRANGE_N5_P5V;
                cfgPara.AAIParam.CHParam[nIndex].nRefGround = NET2991A.NET2991A_AI_REFGND_RSE;
            }

            cfgPara.AAIParam.fSampleRate = UserDef.Frequency; //;
            cfgPara.AAIParam.nSampleMode = NET2991A.NET2991A_AI_SAMPMODE_CONTINUOUS;
            cfgPara.AAIParam.nSampsPerChan = 102400;
            cfgPara.AAIParam.nClockSource = NET2991A.NET2991A_AI_CLOCKSRC_LOCAL;
            cfgPara.AAIParam.nReserved0 = 0;

            cfgPara.AAIParam.nTriggerSource = NET2991A.NET2991A_AI_TRIGSRC_NONE;
            cfgPara.AAIParam.nTriggerDir = NET2991A.NET2991A_AI_TRIGDIR_FALLING;
            cfgPara.AAIParam.fTriggerLevel = 10;
            cfgPara.AAIParam.nDelaySamps = 0;
            cfgPara.AAIParam.nReTriggerCount = 1;

            cfgPara.AAIParam.bMasterEn = 0;
            cfgPara.AAIParam.nReserved1 = 0;
            cfgPara.AAIParam.nReserved2 = 0;

            cfgPara.nReadOffset = 0;
            cfgPara.nReadLength =16*UserDef.Frequency;
            cfgPara.hDevice = (IntPtr)(-1);

            UInt16 nDataTranDir = NET2991A.NET2991A_AI_TRANDIR_CLIENT;

            NET2991A.NET2991_DEV_SetSendSpeed(3);
            if (cfgPara.hDevice != (IntPtr)(-1))
            {
                NET2991A.NET2991_AI_StopTask(cfgPara.hDevice);
                NET2991A.NET2991_DEV_Release(cfgPara.hDevice);
            }
            // �����豸
            cfgPara.hDevice = NET2991A.NET2991_DEV_Create(cfgPara.AAIParam.nDeviceIP,
                                                                                               cfgPara.AAIParam.nDevicePort,
                                                                                               cfgPara.AAIParam.nLocalPort,
                                                                                               nDataTranDir, 0.2, 0.2, 2);
            bool isSuccess = true;
            mstd = std;
            if (cfgPara.hDevice == (IntPtr)(-1))
            {
                mstd("2991A�����豸ʧ��!");
                isSuccess = false;
            }
            // �ж��Ƿ�����
            else if (NET2991A.NET2991_DEV_IsLink(cfgPara.hDevice) == 0)
            {
                mstd("2991A�豸û������!");
                isSuccess = false;
            }

            // ��ʼ���豸
            else if (NET2991A.NET2991_AI_InitTask(cfgPara.hDevice, ref cfgPara.AAIParam, (IntPtr)(-1)) == 0)
            {
                mstd("2991A��ʼ���豸ʧ��!");
                isSuccess = false;
            } // �������ļ�
            else if (NET2991A.NET2991_AI_IsSaveFile(cfgPara.hDevice, 0) == 0)
            {
                mstd("2911A�����ļ�����ʧ��!");
                isSuccess = false;
            }
            // �������
            else if (NET2991A.NET2991_AI_ClearBuffer(cfgPara.hDevice) == 0)
            {
                mstd("2991A�������ʧ��!");
            }



            if (!isSuccess && cfgPara.hDevice != (IntPtr)(-1))
            {
                NET2991A.NET2991_AI_StopTask(cfgPara.hDevice);
                NET2991A.NET2991_DEV_Release(cfgPara.hDevice);
                cfgPara.hDevice = (IntPtr)(-1);
                return false;
            }

            return true;
        }
        public bool start()
        {
            Int32 dwChanCnt = 0;
            Int32 nIndex = 0;
            for (nIndex = 0; nIndex < NET2991A.NET2991A_MAX_CHANNELS; nIndex++)
            {
                if (cfgPara.AAIParam.CHParam[nIndex].bChannelEn == 1)
                {
                    dwChanCnt++;
                }
            }
            // ʵ���ܹ���Ҫ��ȡ��������
            cfgPara.dwRealReadLen = 2 * dwChanCnt * cfgPara.nReadLength;

            // ��ʼ��
            cfgPara.nSampStatus = UserDef.CMD_UNCPT;
            cfgPara.dwReadDataSize = 0;
            cfgPara.bAIStatus = 0;

            // ��ȡ���ݵĳ���
            cfgPara.dwFrameLen = UserDef.LMT_FRMCNT;

            // ��������
            if (NET2991A.NET2991_AI_StartTask(cfgPara.hDevice) == 0)
            {
                Console.WriteLine("2911A��������ʧ��!");
                if (cfgPara.hDevice != (IntPtr)(-1))
                {
                    NET2991A.NET2991_AI_StopTask(cfgPara.hDevice);
                    NET2991A.NET2991_DEV_Release(cfgPara.hDevice);
                    cfgPara.hDevice = (IntPtr)(-1);
                }
                return false;
            }
            needStop = false;
            Thread tDataRead = new Thread(ReadDataFun);
            tDataRead.Start();
            return true;
        }
    }
}
