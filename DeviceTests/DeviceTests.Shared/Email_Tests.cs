using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    // TEST NOTES:
    //   - a human needs to close the email composer window
    public class Email_Tests
    {
        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task Compose_Shows_New_Window()
        {
            if (DeviceInfo.Platform == DeviceInfo.Platforms.UWP)
                return;
            await Email.ComposeAsync();
        }

        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task Compose_With_Message_Shows_New_Window()
        {
            if (DeviceInfo.Platform == DeviceInfo.Platforms.UWP)
                return;

            var email = new EmailMessage
            {
                Subject = "Hello World!",
                Body = "This is a greeting email.",
                To = { "everyone@example.org" }
            };

            await Email.ComposeAsync(email);
        }
    }
}
