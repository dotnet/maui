using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Samples.ViewModel
{
	public class EmailViewModel : BaseViewModel
	{
		string subject = "Hello World!";
		string body = "This is the email body.\nWe hope you like it!";
		string recipientsTo = "someone@example.org";
		string recipientsCc;
		string recipientsBcc;
		string attachmentContents;
		string attachmentName;
		bool hasMultipleAttachements;
		bool isHtml;

		public EmailViewModel()
		{
			SendEmailCommand = new Command(OnSendEmail);
		}

		public ICommand SendEmailCommand { get; }

		public string Subject
		{
			get => subject;
			set => SetProperty(ref subject, value);
		}

		public string Body
		{
			get => body;
			set => SetProperty(ref body, value);
		}

		public string RecipientsTo
		{
			get => recipientsTo;
			set => SetProperty(ref recipientsTo, value);
		}

		public string RecipientsCc
		{
			get => recipientsCc;
			set => SetProperty(ref recipientsCc, value);
		}

		public string RecipientsBcc
		{
			get => recipientsBcc;
			set => SetProperty(ref recipientsBcc, value);
		}

		public string AttachmentContents
		{
			get => attachmentContents;
			set => SetProperty(ref attachmentContents, value);
		}

		public string AttachmentName
		{
			get => attachmentName;
			set => SetProperty(ref attachmentName, value);
		}

		public bool HasMultipleAttachements
		{
			get => hasMultipleAttachements;
			set => SetProperty(ref hasMultipleAttachements, value);
		}

		public bool IsHtml
		{
			get => isHtml;
			set => SetProperty(ref isHtml, value);
		}

		async void OnSendEmail()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			try
			{
				var message = new EmailMessage
				{
					Subject = Subject,
					Body = Body,
					BodyFormat = isHtml ? EmailBodyFormat.Html : EmailBodyFormat.PlainText,
					To = Split(RecipientsTo),
					Cc = Split(RecipientsCc),
					Bcc = Split(RecipientsBcc),
				};

				if (!string.IsNullOrWhiteSpace(AttachmentName) || !string.IsNullOrWhiteSpace(AttachmentContents))
				{
					// create a temprary file
					var fn = string.IsNullOrWhiteSpace(AttachmentName) ? "Attachment.txt" : AttachmentName.Trim();
					var file = Path.Combine(FileSystem.CacheDirectory, fn);
					File.WriteAllText(file, AttachmentContents);

					message.Attachments.Add(new EmailAttachment(file));

					if (HasMultipleAttachements)
					{
						fn = "1" + fn;
						file = Path.Combine(FileSystem.CacheDirectory, fn);
						File.WriteAllText(file, AttachmentContents);

						message.Attachments.Add(new EmailAttachment(file));
					}
				}

				await Email.ComposeAsync(message);
			}
			finally
			{
				IsBusy = false;
			}
		}

		List<string> Split(string recipients)
			=> recipients?.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)?.ToList();
	}
}
