using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
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

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Compose_With_Message_Shows_New_Window_BlankCC()
		{
			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Hello World!",
					Body = "This is a greeting email.",
					To = { "everyone@example.org" },
					Cc = { string.Empty },
					Bcc = { string.Empty },
				};

				return Email.ComposeAsync(email);
			});
		}

		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public Task Email_Attachments_are_Sent()
		{
			// Save a local cache data directory file
			var file = Path.Combine(FileSystem.AppDataDirectory, "EmailTest.txt");
			File.WriteAllText(file, "Attachment contents goes here...");

			return Utils.OnMainThread(() =>
			{
				var email = new EmailMessage
				{
					Subject = "Hello World!",
					Body =
						"This is a greeting email." + Environment.NewLine +
						"There should be an attachment attached.",
					To = { "everyone@example.org" },
					Attachments = { new EmailAttachment(file) }
				};

				return Email.ComposeAsync(email);
			});
		}
	}
}
