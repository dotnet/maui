using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Provider;

using AndroidUri = Android.Net.Uri;

namespace Microsoft.Caboodle
{
    public static partial class Sms
    {
        static bool IsComposeSupported
            => Platform.IsIntentSupported(CreateIntent("0000000000"));

        public static Task ComposeAsync(SmsMessage message)
        {
            if (!IsComposeSupported)
                throw new FeatureNotSupportedException();

            var intent = CreateIntent(message)
                .SetFlags(ActivityFlags.ClearTop)
                .SetFlags(ActivityFlags.NewTask);

            Platform.CurrentContext.StartActivity(intent);

            return Task.FromResult(true);
        }

        static Intent CreateIntent(SmsMessage message)
            => CreateIntent(message?.Recipient, message?.Body);

        static Intent CreateIntent(string recipient, string body = null)
        {
            Intent intent = null;

            body = body ?? string.Empty;
            recipient = recipient ?? string.Empty;

            if (string.IsNullOrWhiteSpace(recipient) && Platform.HasApiLevel(BuildVersionCodes.Kitkat))
            {
                var packageName = Telephony.Sms.GetDefaultSmsPackage(Platform.CurrentContext);
                if (!string.IsNullOrWhiteSpace(packageName))
                {
                    intent = new Intent(Intent.ActionSend);
                    intent.SetType("text/plain");
                    intent.PutExtra(Intent.ExtraText, body);
                    intent.SetPackage(packageName);

                    return intent;
                }
            }

            // Fall back to normal send
            intent = new Intent(Intent.ActionView);

            if (!string.IsNullOrWhiteSpace(body))
                intent.PutExtra("sms_body", body);

            intent.PutExtra("address", recipient);

            var uri = AndroidUri.Parse("smsto:" + AndroidUri.Encode(recipient));
            intent.SetData(uri);

            return intent;
        }
    }
}
