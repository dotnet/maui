using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection AddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			FormsCompatBuilder.AddRenderer(controlType, rendererType);

			handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim));

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TMauiType : IFrameworkElement
		{
			FormsCompatBuilder.AddRenderer(typeof(TControlType), typeof(TRenderer));

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