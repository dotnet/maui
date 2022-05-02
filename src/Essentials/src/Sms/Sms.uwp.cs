using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported
			=> ApiInformation.IsTypePresent("Windows.ApplicationModel.Chat.ChatMessageManager");

		Task PlatformComposeAsync(SmsMessage message)
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
