using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class SmsImplementation:ISms
	{
		internal static bool IsComposeSupported
			=> ApiInformation.IsTypePresent("Windows.ApplicationModel.Chat.ChatMessageManager");

		public Task ComposeAsync(SmsMessage message)
		{
			var chat = new ChatMessage();
			if (!string.IsNullOrWhiteSpace(message?.Body))
				chat.Body = message.Body;

			foreach (var recipient in message?.Recipients)
				chat.Recipients.Add(recipient);

			return ChatMessageManager.ShowComposeSmsMessageAsync(chat).AsTask();
		}
	}
}
