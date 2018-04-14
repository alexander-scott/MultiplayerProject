using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class Logger
    {
        public static event StringDelegate OnNewLogItem;

        private static Logger instance = null;
        private static readonly object padlock = new object();

        Logger()
        {
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new Logger();
                        }
                    }
                }
                return instance;
            }
        }

        private string DatetimeFormat;
        private string Filename;

        /// <summary>
        /// Initialize a new instance of Logger class.
        /// Log file will be created automatically if not yet exists, else it can be either a fresh new file or append to the existing file.
        /// Default is create a fresh new log file.
        /// </summary>
        /// <param name="append">True to append to existing log file, False to overwrite and create new log file</param>
        public void Initalise(bool append = false)
        {
            DatetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            Filename = Assembly.GetExecutingAssembly().GetName().Name + ".log";

            // Log file header line
            string logHeader = Filename + " is created.";
            if (!File.Exists(Filename))
            {
                WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }
            else
            {
                if (append == false)
                    WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="text">Message</param>
        public void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="text">Message</param>
        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log a fatal error message
        /// </summary>
        /// <param name="text">Message</param>
        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="text">Message</param>
        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        /// <summary>
        /// Log a trace message
        /// </summary>
        /// <param name="text">Message</param>
        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a waning message
        /// </summary>
        /// <param name="text">Message</param>
        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        /// <summary>
        /// Format a log message based on log level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="text">Log message</param>
        private void WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;
            switch (level)
            {
                case LogLevel.TRACE: pretext = DateTime.Now.ToString(DatetimeFormat) + " [TRACE]   "; break;
                case LogLevel.INFO: pretext = DateTime.Now.ToString(DatetimeFormat) + " [INFO]    "; break;
                case LogLevel.DEBUG: pretext = DateTime.Now.ToString(DatetimeFormat) + " [DEBUG]   "; break;
                case LogLevel.WARNING: pretext = DateTime.Now.ToString(DatetimeFormat) + " [WARNING] "; break;
                case LogLevel.ERROR: pretext = DateTime.Now.ToString(DatetimeFormat) + " [ERROR]   "; break;
                case LogLevel.FATAL: pretext = DateTime.Now.ToString(DatetimeFormat) + " [FATAL]   "; break;
                default: pretext = ""; break;
            }

            WriteLine(pretext + text);
        }

        /// <summary>
        /// Write a line of formatted log message into a log file
        /// </summary>
        /// <param name="text">Formatted log message</param>
        /// <param name="append">True to append, False to overwrite the file</param>
        /// <exception cref="System.IO.IOException"></exception>
        private void WriteLine(string text, bool append = true)
        {
            Console.WriteLine(text);

            try
            {
                lock (padlock)
                {
                    using (StreamWriter Writer = new StreamWriter(Filename, append, Encoding.UTF8))
                    {
                        if (text != "") Writer.WriteLine(text);
                    }
                }
            }
            catch
            {
                throw;
            }   
            
            if (OnNewLogItem != null)
            {
                OnNewLogItem(text);
            }
        }

        /// <summary>
        /// Supported log level
        /// </summary>
        [Flags]
        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }

        public List<string> ReadLastLines(int nFromLine, int nNoLines, out bool bMore)
        {
            // Initialise more
            bMore = false;
            try
            {
                char[] buffer = null;
                lock (padlock) // Lock something if you need to....
                {
                    if (File.Exists(Filename))
                    {
                        // Open file
                        using (StreamReader sr = new StreamReader(Filename))
                        {
                            long FileLength = sr.BaseStream.Length;

                            int c, linescount = 0;
                            long pos = FileLength - 1;
                            long PreviousReturn = FileLength;
                            // Process file
                            while (pos >= 0 && linescount < nFromLine + nNoLines) // Until found correct place
                            {
                                // Read a character from the end
                                c = BufferedGetCharBackwards(sr, pos);
                                if (c == Convert.ToInt32('\n'))
                                {
                                    // Found return character
                                    if (++linescount == nFromLine)
                                        // Found last place
                                        PreviousReturn = pos + 1; // Read to here
                                }
                                // Previous char
                                pos--;
                            }
                            pos++;
                            // Create buffer
                            buffer = new char[PreviousReturn - pos];
                            sr.DiscardBufferedData();
                            // Read all our chars
                            sr.BaseStream.Seek(pos, SeekOrigin.Begin);
                            sr.Read(buffer, (int)0, (int)(PreviousReturn - pos));
                            sr.Close();
                            // Store if more lines available
                            if (pos > 0)
                                // Is there more?
                                bMore = true;
                        }
                        if (buffer != null)
                        {
                            // Get data
                            string strResult = new string(buffer);
                            strResult = strResult.Replace("\r", "");

                            // Store in List
                            List<string> strSort = new List<string>(strResult.Split('\n'));
                            // Reverse order
                            strSort.Reverse();

                            return strSort;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ReadLastLines Exception:" + ex.ToString());
            }
            // Lets return a list with no entries
            return new List<string>();
        }

        const int CACHE_BUFFER_SIZE = 1024;
        private long ncachestartbuffer = -1;
        private char[] cachebuffer = null;
        // Cache the file....
        private int BufferedGetCharBackwards(StreamReader sr, long iPosFromBegin)
        {
            // Check for error
            if (iPosFromBegin < 0 || iPosFromBegin >= sr.BaseStream.Length)
                return -1;
            // See if we have the character already
            if (ncachestartbuffer >= 0 && ncachestartbuffer <= iPosFromBegin && ncachestartbuffer + cachebuffer.Length > iPosFromBegin)
            {
                return cachebuffer[iPosFromBegin - ncachestartbuffer];
            }
            // Load into cache
            ncachestartbuffer = (int)Math.Max(0, iPosFromBegin - CACHE_BUFFER_SIZE + 1);
            int nLength = (int)Math.Min(CACHE_BUFFER_SIZE, sr.BaseStream.Length - ncachestartbuffer);
            cachebuffer = new char[nLength];
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(ncachestartbuffer, SeekOrigin.Begin);
            sr.Read(cachebuffer, (int)0, (int)nLength);

            return BufferedGetCharBackwards(sr, iPosFromBegin);
        }
    }
}
