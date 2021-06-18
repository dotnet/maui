using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Runners
{
    public enum TestState
    {
        All = 0,
        Passed,
        Failed,
        Skipped,
        NotRun
    }

    public enum RunStatus
    {
        Ok,
        Failed,
        NoTests,
        Skipped,
        NotRun
    }
}