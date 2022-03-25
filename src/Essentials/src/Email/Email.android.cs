using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Webkit;
using Microsoft.Maui.Storage;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public partial class EmailImplementation : IEmail
	{
		static EmailMessage testEmail =>
			new ("Testing Microsoft.Maui.Essentials", "This is a test email.", "Microsoft.Maui.Essentials@example.org");

		public bool IsComposeSupported
			=> Platform.IsIntentSupported(CreateIntent(testEmail));

		public Task ComposeAsync(EmailMessage message)
		{
			var intent = CreateIntent(message);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
			if (Platform.HasApiLevelN)
				flags |= ActivityFlags.LaunchAdjacent;
#endif
			intent.SetFlags(flags);

			Platform.AppContext.StartActivity(intent);

			return Task.FromResult(true);
		}

		public Task ComposeAsync(string subject, string body, params string[] to)
			=> ComposeAsync(
				new EmailMessage()
				{
					Subject = subject,
					Body = body,
					To = to.ToList()
				});

		public Task ComposeAsync()
			=> ComposeAsync(null);

		static Intent CreateIntent(EmailMessage message)
		{
			var action = (message?.Attachments?.Count ?? 0) switch
			{
				0 => Intent.ActionSendto,
				1 => Intent.ActionSend,
				_ => Intent.ActionSendMultiple
			};
			var intent = new Intent(action);

			if (action == Intent.ActionSendto)
				intent.SetData(Uri.Parse("mailto:"));
			else
				intent.SetType(FileSystem.MimeTypes.EmailMessage);

			if (!string.IsNullOrEmpty(message?.Body))
			{
				if (message.BodyFormat == EmailBodyFormat.Html)
				{
					ISpanned html;
#if __ANDROID_24__
					if (Platform.HasApiLevelN)
					{
						html = Html.FromHtml(message.Body, FromHtmlOptions.ModeLegacy);
					}
					else
#endif
					{
#pragma warning disable CS0618 // Type or member is obsolete
						html = Html.FromHtml(message.Body);
#pragma warning restore CS0618 // Type or member is obsolete
					}
					intent.PutExtra(Intent.ExtraText, html);
				}
				else
				{
					intent.PutExtra(Intent.ExtraText, message.Body);
				}
			}
			if (!string.IsNullOrEmpty(message?.Subject))
				intent.PutExtra(Intent.ExtraSubject, message.Subject);
			if (message?.To?.Count > 0)
				intent.PutExtra(Intent.ExtraEmail, message.To.ToArray());
			if (message?.Cc?.Count > 0)
				intent.PutExtra(Intent.ExtraCc, message.Cc.ToArray());
			if (message?.Bcc?.Count > 0)
				intent.PutExtra(Intent.ExtraBcc, message.Bcc.ToArray());

			if (message?.Attachments?.Count > 0)
			{
				var uris = new List<IParcelable>();
				foreach (var attachment in message.Attachments)
				{
					uris.Add(Platform.GetShareableFileUri(attachment));
				}

				if (uris.Count > 1)
					intent.PutParcelableArrayListExtra(Intent.ExtraStream, uris);
				else
					intent.PutExtra(Intent.ExtraStream, uris[0]);

				intent.AddFlags(ActivityFlags.GrantReadUriPermission);
			}

			return intent;
		}
	}
}
