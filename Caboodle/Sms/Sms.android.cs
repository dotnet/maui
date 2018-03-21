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

            var intent = CreateIntent(message);
            Platform.CurrentContext.StartActivity(intent);

            return Task.FromResult(true);
        }

        static Intent CreateIntent(SmsMessage message)
            => CreateIntent(message?.Recipient, message?.Body);

        static Intent CreateIntent(string recipient, string body = null)
        {
            Intent intent;
            if (!string.IsNullOrWhiteSpace(recipient))
            {
                var uri = AndroidUri.Parse("smsto:" + recipient);
                intent = new Intent(Intent.ActionSendto, uri);

                if (!string.IsNullOrWhiteSpace(body))
                    intent.PutExtra("sms_body", body);
            }
            else
            {
                var pm = Platform.CurrentContext.PackageManager;
                var packageName = Telephony.Sms.GetDefaultSmsPackage(Platform.CurrentContext);
                intent = pm.GetLaunchIntentForPackage(packageName);
            }

            return intent;
        }

        public static string GetDefaultSmsPackage(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                return Telephony.Sms.GetDefaultSmsPackage(context);
            }
            else
            {
                var defApp = Settings.Secure.GetString(context.ContentResolver, "sms_default_application");
                var pm = context.ApplicationContext.PackageManager;
                var intent = pm.GetLaunchIntentForPackage(defApp);
                var mInfo = pm.ResolveActivity(intent, 0);
                return mInfo.ActivityInfo.PackageName;
            }
        }
    }
}
