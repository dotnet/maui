using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    public interface IResultChannel : ITestListener
    {
        Task CloseChannel();
        Task<bool> OpenChannel(string message = null);
    }
}