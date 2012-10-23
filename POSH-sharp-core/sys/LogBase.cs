using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using POSH_sharp.sys.strict;
using log4net.Core;
using System.IO;
using log4net.Layout;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace POSH_sharp.sys
{
    /// <summary>
    /// Base for agent-based log messages.
    /// 
    /// This class adds the object variable 'log' to each instance of a subclass
    /// that inherits this class. This log variable is a logging object that is
    /// to be used for creating log messages.
    /// </summary>
    public class LogBase
    {
        public string logDomain {get; private set;}
        public ILog log {public get; private set;}
        internal bool _debug_;

        /// <summary>
        /// Returns a list of available attributes.
        /// </summary>
        internal Dictionary<string, object> attributes {get; set; }

        /// <summary>
        /// Initialises the logger.
        /// 
        /// The logger is initialised to send log messages
        /// under the logging domain [AgentId].[log_name]. The 
        /// name of the agent is retrieved by accessing C{agent.id}
        /// variable.
        /// 
        /// If the logger is initialised for the agent itself,
        /// logName has to be set to an empty string.
        /// </summary>
        /// <param name="agent">A POSH agent.
        /// </param>
        /// <param name="logName">Name of the logging domain, "" if called
        /// for the agent.</param>
        /// <param name="defaultLevel">The default logging level.</param>
        public LogBase(string logName, AgentBase agent=null, Level defaultLevel = null)
        { 
            // workaround for scheduled POSH, where not all plan elements are
            // initialised with an agent -> the given 'agent' attribute does
            // not have an id
            _debug_ = false;
            if (agent == null)
                agent = ((AgentBase)this);
            string agentId = agent.id != string.Empty ? agent.id : "NOID";

            if (logName == string.Empty){
                // called for the agent
                logDomain = agentId;
            }else
                logDomain = agentId+"."+logName;
            log = LogManager.GetLogger(logDomain);
            
            if (defaultLevel != null)
                ((log4net.Repository.Hierarchy.Logger)log.Logger).Level= defaultLevel;
        }
        

    }

    /// <summary>
    /// Class to direct streamed log messages to some output.
    ///    
    /// To use this class, it needs to be inherited and its write() method needs
    /// to be overridden to handle new incoming log messages.
    /// </summary>
    public class StreamLogger : MemoryStream
    {
        /// <summary>
        /// Initialises the stream logger.
        /// </summary>
        public void init(){
            PatternLayout layout = new PatternLayout("%r [%t] %p %c %x - %m%n");
            StreamHandler writer = new StreamHandler(this);
            
            TextWriterAppender appender= new TextWriterAppender();
            appender.Layout = layout;
            appender.Writer = writer;
            Hierarchy h = LogManager.GetRepository()as Hierarchy;
            h.Root.AddAppender(appender);
        }
        /// <summary>
        /// Handes the new debug message.
        /// 
        /// This method needs to be overridden by an inheriting class to handle
        /// log messages. By default, it print the message to the console.
        /// </summary>
        /// <param name="msg">The log message. If a newline is required, then the
        /// message contains the required '\n' character.</param>
        public void write(string msg){
            Console.Write(msg);
        }
        
        /// <summary>
        /// Sets up basic console logging at the given log level.
        /// </summary>
        /// <param name="level"></param>
        public static void setupConsoleLogging(Level level){

            log4net.Config.BasicConfigurator.Configure();
        
            Hierarchy h = LogManager.GetRepository()as Hierarchy;
            h.Root.Level =level;
        }

    }

    public class StreamHandler : StreamWriter
    {
        Stream streamer;

        public StreamHandler(Stream text)
        : base(text){
            streamer=text;
        }
        void Write(byte[] text, int offset =-1, int length=-1){
            if (offset >=0 && length >=0)
                streamer.Write(text,offset,length);
            else
                streamer.Write(text,0,text.Length);
        }
    }



    
}
