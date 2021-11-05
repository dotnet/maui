using Microsoft.Extensions.DependencyInjection;
using ElmSharp;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		
		public static Window GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<Window>();

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, Window nativeWindow)
		{
			var scopedContext = new MauiContext(mauiContext);
			scopedContext.AddSpecific(nativeWindow);
			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, CoreUIAppContext context)
		{
			return new MauiContext(mauiContext.Services, context, mauiContext);
		}
	}
}
