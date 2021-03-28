namespace Microsoft.Maui.LifecycleEvents
{
	public static class IosLifecycleBuilderExtensions
	{
		public static IIosLifecycleBuilder FinishedLaunching(this IIosLifecycleBuilder lifecycle, IosLifecycle.FinishedLaunching del) => lifecycle.OnEvent(del);
		public static IIosLifecycleBuilder OnActivated(this IIosLifecycleBuilder lifecycle, IosLifecycle.OnActivated del) => lifecycle.OnEvent(del);
		public static IIosLifecycleBuilder OnResignActivation(this IIosLifecycleBuilder lifecycle, IosLifecycle.OnResignActivation del) => lifecycle.OnEvent(del);
		public static IIosLifecycleBuilder WillTerminate(this IIosLifecycleBuilder lifecycle, IosLifecycle.WillTerminate del) => lifecycle.OnEvent(del);
		public static IIosLifecycleBuilder DidEnterBackground(this IIosLifecycleBuilder lifecycle, IosLifecycle.DidEnterBackground del) => lifecycle.OnEvent(del);
		public static IIosLifecycleBuilder WillEnterForeground(this IIosLifecycleBuilder lifecycle, IosLifecycle.WillEnterForeground del) => lifecycle.OnEvent(del);
	}
}