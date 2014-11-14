using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;

using System.IO;

#if LOG_ON
using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Appender;
using log4net.Repository.Hierarchy;
#endif 

namespace POSH.sys
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
#if LOG_ON
        public log4net.ILog log {get; private set;}
#else
        public ILog log {get; private set;}
#endif
        protected internal bool _debug_ { get; internal set;}
        protected AgentBase m_agent;

        /// <summary>
        /// Returns a list of available attributes.
        /// </summary>
        protected internal Dictionary<string, object> attributes {get; set; }

		public LogBase(string logName) : this (logName, null, null, false)
		{}
		public LogBase(string logName, AgentBase agent) : this (logName, agent, null, false)
		{}

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
        public LogBase(string logName, AgentBase agent, object defaultLevel, bool debug)
        { 
            // workaround for scheduled POSH, where not all plan elements are
            // initialised with an agent -> the given 'agent' attribute does
            // not have an id
            
            _debug_ = debug;
            attributes = new Dictionary<string, object>();

            if (agent == null)
                m_agent = ((AgentBase)this);
            else 
            {
                m_agent = agent;
                string agentId = agent.id != string.Empty ? agent.id : "NOID";

                if (logName == string.Empty){
                    // called for the agent
                    logDomain = agentId;
                }else
                    logDomain = agentId+"."+logName;
#if LOG_ON
                log = LogManager.GetLogger(logDomain);
#else 
                log = new Log();
#endif
            }
#if LOG_ON           
            if (defaultLevel != null)
                ((log4net.Repository.Hierarchy.Logger)log.Logger).Level= defaultLevel as Level;

            log4net.Config.XmlConfigurator.Configure();
#endif
        }
		protected void Init(string id)
		{
			Init (id, "");
		}
        protected void Init(string id, string logName)
        {
            if (id == null)
                return;

            id = (id != string.Empty) ? id : "NOID";

            if (logName == string.Empty){
                // called for the agent
                logDomain = id;
            }else
                logDomain = id+"."+logName;
#if LOG_ON
            log = LogManager.GetLogger(logDomain);
#else
            log = new Log();
#endif
        }

        public Dictionary<string,object> GetAttributes()
        {
            Dictionary<string,object> output = new Dictionary<string,object>();
            foreach (string key in this.attributes.Keys)
            {
                output[key] = this.attributes[key];
            }

            return output;
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
#if LOG_ON
            PatternLayout layout = new PatternLayout("%r [%t] %p %c %x - %m%n");
            StreamHandler writer = new StreamHandler(this);
            
            TextWriterAppender appender= new TextWriterAppender();
            appender.Layout = layout;
            appender.Writer = writer;

            Hierarchy h = LogManager.GetRepository()as Hierarchy;
            h.Root.AddAppender(appender);
#endif
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
        
    }

    public class StreamHandler : StreamWriter
    {
        Stream streamer;

        public StreamHandler(Stream text)
        : base(text){
            streamer=text;
        }
		void Write(byte[] text)
		{
			Write (text, -1, -1);
		}

        void Write(byte[] text, int offset, int length)
		{
            if (offset >=0 && length >=0)
                streamer.Write(text,offset,length);
            else
                streamer.Write(text,0,text.Length);
        }
    }



    
}
