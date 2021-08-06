using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void RecordResult(TestResultViewModel result);
    }
}