using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Webkit;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        static readonly EmailMessage testEmail =
            new EmailMessage("Testing Xamarin.Essentials", "This is a test email.", "Xamarin.Essentials@example.org");

        internal static bool IsComposeSupported
            => Platform.IsIntentSupported(CreateIntent(testEmail));

        static Task PlatformComposeAsync(EmailMessage message)
        {
            var intent = CreateIntent(message)
                .SetFlags(ActivityFlags.ClearTop)
                .SetFlags(ActivityFlags.NewTask);

            Platform.AppContext.StartActivity(intent);

            return Task.FromResult(true);
        }

        static Intent CreateIntent(EmailMessage message)
        {
            var action = message?.Attachments?.Count > 1 ? Intent.ActionSendMultiple : Intent.ActionSend;
            var intent = new Intent(action);
            intent.SetType("message/rfc822");

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
