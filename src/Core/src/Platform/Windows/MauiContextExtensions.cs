using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Windows.ApplicationModel.Resources.Core;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			string resourceFlowDirection = ResourceManager.Current.DefaultContext.QualifierValues["LayoutDirection"];
			if (resourceFlowDirection == "LTR")
				return FlowDirection.LeftToRight;
			else if (resourceFlowDirection == "RTL")
				return FlowDirection.RightToLeft;

			return FlowDirection.MatchParent;
		}

		public static WindowManager GetWindowManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<WindowManager>();

		public static UI.Xaml.Window GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UI.Xaml.Window>();

		public static UI.Xaml.Window? GetOptionalNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<UI.Xaml.Window>();

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UI.Xaml.Application nativeApplication)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(nativeApplication);
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IDispatcherProvider>().GetDispatcher(nativeApplication));

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UI.Xaml.Window nativeWindow)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(nativeWindow);
			scopedContext.AddSpecific(new WindowManager(scopedContext));
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IAnimationManager>());
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IDispatcherProvider>().GetDispatcher(nativeWindow));

			return scopedContext;
		}
	}
}
