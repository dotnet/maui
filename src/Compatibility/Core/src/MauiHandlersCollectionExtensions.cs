using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection TryAddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			// This will eventually get copied to the all the other adds once we get rid of FormsCompatBuilder
			Internals.Registrar.Registered.Register(controlType, rendererType);

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
			handlersCollection.TryAddHandler(controlType, typeof(RendererToHandlerShim));
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			FormsCompatBuilder.AddRenderer(controlType, rendererType);

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
			handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim));
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TMauiType : IFrameworkElement
		{
			FormsCompatBuilder.AddRenderer(typeof(TControlType), typeof(TRenderer));

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
			handlersCollection.AddHandler<TMauiType, RendererToHandlerShim>();
#endif
			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TControlType : IFrameworkElement
		{
			handlersCollection.AddCompatibilityRenderer<TControlType, TControlType, TRenderer>();

			return handlersCollection;
		}
	}
}