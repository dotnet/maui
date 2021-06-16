using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection TryAddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			Internals.Registrar.Registered.Register(controlType, rendererType);

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
			handlersCollection.TryAddHandler(controlType, typeof(RendererToHandlerShim));
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			Internals.Registrar.Registered.Register(controlType, rendererType);

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
			handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim));
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TMauiType : IFrameworkElement
		{
			Internals.Registrar.Registered.Register(typeof(TControlType), typeof(TRenderer));

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

		public static IMauiHandlersCollection AddCompatibilityRenderers(this IMauiHandlersCollection handlersCollection, params global::System.Reflection.Assembly[] assemblies)
		{

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST

			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[] 
				{
					typeof(ExportRendererAttribute),
					typeof(ExportCellAttribute),
				}, default(InitializationFlags),
				(controlType) =>
				{
					handlersCollection?.TryAddHandler(controlType, typeof(RendererToHandlerShim));
				});


			DependencyService.ScanAssemblies(assemblies);
#endif


			return handlersCollection;
		}

		public static IFontCollection AddFonts(this IFontCollection fontCollection, params global::System.Reflection.Assembly[] assemblies)
		{
			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[]
				{
					typeof(ExportFontAttribute)
				}, default(InitializationFlags),
				null);
			return fontCollection;
		}

		public static IImageSourceServiceCollection AddServices(this IImageSourceServiceCollection services, params global::System.Reflection.Assembly[] assemblies)
		{
			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[]
				{
					typeof(ExportImageSourceHandlerAttribute)
				}, default(InitializationFlags),
				null);

			return services;
		}
	}
}