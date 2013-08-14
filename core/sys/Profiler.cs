using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace POSH_sharp.sys
{
    /**
     * This class was written by Philipp Rohlfshagen to report the data found in Rohlfshagen & Bryson (2008).
     * It should only be included if profiling is desirable.  For an example of its use, see library latchTest and 
     * the experiment script replication-scripts/profiled_experiment_executer.py
    
     * Note that this file is not currently well-written, but has a great deal of special-purpose code in it.
     * For example, a data directory is hardcoded below.  This script does not create the directory; this must all be set up.

     * You will want to save results you publish somewhere archival after the run.
    
     * If you want to use this, you need to turn on profiling in agent_base.py before loading the behaviours.  Do this by 
     *     POSH.profiler.Profiler.turnOnProfiling()
     * Comment by JJB, 29 Feb 2008 (updated 2 April)
     * 
     * reimpl 9.June2012 by conversion to csparp (Swen Gaudl)
     */
    public class Profiler
    {
        public int LIMIT = 5001;
        string directory = AssemblyControl.GetControl().getRootPath() + "/replication-scripts/data/";
        string name;
        string secondName;
        string info;
        Dictionary<string, int> logger;
        Dictionary<string, string> associations;

        Dictionary<string,int> counts;
        Dictionary<string,float> avgs;

        int totalCalls;

        private Profiler(string name)
        {
            this.name = name;
            this.logger = new Dictionary<string, int>();
            this.associations = new Dictionary<string, string>();

            this.counts = new Dictionary<string,int>();
            this.avgs = new Dictionary<string,float>();
            this.totalCalls = 0;

            initProfile = new InitProfile(_initProfile);
        }


        public delegate Profiler InitProfile(AgentBase other);
        public static InitProfile initProfile;
        public delegate bool ActionMethod();

        // this normally does nothing.  If you want it to do something, you have to reset it!  
        // (see next two functions)  
        private static Profiler _initProfile(AgentBase other)
        {
            return null;
        }
        private static Profiler _reallyInitProfile(AgentBase other)
        {
            other.profiler = new Profiler(other.id);
            return other.profiler;
        }

        // call this in your init_world to turn on profiling.  This changes what POSH.agent_base.AgentBase effectively does.
        // this is a class method and needs no instance
        public static void turnOnProfiling()
        {
            initProfile = new InitProfile(_reallyInitProfile);

            //turnOnProfiling = staticmethod(turnOnProfiling)
            //_reallyInitProfile = classmethod(_reallyInitProfile)
            //initProfile = classmethod(initProfile)
        }

        public string getName()
        {
            return name;
        }
        public int getNumCalls(string name)
        {
            return logger[name];
        }

        public string[] getRegisteredMethods()
        {
            return logger.Keys.ToArray();
        }

        public String[] getRegisteredAssociations()
        {
            return associations.Values.ToArray();
        }

        public Dictionary<string, int> getLogger()
        {
            return logger;
        }

        public Dictionary<string, string> getAssociations()
        {
            return associations;
        }

        public void setSecondName(String name)
        {
            this.secondName = name;
        }

		public void increaseTotalCalls()
		{ 
			increaseTotalCalls (1);
		}

        public void increaseTotalCalls(int numCalls)
        {
            totalCalls += numCalls;

            if (totalCalls >= LIMIT)
            {
                string filename = string.Format("data/agent_{0}_log.txt", name);
                writeToFile(filename);
            }
        }

        public void reset()
        {
            logger = new Dictionary<string, int>();
            associations = new Dictionary<string, string>();
            name = "";
        }

        public void setInfo(string info)
        {
            this.info = info;
        }

        public String[] getallMethods(Type type)
        {
            MethodInfo[] info = type.GetMethods();
            string[] names = new string[info.Length];
            int count = 0;

            foreach (MethodInfo elem in info)
                names[count++] = elem.Name;

            return names;
        }

        public void register(Type klass, String[] names)
        {
            if (names.Length == 0)
                names = getallMethods(klass);

            foreach (string elem in names)
            {
                this.logger[elem] = 0;
                this.associations[elem] = klass.Name;
                //  TODO: Complicated because here methods are attached to a class which is hard in Csharp
                // setattr(klass,name,self.wrapper(name,getattr(klass,name)))

            }
        }
        //def wrapper(self,name,fun):
        //    def new_fun(*args, **kwargs):
        //        self._logger[name]+=1
        //        return fun(*args, **kwargs)
        //    return new_fun   

        public string getFormattedOutput(Dictionary<string, float> dict)
        {
            Dictionary<string,object> dictionary = new Dictionary<string,object>();

            foreach (KeyValuePair<string, float> pair in dict)
                dictionary[pair.Key] = pair.Value;
            return getFormattedOutput(dictionary);
        }

        public string getFormattedOutput(Dictionary<string, int> dict)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (KeyValuePair<string, int> pair in dict)
                dictionary[pair.Key] = pair.Value;
            return getFormattedOutput(dictionary);
        }


        public string getFormattedOutput(Dictionary<string, object> dict)
        {
            string str = "";
            
            List<string> keys = dict.Keys.ToList();
            keys.Sort();

            foreach (string key in keys)
            {
                str += string.Format(">>\t{0}\t{1}\n", key, dict[key]);
            }

            return str;
        }

        public void writeToFile(string outputFile)
        {
            StreamWriter log;

            if (File.Exists(outputFile))
            {
                log = new StreamWriter(File.OpenWrite(outputFile));
                log.Write(this.logger.Keys);
                // if hasattr(self,'_info'):
                //    print >> log,self._info         
                // else:
                //    print >> log,'no information supplied'  
            }
            else
                log = new StreamWriter(File.Create(outputFile));

            log.WriteLine(totalCalls);
            log.WriteLine(getFormattedOutput(logger));
            log.Close();

        }

        public void loadFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            string[] columns;

            if (!File.Exists(fileName))
                throw new FileNotFoundException(string.Format("Profiler could not find {0}",fileName));

            foreach(string line in lines)
            {
                columns = line.Split();
                if (columns.Length == 3 && columns[0] == ">>")
                    if (avgs.ContainsKey(columns[1]))
                    {
                        avgs[columns[1]] += float.Parse(columns[2]);
                        counts[columns[1]] += 1;
                    }
                    else
                    {
                        avgs[columns[1]] = float.Parse(columns[2]);
                        counts[columns[1]] = 1;
                    }
            }
        }

        public void computeAverage(string fileName)
        {
            loadFile(fileName);

            Dictionary<string,float>.KeyCollection items = avgs.Keys;

            Console.Out.WriteLine(items.ToArray());

            foreach (string item in items)
                avgs[item] /= counts[item];
            
            Console.Out.WriteLine(avgs.ToArray());

            StreamWriter log=new StreamWriter(File.OpenWrite(fileName));
            log.WriteLine("AVERAGES");
            log.WriteLine(getFormattedOutput(avgs));
            log.Close();

        }

        public void computeMultiAgentAverage(string fileName, AgentBase[] agents)
        {
            // TODO: Profiler needs to be completed
            throw new NotImplementedException();
        }
        /**
    
               
       
            def compute_multi_agent_avg(self,target_file,agents):    
                avgs={}
                counts={}
        
                for agent in agents:
                    file_name=self.directory+'/agent_%s_log.txt' % (agent)
                    file=open(file_name,'r')
        
                    for line in file.readlines():                        
                        columns=line.split()
                
                        if len(columns)==3 and columns[0]=='>>':
                            if avgs.has_key(columns[1]):                
                                avgs[columns[1]]+=float(columns[2])
                                counts[columns[1]]+=1
                            else:
                                avgs[columns[1]]=float(columns[2])
                                counts[columns[1]]=1            
            
                items=avgs.keys()

                for item in items:
                    avgs[item]/=counts[item]
                
                log=open(target_file,'a')
                print >> log,'AVERAGES'
                print >> log,self.get_formatted_output(avgs)         
                log.close()
   
            def compute_depletion_avg(self,target_file,agents):    
                avgs={}
                counts={}
        
                for agent in agents:
                    file_name=self.directory+'/agent_%s_log.txt' % (agent)
                    file=open(file_name,'r')
        
                    for line in file.readlines():                        
                        columns=line.split()
                
                        if len(columns)==3 and columns[0]=='>>' and columns[1]=='depleted':
                            if avgs.has_key(columns[1]):                
                                avgs[columns[1]]+=float(columns[2])
                                counts[columns[1]]+=1
                            else:
                                avgs[columns[1]]=float(columns[2])
                                counts[columns[1]]=1            
            
                items=avgs.keys()

                for item in items:
                    avgs[item]/=counts[item]
                
                log=open(target_file,'a')
                print >> log,self.get_formatted_output(avgs)         
                log.close()
      
        if __name__=='__main__':     
            p=Profiler(None)
            p.compute_multi_agent_avg(p.directory+'final_result.txt',['A00','A01','A02','A03','A04'])
            p.compute_depletion_avg(p.directory+'final_result.txt',['A05','A06','A07','A08'])
         * */


    }
}
