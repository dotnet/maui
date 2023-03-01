using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace Tests
{
	public class EmailDataGenerator : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			// empty
			new object[]
			{
				new EmailMessage(),
				"mailto:?"
			},

			// body only
			new object[]
			{
				new EmailMessage { Body = "Hello" },
				"mailto:?body=Hello"
			},

			// subject only
			new object[]
			{
				new EmailMessage { Subject = "Hello" },
				"mailto:?subject=Hello"
			},

			// subject and body
			new object[]
			{
				new EmailMessage { Subject = "Hello", Body = "Yo" },
				"mailto:?subject=Hello&body=Yo"
			},

			// to only
			new object[]
			{
				new EmailMessage { To = new List<string> { "john@doe.net" } },
				"mailto:?to=john%40doe.net"
			},

			// cc only
			new object[]
			{
				new EmailMessage { Cc = new List<string> { "john@doe.net" } },
				"mailto:?cc=john%40doe.net"
			},

			// bcc only
			new object[]
			{
				new EmailMessage { Bcc = new List<string> { "john@doe.net" } },
				"mailto:?bcc=john%40doe.net"
			},

			// To 1 recipient
			new object[]
			{
				new EmailMessage
				{
					To = new List<string> { "sauron@mordor.gov.middleearth" },
					Subject = "Claim your free rings of power",
					Body = "Click this link to get your rings..."
				},
				"mailto:?to=sauron%40mordor.gov.middleearth&subject=Claim%20your%20free%20rings%20of%20power&body=Click%20this%20link%20to%20get%20your%20rings..."
			},

			// To 2 recipients
			new object[]
			{
				new EmailMessage
				{
					To = new List<string> { "bilbo@hobbiton.shire", "frodo@hobbiton.shire" },
					Subject = "Greetings",
					Body = "Greetings Hobbits!"
				},
				"mailto:?to=bilbo%40hobbiton.shire,frodo%40hobbiton.shire&subject=Greetings&body=Greetings%20Hobbits%21"
			},

			// Cc 1 recipient
			new object[]
			{
				new EmailMessage
				{
					Cc = new List<string> { "surfer@maui.net" },
					Subject = "Big waves",
					Body = "Dude, there were huge waves yesterday"
				},
				"mailto:?cc=surfer%40maui.net&subject=Big%20waves&body=Dude%2C%20there%20were%20huge%20waves%20yesterday"
			},

			// Cc 2 recipients
			new object[]
			{
				new EmailMessage
				{
					Cc = new List<string> { "surfer@maui.net", "dude@surf.net" },
					Subject = "Duuuude",
					Body = "Sweet"
				},
				"mailto:?cc=surfer%40maui.net,dude%40surf.net&subject=Duuuude&body=Sweet"
			},

			// Bcc 1 recipient
			new object[]
			{
				new EmailMessage
				{
					Bcc = new List<string> { "knights@who.say.ni" },
					Subject = "Shrubberies here!",
					Body = "Ekke Ekke Ekke Ekke Ptang Zoo Boing!"
				},
				"mailto:?bcc=knights%40who.say.ni&subject=Shrubberies%20here%21&body=Ekke%20Ekke%20Ekke%20Ekke%20Ptang%20Zoo%20Boing%21"
			},

			// Bcc 2 recipients
			new object[]
			{
				new EmailMessage
				{
					Bcc = new List<string> { "knights@who.say.ni", "arthur@who.says.nu"
					},
					Subject = "Shrubberies here!",
					Body = "Ekke Ekke Ekke Ekke Ptang Zoo Boing!"
				},
				"mailto:?bcc=knights%40who.say.ni,arthur%40who.says.nu&subject=Shrubberies%20here%21&body=Ekke%20Ekke%20Ekke%20Ekke%20Ptang%20Zoo%20Boing%21"
			},

			// Mixed recipients
			new object[]
			{
				new EmailMessage
				{
					To = new List<string> { "bilbo@hobbiton.shire", "frodo@hobbiton.shire" },
					Cc = new List<string> { "knights@who.say.ni", "arthur@who.says.nu" },
					Subject = "Greetings", Body = "Greetings Hobbits!"
				},
				"mailto:?to=bilbo%40hobbiton.shire,frodo%40hobbiton.shire&cc=knights%40who.say.ni,arthur%40who.says.nu&subject=Greetings&body=Greetings%20Hobbits%21"
			},
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class Email_Tests
	{
		[Theory]
		[ClassData(typeof(EmailDataGenerator))]
		public void GetMailToUri_Returns_RFC2368_Valid_Url(EmailMessage message, string expectedUrl)
		{
			var result = EmailImplementation.GetMailToUri(message);

			Assert.Equal(expectedUrl, result);
		}

		[Fact]
		public void GetMailToUri_Ignores_Attachments()
		{
			var message = new EmailMessage
			{
				To = new List<string> { "mom@maui.net" },
				Attachments = new List<EmailAttachment>
				{
					new EmailAttachment("/my/lovely/path/selfie.jpeg")
				}
			};

			var result = EmailImplementation.GetMailToUri(message);

			Assert.DoesNotContain("selfie", result, StringComparison.InvariantCultureIgnoreCase);
		}

		[Fact]
		public void GetMailToUri_Ingores_BodyFormat()
		{
			var message = new EmailMessage
			{
				To = new List<string> { "mom@maui.net" },
				BodyFormat = EmailBodyFormat.Html,
				Body = "Hi Mom!"
			};

			var result = EmailImplementation.GetMailToUri(message);

			Assert.Contains("Hi%20Mom%21", result, StringComparison.InvariantCulture);
		}
	}
}
