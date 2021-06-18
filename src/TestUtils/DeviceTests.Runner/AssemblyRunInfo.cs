using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runners
{
    /// <summary>
    /// 
    /// </summary>
    public class AssemblyRunInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string AssemblyFileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TestAssemblyConfiguration Configuration { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<TestCaseViewModel> TestCases { get; set; }
    }
}