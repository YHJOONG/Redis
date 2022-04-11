using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using StackExchange.Redis;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace RedisSubscribe
{
    class Program
    {
        private ConnectionMultiplexer redisConnection;

        private static ConfigurationOptions options;
        public static string m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string g_strToday;

        private static readonly log4net.ILog log = SetCheckFile("logs\\infos\\info", "CheckFileRepo", "SubScribe");
        private static readonly log4net.ILog log_ohlcv_1m = SetCheckFile("logs\\ohlcvs\\ohlcv_1m", "CheckFileRepo2", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_5m = SetCheckFile("logs\\ohlcvs\\ohlcv_5m", "CheckFileRepo6", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_15m = SetCheckFile("logs\\ohlcvs\\ohlcv_15m", "CheckFileRepo7", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_10m = SetCheckFile("logs\\ohlcvs\\ohlcv_10m", "CheckFileRepo8", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_30m = SetCheckFile("logs\\ohlcvs\\ohlcv_30m", "CheckFileRepo9", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_1h = SetCheckFile("logs\\ohlcvs\\ohlcv_1h", "CheckFileRepo10", "SubScribe-1");
        private static readonly log4net.ILog log_ohlcv_1d = SetCheckFile("logs\\ohlcvs\\ohlcv_1d", "CheckFileRepo11", "SubScribe-1");
        private static readonly log4net.ILog log_tr = SetCheckFile("logs\\trs\\tr", "CheckFileRepo3", "SubScribe-2");
        private static readonly log4net.ILog log_hoga = SetCheckFile("logs\\hogas\\hoga", "CheckFileRepo4", "SubScribe-3");
        private static readonly log4net.ILog log_ticker = SetCheckFile("logs\\tickers\\ticker", "CheckFileRepo5", "SubScribe-4");

        public static ILog SetCheckFile(string file, string strRepository, string strRepositoryName)
        {
            String FilePath;
            Hierarchy hierarchy = new Hierarchy();
            //XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
            RollingFileAppender rollingAppender = new RollingFileAppender();
            PatternLayout layout = new PatternLayout();

            ILog log;

            FilePath = file;

            hierarchy.Configured = true;

            rollingAppender.ImmediateFlush = true;
            rollingAppender.File = FilePath;
            rollingAppender.AppendToFile = true;
            rollingAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            rollingAppender.Encoding = Encoding.UTF8;

            rollingAppender.DatePattern = "_yyyy-MM-dd\".log\""; // 날짜가 지나간 경우 이전 로그에 붙을 이름 구성
            rollingAppender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            //rollingAppender.StaticLogFileName = false;
            rollingAppender.MaxSizeRollBackups = 100;
            rollingAppender.MaximumFileSize = "50MB";
            layout = new log4net.Layout.PatternLayout("[%-23d] [%-5p] [%c] %M: %m %n");
            rollingAppender.Layout = layout;
            rollingAppender.ActivateOptions();

            log4net.Repository.ILoggerRepository repository = LogManager.CreateRepository(strRepository);
            BasicConfigurator.Configure(repository, rollingAppender);
            log = LogManager.GetLogger(strRepository, strRepositoryName);

            return log;
        }


        public static long g_info_fp = 0;
        public static void LogWrite(string logMessage)
        {
            try
            {
                DateTime dt = DateTime.UtcNow;
                g_strToday = dt.ToString("yyyyMMdd");

                bool exists = Directory.Exists(m_exePath + "\\" + g_strToday);

                if (!exists)
                {
                    Directory.CreateDirectory(m_exePath + "\\" + g_strToday);
                }
               
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + g_strToday + "\\info.log"))
                {
                    LogFile(logMessage, w);
                    log.Debug(logMessage);
                    //log4NetLib.Debug(logMessage);
                   
                }
            }
            catch (Exception ex)
            {
               
            }
            finally
            {
               
            }
        }

      
        public static void LogFile(string logMessage, TextWriter txtWriter)
        {
            try
            {
                string dateTime = DateTime.UtcNow.ToString("s");
                txtWriter.WriteLine($"{dateTime} | {logMessage}");

            }
            catch (Exception ex)
            {
                LogWrite(ex.Message); 
            }
        }

        static void Main(string[] args)
        {

            XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            options = new ConfigurationOptions
            {
                EndPoints = { { "127.0.0.1", 6383} }, //상용
            
                AbortOnConnectFail = false,
                ReconnectRetryPolicy = new LinearRetry(5000),
                ConnectRetry = 10,
            };

            using (var con = ConnectionMultiplexer.Connect(options))
            {
                var sub = con.GetSubscriber();

                sub.Subscribe("topic", (c, v) =>
                {
                    string tmp = v.ToString().Replace("\r\n", "").Replace(" ", "");
                    //Console.WriteLine("Receive Message : " + tmp);
                    string dateTime = DateTime.UtcNow.ToString("s");
                    string data = $"{dateTime} | {tmp}";
                    //Console.WriteLine(data);
                    log_hoga.Debug(data);
                });

                sub.Subscribe("topic", (c, v) =>
                {
                    string tmp = v.ToString().Replace("\r\n", "").Replace(" ", "");
                    //Console.WriteLine("Receive Message : " + tmp);
                    string dateTime = DateTime.UtcNow.ToString("s");
                    string data = $"{dateTime} | {tmp}";
                    //Console.WriteLine(data);
                    log_tr.Debug(data);
                });

                sub.Subscribe("topic", (c, v) =>
                {
                    string tmp = v.ToString().Replace("\r\n", "").Replace(" ", "");
                    //Console.WriteLine("Receive Message : " + tmp);
                    string dateTime = DateTime.UtcNow.ToString("s");
                    string data = $"{dateTime} | {tmp}";
                    //Console.WriteLine(data);
                    log_ticker.Debug(data);
                });

                // Restarting server.
                Console.ReadKey();

            }
        }
    }
}
