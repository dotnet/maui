using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Microsoft.Maui.Storage;
using Uri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		static EmailMessage testEmail =>
			new("Testing Microsoft.Maui.Essentials", "This is a test email.", "Microsoft.Maui.Essentials@example.org");

		public bool IsComposeSupported
			=> PlatformUtils.IsIntentSupported(CreateIntent(testEmail));

		Task PlatformComposeAsync(EmailMessage message)
		{
			var intent = CreateIntent(message);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			intent.SetFlags(flags);

			Application.Context.StartActivity(intent);

			return Task.FromResult(true);
		}

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
				intent.SetType(FileMimeTypes.EmailMessage);

			if (!string.IsNullOrEmpty(message?.Body))
			{
				if (message.BodyFormat == EmailBodyFormat.Html)
				{
					ISpanned html;
#if __ANDROID_24__
					if (OperatingSystem.IsAndroidVersionAtLeast(24))
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
					uris.Add(FileSystemUtils.GetShareableFileUri(attachment));
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
