using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Sms_Tests
    {
        [Fact]
        public Task Sms_ComposeAsync_Does_Not_Throw()
        {
            return Utils.OnMainThread(() =>
            {
                if (Utils.IsiOSSimulator)
                    return Assert.ThrowsAsync<FeatureNotSupportedException>(() => Sms.ComposeAsync());
                else
                    return Sms.ComposeAsync();
            });
        }

        [Fact]
        public Task Sms_ComposeAsync_Does_Not_Throw_When_Empty()
        {
            var message = new SmsMessage();
            return Utils.OnMainThread(() =>
            {
                if (Utils.IsiOSSimulator)
                    return Assert.ThrowsAsync<FeatureNotSupportedException>(() => Sms.ComposeAsync(message));
                else
                    return Sms.ComposeAsync(message);
            });
        }
    }
}
