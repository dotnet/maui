#nullable enable
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace mauitemplate
{
    [Instrumentation(Name = "com.microsoft.mauitemplate.Launch")]
    public class TemplateLaunchInstrumentation : Instrumentation
    {
        protected TemplateLaunchInstrumentation()
        { }

        protected TemplateLaunchInstrumentation(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        { }

        public override void OnCreate(Bundle? arguments)
        {
            base.OnCreate(arguments);
            Start();
        }

        public override void OnStart()
        {
            base.OnStart();

            Bundle results = new Bundle();
            var activityName = "com.microsoft.mauitemplate.MainActivity";
            var monitor = AddMonitor(activityName, null, false);
            Intent intent = new Intent(Intent.ActionMain);
            intent.SetFlags(ActivityFlags.NewTask);
            intent.SetClassName(TargetContext!, activityName);
            StartActivitySync(intent);
            var currentActivity = WaitForMonitor(monitor);
            var resultCode = currentActivity is not null ? Result.Ok : Result.Canceled;

			results.PutString("return-code", resultCode.ToString("D"));		
			
            Finish(resultCode, results);
        }
    }
}
