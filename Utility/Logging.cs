/*
	All files containing this header is released under the GPL 3.0 license.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
        This logging class is based on NoFates logging class from a few years ago.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Text;

namespace SteamServer
{
    // Severity.
    public enum LogLevel
    {
        All = 0,
        Data = 1,
        Error = 2,
        Debug = 4,
        Warning = 8,
        Info = 16,
        None = 31
    }

    // Debug log.
    public static class Log
    {
        private static String _FileName; // Output file.
        private static LogLevel _LogLevel; // Log higher priority messages.
        public static StreamWriter _LogWriter; // Write to file.
        public static Queue<String> _LogBuffer = new Queue<string>(10); // Only keep the last 10 in memory.

        public static void Initialize(String Filename, LogLevel LogLevel, bool Append)
        {
            _FileName = Filename;
            _LogLevel = LogLevel;

            if (!Append)
            {
                try
                {
                    File.Delete(Filename);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Old logfile could not be deleted: {0}", e.Message);
                }
            }

            _LogWriter = new StreamWriter(_FileName, true);
        }
        public static void PrintBuffer()
        {
            lock ("LoggingCriticalSection")
            {
                foreach (String S in _LogBuffer)
                    Console.WriteLine(S);
            }
        }
        private static void Write(String Message, LogLevel Level)
        {
            StackFrame Frame = null;
            String Caller = "";

            // Threadsafe.
            lock ("LoggingCriticalSection")
            {
                // C# doesn't provide a __FUNCTION__ preprocessor macro.
                // So we simply check the stack to get the caller.
                Frame = new StackTrace().GetFrame(2);

                // Get the calling methods name.
                if (Frame != null && Frame.GetMethod().DeclaringType != null)
                {
                    Caller = String.Format("{0}: ", Frame.GetMethod().Name);
                }

                // Prepend the error level.
                switch (Level)
                {
                    case LogLevel.Debug:
                        Message = String.Format("{0}: {1}", "DEBUG", Message);
                        break;

                    case LogLevel.Info:
                        Message = String.Format("{0}: {1}", "INFO", Message);
                        break;

                    case LogLevel.Warning:
                        Message = String.Format("{0}: {1}", "WARNING", Message);
                        break;

                    case LogLevel.Error:
                        Message = String.Format("{0}: {1}", "ERROR", Message);
                        break;
                }

                // Check the buffer and make room for our message.
                while (_LogBuffer.Count > 9)
                    _LogBuffer.Dequeue();

                // Save the message.
                _LogBuffer.Enqueue(String.Format("{0} - {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Caller, Message));

                // Log to file.
                if (Level > _LogLevel)
                {
                    _LogWriter.WriteLine(String.Format("{0} - {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), Caller, Message));
                    _LogWriter.Flush();
                }
            }
        }

        // Logging methods.
        public static void Data(String Message)
        {
            Write(Message, LogLevel.Data);
        }
        public static void Error(String Message)
        {
            Write(Message, LogLevel.Error);
        }
        public static void Warning(String Message)
        {
            Write(Message, LogLevel.Warning);
        }
        public static void Info(String Message)
        {
            Write(Message, LogLevel.Info);
        }
        public static void Debug(String Message)
        {
            Write(Message, LogLevel.Debug);
        }
    }
}
