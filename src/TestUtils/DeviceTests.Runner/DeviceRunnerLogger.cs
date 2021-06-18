using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.Runners
{
    class DeviceRunnerLogger : IRunnerLogger
    {
        public object LockObject { get; } = new object();

        public void LogError(StackFrameInfo stackFrame, string message)
        {
            
        }

        public void LogImportantMessage(StackFrameInfo stackFrame, string message)
        {
            
        }

        public void LogMessage(StackFrameInfo stackFrame, string message)
        {
            
        }

        public void LogWarning(StackFrameInfo stackFrame, string message)
        {
            
        }
    }
}
