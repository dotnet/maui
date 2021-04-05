using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection AddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Microsoft.Maui.Controls.Internals.Registrar.Registered.Register(
				controlType,
				rendererType);

			handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim));

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TMauiType : IFrameworkElement
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Controls.Internals.Registrar.Registered.Register(
				typeof(TControlType),
				typeof(TRenderer));

			handlersCollection.AddHandler<TMauiType, RendererToHandlerShim>();

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