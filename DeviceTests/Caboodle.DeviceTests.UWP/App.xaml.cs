using System.Reflection;
using Xunit.Runners.UI;

namespace Caboodle.DeviceTests.UWP
{
    public sealed partial class App : RunnerApplication
    {
        protected override void OnInitializeRunner()
        {
            AddTestAssembly(typeof(App).GetTypeInfo().Assembly);
        }
    }
}
