using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.UI
{
    public class RunnerOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly RunnerOptions Current = new RunnerOptions();
        /// <summary>
        /// 
        /// </summary>
        public RunnerOptions()
        {
            EnableNetwork = false;
            HostName = string.Empty;
            HostPort = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool ShowUseNetworkLogger => (EnableNetwork && !string.IsNullOrWhiteSpace(HostName) && (HostPort > 0));
        /// <summary>
        /// 
        /// </summary>
        public bool AutoStart { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool EnableNetwork { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int HostPort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool TerminateAfterExecution { get; set; }
    }
}