using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
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

		public static NavigationRootManager GetNavigationRootManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<NavigationRootManager>();

		public static UI.Xaml.Window GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UI.Xaml.Window>();

		public static UI.Xaml.Window? GetOptionalNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<UI.Xaml.Window>();

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UI.Xaml.Application nativeApplication)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(nativeApplication);

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UI.Xaml.Window nativeWindow, out IServiceScope scope)
		{
			scope = mauiContext.Services.CreateScope();

			var scopedContext = new MauiContext(scope.ServiceProvider, mauiContext);

			scopedContext.AddSpecific(nativeWindow);
			scopedContext.AddSpecific(new NavigationRootManager(scopedContext));
			scopedContext.AddSpecific(new WindowManager(scopedContext));
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IAnimationManager>());

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}
	}
}