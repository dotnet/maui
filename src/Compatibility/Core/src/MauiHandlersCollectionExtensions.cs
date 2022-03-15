using System;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection TryAddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			Internals.Registrar.Registered.Register(controlType, rendererType);

#if PLATFORM
#pragma warning disable CS0612 // Type or member is obsolete
			handlersCollection.TryAddHandler(controlType, typeof(RendererToHandlerShim));
#pragma warning restore CS0612 // Type or member is obsolete
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer(this IMauiHandlersCollection handlersCollection, Type controlType, Type rendererType)
		{
			Internals.Registrar.Registered.Register(controlType, rendererType);

#if PLATFORM
#pragma warning disable CS0612 // Type or member is obsolete
			handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim));
#pragma warning restore CS0612 // Type or member is obsolete
#endif

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TMauiType : IView
		{
			Internals.Registrar.Registered.Register(typeof(TControlType), typeof(TRenderer));

#if PLATFORM
#pragma warning disable CS0612 // Type or member is obsolete
			handlersCollection.AddHandler<TMauiType, RendererToHandlerShim>();
#pragma warning restore CS0612 // Type or member is obsolete
#endif
			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderer<TControlType, TRenderer>(this IMauiHandlersCollection handlersCollection)
			where TControlType : IView
		{
			handlersCollection.AddCompatibilityRenderer<TControlType, TControlType, TRenderer>();

			return handlersCollection;
		}

		public static IMauiHandlersCollection AddCompatibilityRenderers(this IMauiHandlersCollection handlersCollection, params global::System.Reflection.Assembly[] assemblies)
		{
#if PLATFORM

			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[] 
				{
					typeof(ExportRendererAttribute),
					typeof(ExportCellAttribute),
				}, default(InitializationFlags),
				(result) =>
				{
#pragma warning disable CS0612 // Type or member is obsolete
					handlersCollection?.TryAddHandler(result.target, typeof(RendererToHandlerShim));
#pragma warning restore CS0612 // Type or member is obsolete
				});
#endif


			return handlersCollection;
		}

		public static IFontCollection AddCompatibilityFonts(this IFontCollection fontCollection, IFontRegistrar fontRegistrar, params global::System.Reflection.Assembly[] assemblies)
		{
			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[]
				{
					typeof(ExportFontAttribute)
				}, default(InitializationFlags),
				null,
				fontRegistrar);
			return fontCollection;
		}

		public static IImageSourceServiceCollection AddCompatibilityServices(this IImageSourceServiceCollection services, params global::System.Reflection.Assembly[] assemblies)
		{
#if PLATFORM
			Internals.Registrar.RegisterAll(
				assemblies,
				null,
				new[]
				{
					typeof(ExportImageSourceHandlerAttribute)
				}, default(InitializationFlags),
				(result) =>
				{
					// TODO MAUI: need to fill in registration of a service
					// that can map legacy image handlers to new image service structures
				});
#endif
			return services;
		}



		public static IEffectsBuilder AddCompatibilityEffects(this IEffectsBuilder effectsBuilder, params global::System.Reflection.Assembly[] assemblies)
		{
			Internals.Registrar.RegisterEffects(assemblies);
			return effectsBuilder;
		}
	}
}