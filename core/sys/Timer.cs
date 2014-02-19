using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace POSH_sharp.sys.strict
{
    /**
     * Implementation of the agent timer.

     *  The agent timer is responsible for checking drive element call
        frequencies, and for adjusting the loop timing. The time is usually
        initialised at 0 and then each call to C{time} returns the time in
        milliseconds that has passed since initialisation.

     *  Currently, this module provides a stepped timer and a real-time
        timer. The step timer increases its time by 1 every time that
        loopEnd() is called, but does not provide frequency checking and
        neither cares about the loop timing, as it is assumed that the timing
        is controlled from the outside. The real-time timer uses the pc time
        and provides both frequency checking and loop frequency control.
    */

    ///<summary>
    ///An agent timer base class. 
    ///This class defines the interface of an agent timer class.
    ///</summary>
    public abstract class TimerBase
    {
        ///<summary>
        ///This method resets the timer.
        ///</summary>
        public TimerBase()
        {
            Reset();
        }

        ///<summary>
        ///Returns the current timestamp in milliseconds.
        ///</summary>
        public static long CurrentTimeStamp(){
            return System.DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        ///<summary>
        ///Resetting the timer sets its internal starting time to 0. 
        ///All calls to L{time} after calling this method return the time 
        ///that has passed since the this method has been classed.
        ///</summary>
        public virtual void Reset(){
            throw new NotImplementedException();
        }
        ///<summary>
        ///Returns the current time in milliseconds.
        ///<returns>
        ///The current time in milliseconds using the long Data type
        ///</returns>
        ///</summary>
        public virtual long Time(){
            throw new NotImplementedException();
        }
        ///<summary>
        ///To be called at the end of each loop.
        ///
        /// For a stepped timer, this method increases the time. For a 
        /// real-time timer, this method does nothing.
        ///</summary>
        public virtual void LoopEnd(){
            throw new NotImplementedException();
        }
        ///<summary>
        ///Manages the loop frequency.
        ///
        /// This method is supposed to be called at the end of each loop
        /// to adjust the loop frequency. It waits a certain time and then
        /// returns, to make the loop run at a certain frequency. Hence,
        /// it holds statistics about the time inbetween two calls of this
        /// methods and adjusts the wait time to achieve the required time
        /// difference.
        ///</summary>
        public virtual void LoopWait(){
            throw new NotImplementedException();
        }
        ///<summary>
        ///Sets the new loop frequency and resets the timer.
        ///This method should only affect real-time timers.
        ///<value name="loopFreq">
        ///The loop frequence, given by the time in milliseconds that one loop should take.
        ///</value>
        ///</summary>
        public virtual void SetLoopFreq(long loopFreq)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A stepped agent timer.
    /// 
    /// This timer is a stepped timer, which is to be used if the agent is
    /// stepped, i.e. controlled from and outside controller. The timer
    /// starts at time 0 and increases the time every time that L{loopEnd} is
    /// called. It does not provide loop timing, as that wouldn't make any
    /// sense if the agent is controlled from the outside.
    /// </summary>
    public class SteppedTimer : TimerBase
    {
        private long _time;

        public SteppedTimer()
            :base()
        {
        }

        public override void Reset()
        {
            _time = 0;
        }

        /// <summary>
        /// Returns the current state of the internal timer.
        /// </summary>
        /// <returns>current time</returns>
        public override long Time()
        {
            return _time;
        }

        /// <summary>
        /// Increases the internal timer by 1.
        /// </summary>
        public override void LoopEnd()
        {
            _time +=1;
        }

        /// <summary>
        /// Does nothing, as the stepped timer does not provide loop control.
        /// </summary>
        public override void LoopWait()
        {
            
        }

        /// <summary>
        /// Does nothing, as the stepped timer does not provide loop control.
        /// </summary>
        /// <param name="loopFreq"> is ignored
        /// </param>
        public override void SetLoopFreq(long loopFreq)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An agent real-time timer.
    /// 
    /// The real-time timer relies on the system clock for its timing. On
    /// initialising and resetting the timer, its internal clock is set to
    /// 0, and any call to L{time} returns the time that passed since the
    /// timer was resetted last. The timer provides loop frequency
    /// control.
    /// </summary>
    public class RealTimeTimer : TimerBase
    {
        private long _base;
        private long _lastReturn;
        private Queue<long> _procTime;
        private long _freq;

        ///<summary>
        ///Resets the timer and sets the loop frequency.
        ///<param name="loopFreq">
        ///The wanted loop frequency, given by the time in
        ///milliseconds that one loop should take.
        ///</param>
        /// The loop frequency is the one used by L{loopWait}.
        ///</summary>
        public RealTimeTimer(long loopFreq)
            :base()
        {

        _base = 0;
        _lastReturn = 0;
        _procTime = new Queue<long>();
        _freq = loopFreq;
        }

        ///<summary>
        ///Resets the timer.
        ///
        /// All future calls to L{time} return the time that passed since this
        /// method was called last.
        ///</summary>
        public override void Reset(){
        _lastReturn = 0;
        _procTime = new Queue<long>();
        _base = CurrentTimeStamp();
        }

        ///<summary>
        ///Returns the time passed since the last call of L{reset}.
        ///<returns>
        ///Time passed in milliseconds.
        ///</returns>
        ///</summary>
        public override long Time(){
            return CurrentTimeStamp() - _base;
        }
        ///<summary>
        ///To be called at the end of each loop.
        ///
        /// Does nothing, as the timing is provided by the system clock.
        ///</summary>
        public override void LoopEnd(){
        }
        ///<summary>
        ///Waits some time to adjust the loop frequency.
        ///
        ///The loop frequency is the one given on initialising the timer, or
        ///by calling L{setLoopFreq}. The method adjusts its own waiting time
        ///based on passed statistics about when it was called, to reach that
        ///it is called in a certain interval, based on the assumption that the
        ///process that calls this method always takes the same time inbetween
        ///calling this method.

        ///The waiting time is estimated based on the average of the estimate
        ///of the last 5 process times. The process time is estimated by the
        ///time inbetween the last return of this method and the time when it
        ///is called the next time.

        ///To make sure that the process time estimate is accurate, the
        ///timer has to be resetted (by calling L{reset}) just before the
        ///loop is started (for the first time, or after a pause).
        ///</summary>
        public override void LoopWait(){
            
            long ts = Time();
            Queue<long> pc = _procTime;
            long avgTime;
            long waitTime;

            // compute process time estimate.
            if (pc.Count >= 5){
                pc.Reverse();
                pc.Dequeue();
                pc.Reverse();
            }
            pc.Enqueue(ts - _lastReturn);
            // get the average process time estimate and the time we
            // therefore need to wait
            
            long sumProcTime = 0;
            foreach (long elem in pc)
                sumProcTime += elem;
            avgTime = sumProcTime / pc.Count;
            waitTime = _freq -avgTime;
            // the time we're going to return is the time this method was
            // called + the waiting time (given that is > 0)
            if (waitTime > 0){
                ts = Time();
                Thread.Sleep((int)(waitTime / 1000.0));
                _lastReturn = ts + waitTime;
            }else{
                _lastReturn = ts;
            }
        }
        ///<summary>
        ///Sets the new loop frequency and resets the timer.
        ///<value name="loopFreq">
        ///The loop frequence, given by the time in milliseconds that one loop should take.
        ///</value>
        ///</summary>
        public override void SetLoopFreq(long loopFreq)
        {
            _freq= loopFreq;
            Reset();
        }
    }
}
