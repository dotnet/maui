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
        public Task Compose_Shows_New_Window()
        {
            return Utils.OnMainThread(() => Email.ComposeAsync());
        }

        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public Task Compose_With_Message_Shows_New_Window()
        {
            return Utils.OnMainThread(() =>
            {
                var email = new EmailMessage
                {
                    Subject = "Hello World!",
                    Body = "This is a greeting email.",
                    To = { "everyone@example.org" }
                };

                return Email.ComposeAsync(email);
            });
        }
    }
}
