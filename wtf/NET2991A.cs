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
        public const Int32 STATUS_RUNNING = 1;           // 正在采集
        public const Int32 STATUS_STOPED = 2;              // 采集开始或采集完成

        /////////////////////////////////////////////////////////////////////////
        public const Int32 NET2991A_MAX_CHANNELS = 17;                   // 本卡最多支持17路模拟量和数字量输入
        public const Int32 NET2991A_AI_MAX_CHANNELS = 16;             // 本卡最多支持16路模拟量单端输入通道
        public const Int32 NET2991A_DI_MAX_PORTS = 1;                       // 本卡最多支持1个数字量端口
        public const Int32 NET2991A_DIO_PORT0_MAX_LINES = 16;      // 数字量端口0支持16条线
        public const Int32 NET2991A_MAX_DEVICENAME_LEN = 32;    // 设备名字长度

        //#################### AI硬件参数USB2851_AI_PARAM定义 #####################
        // 通道参数结构体
        public struct NET2991A_CH_PARAM
        {
            public UInt32 bChannelEn;             // BYTE[3:0] 通道使能
            public UInt32 nSampleRange;         // BYTE[7:4] 采样范围(Sample Range)档位选择，详见下面常量定义
            public UInt32 nRefGround;             // BYTE[11:8] 地参考方式：	0:单端输入; 1:差分输入
            public UInt32 nReserved0;             // BYTE[15:12] 保留(暂未定义)
            public UInt32 nReserved1;             // BYTE[19:16] 保留(暂未定义)
            public UInt32 nReserved2;             // BYTE[23:20] 保留(暂未定义)
        }

        // AI硬件参数结构体NET2991A_CH_PARAM中NET2991A_CH_PARAM结构体数组中的nSampleRange采样范围所使用的选项
        public const UInt32 NET2991A_AI_SAMPRANGE_N10_P10V = 0;               // ±10V
        public const UInt32 NET2991A_AI_SAMPRANGE_N5_P5V = 1;                   // ±5V
        public const UInt32 NET2991A_AI_SAMPRANGE_N2D5_P2D5V = 2;          // ±2.5V
        public const UInt32 NET2991A_AI_SAMPRANGE_N1D25_P1D25V = 3;       // ±1.25V

        // AI硬件参数结构体NET2991A_CH_PARAM中NET2991A_CH_PARAM结构体数组中的nRefGround参考地模式所使用的选项
        public const UInt32 NET2991A_AI_REFGND_RSE = 0;              // 接地参考单端(Referenced Single Endpoint)
        public const UInt32 NET2991A_AI_REFGND_DIFF = 1;             // 差分(Differential)

        public struct NET2991A_AI_PARAM
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public SByte[] szDevName;                                               // [31:0]  名称不超过15个汉字，即30个字节
            public UInt32 nDeviceIP;                                                   // [35:32] 设备IP地址
            public UInt16 nDevicePort;                                                // [37:36] 设备端口号
            public UInt16 nLocalPort;                                                  // [39:38] 本机端口号
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public NET2991A_CH_PARAM[] CHParam;                       // [447:40] 通道参数配置, 0-15对应AI0-AI15, 16对应DI

            public double fSampleRate;                                                // [455:448] 采样速率[0.00931sps, 1000000sps]
            public UInt32 nSampleMode;                                             // [459:456] 采样模式  2:有限采样, 3:连续采样 
            public UInt32 nSampsPerChan;                                          // [463:460] 设备每个通道采样点数(也是每通道待读取点数)(单位:字)
            public UInt32 nClockSource;                                              // [467:464] 时钟源 0:内时钟; 1:外部参考10M时钟; 2:主卡时钟; 3:外部转换时钟; 
            public UInt32 nReserved0;                                                 // [471:468] 保留(暂未定义)

            public UInt32 nTriggerSource;                                            // [475:472] 触发源选择 0:软件触发; 1:外部数据量触发; 2:外部模拟量触发; 3:同步触发 
            public UInt32 nTriggerDir;                                                  // [479:476] 触发方向 0:下降沿触发; 1:上升沿触发; 2/3:上升下降均触发(变化)
            public Single fTriggerLevel;                                                // [483:480] 触发电平(V)
            public Int32 nDelaySamps;                                                 // [487:484] 触发延迟点数(单位:字)
            public UInt32 nReTriggerCount;                                         // [491:488] 重复触发次数 仅在有限采样的后触发及硬件延时触发模式下

            public UInt32 bMasterEn;                                                  // [495:492] 主设备使能 0:从设备; 1:主设备

            public UInt32 nReserved1;                                                 // [499:496] 保留(暂未定义)
            public UInt32 nReserved2;                                                 // [503:500] 保留(暂未定义)
        }

        // AI硬件数据传输的方向
        public const Int32 NET2991A_AI_TRANDIR_CLIENT = 0;             // 数据传输方向为客户端
        public const Int32 NET2991A_AI_TRANDIR_SERVER = 1;            // 数据传输方向为服务器

        // AI硬件参数结构体NET2991A_AI_PARAM中的nSampleMode采样模式所使用的选项
        public const Int32 NET2991A_AI_SAMPMODE_FINITE = 2;                              // 有限采样
        public const Int32 NET2991A_AI_SAMPMODE_CONTINUOUS = 3;                 // 连续采样

        // AI硬件参数结构体NET2991A_AI_PARAM中的nTriggerSource触发源信号所使用的选项
        public const Int32 NET2991A_AI_TRIGSRC_NONE = 0;                         // 无触发(等价于软件触发)
        public const Int32 NET2991A_AI_TRIGSRC_DIGITAL = 1;                     // 外部数字量触发
        public const Int32 NET2991A_AI_TRIGSRC_ANALOG = 2;                    // 外部模拟量触发
        public const Int32 NET2991A_AI_TRIGSRC_SYNC = 3;                          // 同步触发

        // AI硬件参数结构体NET2991A_AI_PARAM中的nTriggerDir触发方向所使用的选项
        public const Int32 NET2991A_AI_TRIGDIR_FALLING = 0;                       // 下降沿触发
        public const Int32 NET2991A_AI_TRIGDIR_RISING = 1;                          // 上升沿触发
        public const Int32 NET2991A_AI_TRIGDIR_CHANGE = 2;                       // 变化(上升下降均触发)

        // AI硬件参数结构体NET2991A_AI_PARAM中的nClockSource时钟源所使用的选项
        public const Int32 NET2991A_AI_CLOCKSRC_LOCAL = 0;                      // 本地时钟(通常为本地晶振时钟OSCCLK),也叫内部时钟
        public const Int32 NET2991A_AI_CLOCKSRC_CLKIN_10M = 1;              // 外部参考10M时钟定时触发
        public const Int32 NET2991A_AI_CLOCKSRC_MAIN_BOARD = 2;         // 主卡时钟
        public const Int32 NET2991A_AI_CLOCKSRC_CLKIN = 3;                       // 外部采样时钟

        // 用于AI采样的硬件工作状态
        public struct NET2991A_AI_STATUS
        {
            public UInt32 bTaskDone;                                                          // [3:0]   AI采样任务是否结束，1:表示已结束; 0:表示未结束
            public UInt32 bTriggered;                                                           // [7:4]   AI是否被触发，1:表示已被触发; 0:表示未被触发(默认)
            public UInt32 bFreq10M;                                                           // [11:8]  外部输入信号是否10M
            public UInt32 nClockInFreq;                                                      // [15:12] 外部时钟信号频率, 单位Hz

            public UInt32 nSampTaskState;                                                  // [19:16] 采样任务状态, =1:正常, 其它值表示有异常情况
            public UInt32 nAvailSampsPerChan;                                          // [23:20] 每通道有效点数，只有它大于当前指定读数长度时才能调用AI_ReadAnalog()立即读取指定长度的采样数据
            public UInt32 nMaxAvailSampsPerChan;                                    // [27:24] 自AI_StartTask()后每通道出现过的最大有效点数，状态值范围[0, nBufSampsPerChan],它是为监测采集软件性能而提供，如果此值越趋近于1，则表示意味着性能越高，越不易出现溢出丢点的可能
            public UInt32 nBufSampsPerChan;                                             // [31:28] 每通道缓冲区大小(采样点数)
            public Int64 nSampsPerChanAcquired;                                       // [39:32] 每通道已采样点数(自启动AI_StartTask()之后所采样的点数)，这个只是给用户的统计数据

            public UInt32 nHardOverflowCnt;                                              // [43:40] 硬件溢出计数(在不溢出情况下恒等于0)
            public UInt32 nSoftOverflowCnt;                                               // [47:44] 软件溢出计数(在不溢出情况下恒等于0)
            public UInt32 nInitTaskCnt;                                                       // [51:48] 初始化采样任务的次数(即调用AI_InitTask()的次数)
            public UInt32 nReleaseTaskCnt;                                                // [55:52] 释放采样任务的次数(即调用AI_ReleaseTask()的次数)
            public UInt32 nStartTaskCnt;                                                     // [59:56] 启动采样任务的次数(即调用AI_StartTask()的次数)
            public UInt32 nStopTaskCnt;                                                      // [63:60] 停止采样任务的次数(即调用AI_StopTask()的次数)
            public UInt32 nTransRate;                                                         // [67:64] 传输速率, 即每秒传输点数(sps)，作为USB及应用软件传输性能的监测信息

            public UInt32 nReserved0;                                                         // [71:68] 保留字段(暂未定义)
            public UInt32 nReserved1;                                                         // [75:72] 保留字段(暂未定义)
            public UInt32 nReserved2;                                                         // [79:76] 保留字段(暂未定义)
            public UInt32 nReserved3;                                                         // [83:80] 保留字段(暂未定义)
            public UInt32 nReserved4;                                                         // [87:84] 保留字段(暂未定义)
        }

        // 设备网络参数信息结构体
        public struct DEVICE_NET_INFO
        {
            public UInt32 nDeviceIP;                       // [3:0]   IP地址  192.168.0.1
            public UInt16 nDevicePort;                    // [5:4]   端口号 最大65535
            public UInt16 nReserved0;                     // [7:6]   保留字段(暂未定义)
            public UInt64 nMAC;                            // [15:8]  网卡物理地址,用户一般不可更改
            public UInt32 bOneline;                         // [19:16] 在线状态，1:在线; 0:离线(下线)
            public UInt32 nSubnetMask;                  // [23:20] 子网掩码, "255.255.255.0"
            public UInt32 nGateway;                       // [27:24] 网关, "192.168.0.1"
            public UInt32 nReserved1;                     // [31:28] 保留字段(暂未定义)
        }

        // 得到dll内部设备采样状态
        public struct DEVICE_SAMP_STS
        {
            public UInt32 nDevIP;                               // [0:3]设备IP
            public Int32 nSampSts;                               // [4:7]设备状态

            public Int32 nChan;                                    // [8:11]当前通道号，有限使用
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public SByte[] szFileName;                        // [12:267]文件名
            public UInt32 nReserved0;                          // [268:271]保留未用
            public UInt64 ullFileSize;                             // [272:279]文件大小
        }

        // 设备数据读取完成信息结构体
        public struct DEVICE_SAMP_CMP
        {
            public UInt32 nDevIP;                   // [0:3]设备IP
            public UInt32 bSampCmp;             // [4:7]设备数据读取完成标志 0：未完成; 1：完成
        }

        // 采样状态定义 内部定义,已占用0--100,用户需要定义其它状态请从101开始
        public const Int32 NET2991A_AI_SAMPSTS_NONE = 0;                                       // 无采样状态
        public const Int32 NET2991A_AI_SAMPSTS_REDUCESPEED_FAIL = 1;              // 降速失败， 有限时返回
        public const Int32 NET2991A_AI_FILE_READCH = 2;                                           // 保存文件时正在读取的通道号,有限时返回
        public const Int32 NET2991A_AI_SAMPSTS_SAVEFILE = 3;                                 // 正在保存文件
        public const Int32 NET2991A_AI_SAMPSTS_SAVEFILE_COMPLETE = 4;           // 保存文件完成
        public const Int32 NET2991A_AI_SAMPSTS_START_PROCESSFFILE = 5;           // 开始文件处理
        public const Int32 NET2991A_AI_SAMPSTS_FINISH_PROCESSFFILE = 6;          // 完成文件处理

        // ################################ DEV设备对象管理函数 ###################################
        
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_Init(UInt32 nReadBufSize);        // 对设备采集做一些初始工作

        [DllImport("NET2991_32.DLL")]
        public static extern IntPtr NET2991_DEV_Create(                                                  // 创建设备对象句柄(hDevice), 成功返回实际句柄,失败则返回INVALID_HANDLE_VALUE(-1)
                                                                                    UInt32 nDeviceIP,                      // 设备IP
                                                                                    UInt16 nDevicePort,                   // 设备端口号
                                                                                    UInt16 nLocalPort,                      // 本地端口号
                                                                                    UInt16 nDataTranDir,                  // 数据传输方向 0:客户端方向,1:服务器方向
                                                                                    Double fSendTimeout,                  // 发送超时(单位:秒)
                                                                                    Double fRecvTimeout,                 // 接收超时(单位:秒)
                                                                                    Double fFiniteWaitTimeOut);        // 有限模式下数据等待超时

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_Release(IntPtr hDevice);                       // 释放设备对象

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_IsLink(IntPtr hDevice);                       // 判断设备是否真正连接

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_SetSendSpeed(Single fSendSpeed);                       // 设置设备发送数据的速度, 设备发送数据的速度,1---127,1最快，127最慢

        // ################################ DEV设备网络参数管理函数 ################################
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_SetNetInfo(                                                                           // 设置网络信息
                                                                                        IntPtr hDevice,                                                      // 设备对象句柄,它由DEV_Create()函数创建
                                                                                        ref DEVICE_NET_INFO pNetInfo);                     // 网络信息结构体

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_DEV_GetNetInfo(                                                                           // 得到网络信息
                                                                                        IntPtr hDevice,                                                      // 设备对象句柄,它由DEV_Create()函数创建
                                                                                        ref DEVICE_NET_INFO pNetInfo);                     // 网络信息结构体

        // ################################ AI模拟量输入实现函数 ###################################
        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_InitTask(                                                                           // 得到网络信息
                                                                                 IntPtr hDevice,                                                      // 设备对象句柄,它由DEV_Create()函数创建
                                                                                 ref NET2991A_AI_PARAM pAIParam,                  // AI工作参数, 它仅在此函数中决定硬件初始状态和各工作模式
                                                                                 IntPtr pSampEvent);                                             // 返回采样事件对象句柄,当设备中出现可读数据段时会触发此事件，参数=NULL,表示不需要此事件句柄

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_ClearBuffer(IntPtr hDevice);                                             // 清除AI硬件缓存

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_StartTask(IntPtr hDevice);                                                 // 启动采集任务

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SendSoftTrig(IntPtr hDevice);                                            // 发送软件触发事件(Send Software Trigger),软件触发也叫强制触发

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_GetStatus(                                                                           // 取得AI各种状态
                                                                                    IntPtr hDevice,                                                    // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    ref NET2991A_AI_STATUS pAIStatus);                // AI状态结构体

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_GetClockStatus(                                                                   // 得到时钟状态,外部10M时钟时有效
                                                                                    IntPtr hDevice,                                                    // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    ref NET2991A_AI_STATUS pAIStatus);                // AI状态结构体

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReadOffsetAndLength(                                                  // 设置读取数据的偏移位置和读取长度(有限时模式下使用)
                                                                                    IntPtr hDevice,                                                    // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    Int32 nReadOffset,                                               // 采样数据的偏移位置，参考点是整个采样序列的0位置(单位:字)
                                                                                    Int32 nReadLength);                                            // 采样数据的长度(单位:字)

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_ReadBinary(                                                                       // 读取采样数据(二进制原码数据序列)
                                                                                    IntPtr hDevice,                                                    // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    ref UInt32 pReadChannel,                                       // 返回的读取数据的通道号(物理通道号),取值范围[0, 16], 0-15对应AI0-AI15, 16对应DI,有限模式使用，连续模式==NULL
                                                                                    UInt16[] nBinArray,                                             // 模拟数据数组(二进制原码数组),用于返回采样的二进制原码数据，取值区间由各通道采样时的采样范围决定(单位:V)
                                                                                    UInt32 nReadSampsPerChan,                               // 每通道请求读取的点数(单位：点)
                                                                                    ref UInt32 pSampsPerChanRead,                         // 返回每通道实际读取的点数(单位：点), =NULL,表示无须返回
                                                                                    ref UInt32 pAvailSampsPerChan,                         // 任务中还存在的可读点数, =NULL,表示无须返回
                                                                                    Double fTimeout);                                               // 超时时间，单位：秒, -1:无超时

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReadComplete(IntPtr hDevice);                                        // 某设备采样完成

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_StopTask(IntPtr hDevice);                                                     // 某设备采样完成

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_IsSaveFile(                                                                           // 是否保存文件
                                                                                    IntPtr hDevice,                                                     // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    UInt32 bSaveFile);                                                // 0：不保存文件(图形数字显示)，1：保存文件

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_AI_SetReReadData(                                                                  // 是否保存文件
                                                                                    IntPtr hDevice,                                                     // 设备对象句柄,它由DEV_Create()函数创建
                                                                                    ref NET2991A_AI_PARAM pAIParam,                 // AI工作参数, 它仅在此函数中决定硬件初始状态和各工作模式
                                                                                    ref UInt32 pLastSampsPerChan,                           // 历史待采样点数
                                                                                    UInt32 bReRead);                                                // 设置是否重新读取

        [DllImport("NET2991_32.DLL")]
        public static extern Int32 NET2991_GetDevSampleStatus(ref DEVICE_SAMP_STS pDevSampSts);      // 得到dll里设备采样的状态

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
        //缓冲区的大小
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
            // 根据用户设置的读取长度设置每通道数据的读取长度
            UInt32 nChanSize = (uint)(cfgPara.nReadLength * 2);
            // 读数据

           
            while (true)
            {
                try
                {
                    
                    if (needStop)
                        break;
                    dwReadSampsPerChan = 1024;

                    //long t1 = System.DateTime.Now.Ticks;
                    // 连续采集模式下数据排列顺序为16路DI第一个采样点、CH0第一个采样点、CH1第一个采样点、CH2第一采样点.....CH15第一个采样点、16路DI第二个采样点...
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
                    //16通道，采集够1秒数据在处理
                    //先将数据放进缓存，这里确认每次读回来的数据远小于1秒数据，所以不考虑溢出
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
                    { //如果还不够一帧数据
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
            // 创建设备
            cfgPara.hDevice = NET2991A.NET2991_DEV_Create(cfgPara.AAIParam.nDeviceIP,
                                                                                               cfgPara.AAIParam.nDevicePort,
                                                                                               cfgPara.AAIParam.nLocalPort,
                                                                                               nDataTranDir, 0.2, 0.2, 2);
            bool isSuccess = true;
            mstd = std;
            if (cfgPara.hDevice == (IntPtr)(-1))
            {
                mstd("2991A创建设备失败!");
                isSuccess = false;
            }
            // 判断是否连接
            else if (NET2991A.NET2991_DEV_IsLink(cfgPara.hDevice) == 0)
            {
                mstd("2991A设备没有连接!");
                isSuccess = false;
            }

            // 初始化设备
            else if (NET2991A.NET2991_AI_InitTask(cfgPara.hDevice, ref cfgPara.AAIParam, (IntPtr)(-1)) == 0)
            {
                mstd("2991A初始化设备失败!");
                isSuccess = false;
            } // 不保存文件
            else if (NET2991A.NET2991_AI_IsSaveFile(cfgPara.hDevice, 0) == 0)
            {
                mstd("2911A设置文件保存失败!");
                isSuccess = false;
            }
            // 清除缓存
            else if (NET2991A.NET2991_AI_ClearBuffer(cfgPara.hDevice) == 0)
            {
                mstd("2991A清除缓存失败!");
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
            // 实际总共需要读取的数据数
            cfgPara.dwRealReadLen = 2 * dwChanCnt * cfgPara.nReadLength;

            // 初始化
            cfgPara.nSampStatus = UserDef.CMD_UNCPT;
            cfgPara.dwReadDataSize = 0;
            cfgPara.bAIStatus = 0;

            // 读取数据的长度
            cfgPara.dwFrameLen = UserDef.LMT_FRMCNT;

            // 启动采样
            if (NET2991A.NET2991_AI_StartTask(cfgPara.hDevice) == 0)
            {
                Console.WriteLine("2911A启动任务失败!");
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
