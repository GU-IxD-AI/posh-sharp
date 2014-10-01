using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace POSH.sys
{
    public interface ILog
    {
         void Debug(object message, Exception exception);

         void Debug(object message);

         void DebugFormat(string format, object arg0, object arg1, object arg2);

         void DebugFormat(string format, object arg0, object arg1);

         void DebugFormat(string format, object arg0);

         void DebugFormat(string format, params object[] args);

         void Error(object message, Exception exception);

         void Error(object message);

         void ErrorFormat(string format, object arg0, object arg1, object arg2);

         void ErrorFormat(string format, object arg0, object arg1);

         void ErrorFormat(string format, object arg0);

         void ErrorFormat(string format, params object[] args);

         void Fatal(object message, Exception exception);

         void Fatal(object message);

         void FatalFormat(string format, object arg0, object arg1, object arg2);

         void FatalFormat(string format, object arg0, object arg1);

         void FatalFormat(string format, object arg0);

         void FatalFormat(string format, params object[] args);

         void Info(object message, Exception exception);

         void Info(object message);

         void InfoFormat(string format, object arg0, object arg1, object arg2);

         void InfoFormat(string format, object arg0, object arg1);

         void InfoFormat(string format, object arg0);

         void InfoFormat(string format, params object[] args);

         void Warn(object message, Exception exception);

         void Warn(object message);

         void WarnFormat(string format, object arg0, object arg1, object arg2);

         void WarnFormat(string format, object arg0, object arg1);

         void WarnFormat(string format, object arg0);

         void WarnFormat(string format, params object[] args);

    }
}
