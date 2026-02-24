using System;
using System.IO;

namespace BD.Standard.KY.ReportServicePlugIn
{

    class Logger
    {
        private string _logPath;

        public Logger(string logDirectory, string logFileName)
        {
            _logPath = Path.Combine(logDirectory, logFileName);
        }

        public void WriteLog(string message)
        {
            try
            {
                // 1. 确保日志目录存在
                if (!Directory.Exists(Path.GetDirectoryName(_logPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logPath));
                }

                // 2. 追加内容到日志文件
                using (StreamWriter sw = File.AppendText(_logPath))
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    sw.Flush(); // 可选：立即写入磁盘（默认缓冲写入）
                }
            }
            catch (Exception ex)
            {
                // 处理异常（例如写入失败时打印到控制台）
                Console.WriteLine($"写入日志失败: {ex.Message}");
            }
        }
    }

}
