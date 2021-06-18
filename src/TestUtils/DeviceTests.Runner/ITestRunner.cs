using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runners
{
    interface ITestRunner
    {
        Task<IReadOnlyList<TestAssemblyViewModel>> Discover();
        Task Run(TestCaseViewModel test);
        Task Run(IEnumerable<TestCaseViewModel> tests, string message = null);
        Task Run(IReadOnlyList<AssemblyRunInfo> runInfos, string message = null);
        event Action<string> OnDiagnosticMessage;
    }
}