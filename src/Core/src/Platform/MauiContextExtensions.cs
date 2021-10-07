using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		public static IAnimationManager GetAnimationManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<IAnimationManager>();

		public static IDispatcher GetDispatcher(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<IDispatcher>();

		public static IDispatcher? GetOptionalDispatcher(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<IDispatcher>();
	}
}