using System;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources.Core;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static NavigationRootManager GetNavigationRootManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<NavigationRootManager>();

		public static UI.Xaml.Window GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UI.Xaml.Window>();

		public static UI.Xaml.Window? GetOptionalPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<UI.Xaml.Window>();

		public static IServiceProvider GetApplicationServices(this IMauiContext mauiContext)
		{
			return MauiWinUIApplication.Current.Services
				?? throw new InvalidOperationException("Unable to find Application Services");
		}


		public static IMauiContext MakeScoped(this IMauiContext mauiContext, bool registerNewNavigationRoot)
		{
			var scopedContext = new MauiContext(mauiContext.Services);

			if (registerNewNavigationRoot)
			{
				scopedContext.AddSpecific(new NavigationRootManager(scopedContext.GetPlatformWindow()));
			}

			return scopedContext;
		}
	}
}