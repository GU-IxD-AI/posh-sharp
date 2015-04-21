using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace POSH.sys
{
    class Log : ILog
    {

        private StreamWriter output;
        public enum LogState { DEBUG, ERROR, FATAL, INFO, WARN };
        public LogState state { get; private set; }
        
        public void SetLog(StreamWriter chan)
        {
            output = chan;
        }

        public void Debug(object message, Exception exception)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
                output.WriteLine(exception.StackTrace);
            }
        }

        public void Debug(object message)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
            }
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            
        }

        public void DebugFormat(string format, object arg0)
        {
            
        }

        public void DebugFormat(string format, params object[] args)
        {
            
        }

        public void Error(object message, Exception exception)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
                output.WriteLine(exception.StackTrace);
            }
        }

        public void Error(object message)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
            }
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            
        }

        public void ErrorFormat(string format, object arg0)
        {
            
        }

        public void ErrorFormat(string format, params object[] args)
        {
            
        }

        public void Fatal(object message, Exception exception)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
                output.WriteLine(exception.StackTrace);
            }
        }

        public void Fatal(object message)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
            }
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            
        }

        public void FatalFormat(string format, object arg0)
        {
            
        }

        public void FatalFormat(string format, params object[] args)
        {
            
        }

        public void Info(object message, Exception exception)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
            }
        }

        public void Info(object message)
        {
            if (output is StreamWriter)
            {
                output.WriteLine(message);
            }
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            
        }

        public void InfoFormat(string format, object arg0)
        {
            
        }

        public void InfoFormat(string format, params object[] args)
        {
            
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public void Warn(object message, Exception exception)
        {
            
        }

        public void Warn(object message)
        {
            
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            
        }

        public void WarnFormat(string format, object arg0)
        {
            
        }

        public void WarnFormat(string format, params object[] args)
        {
            
        }
    }
}
