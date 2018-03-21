using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Caboodle.Tests
{
    public class Sms_Tests
    {
        [Fact]
        public Task Sms_Fail_On_NetStandard()
        {
            return Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Sms.ComposeAsync());
        }
    }
}
