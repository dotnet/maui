#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseFormsCompatibility(this IAppHostBuilder builder, bool registerRenderers = true)
		{
			// TODO: this hideousness is just until the dynamic handler registration is merged
			FormsCompatBuilder? compatBuilder = null;
			IMauiHandlersCollection? handlersCollection = null;
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlersCollection = handlers;
				compatBuilder?.SetHandlersCollection(handlersCollection);
			});

			builder.ConfigureServices<FormsCompatBuilder>(compat =>
			{
				// TODO: this hideousness is just until the dynamic handler registration is merged
				compatBuilder = compat;
				if (handlersCollection != null)
					compatBuilder.SetHandlersCollection(handlersCollection);

				compat.UseCompatibilityRenderers = registerRenderers;
			});

			return builder;
		}
	}

	class FormsCompatBuilder : IMauiServiceBuilder
	{
		// TODO: This won't really be a thing once we have all the handlers built
		static readonly List<Type> ControlsWithHandlers = new()
		{
			typeof(Button),
			typeof(ContentPage),
			typeof(Page),
			typeof(Label),
			typeof(CheckBox),
			typeof(Entry),
			typeof(Image),
			typeof(Switch),
			typeof(Editor),
			typeof(ActivityIndicator),
			typeof(DatePicker),
			typeof(Picker),
			typeof(ProgressBar),
			typeof(SearchBar),
			typeof(Slider),
			typeof(Stepper),
			typeof(TimePicker),
			typeof(Shell),
		};

		static readonly List<(Type Control, Type Renderer)> PendingRenderers = new();

		IMauiHandlersCollection? _handlers;

		public bool UseCompatibilityRenderers { get; set; }

		public void SetHandlersCollection(IMauiHandlersCollection handlersCollection) =>
			_handlers = handlersCollection;

		public static void AddRenderer(Type control, Type renderer) =>
			PendingRenderers.Add((control, renderer));

		public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
		}

		public void Configure(HostBuilderContext context, IServiceProvider services)
		{
			CompatServiceProvider.SetServiceProvider(services);

#if __ANDROID__
			var options = new InitializationOptions(global::Android.App.Application.Context, null, null);
#elif __IOS__
			var options = new InitializationOptions();
#elif WINDOWS
			var options = new InitializationOptions(MauiWinUIApplication.Current.LaunchActivatedEventArgs);
#endif

#if (__ANDROID__ || __IOS__ || WINDOWS)
			options.Flags |= InitializationFlags.SkipRenderers;

			Forms.Init(options);

			if (UseCompatibilityRenderers)
			{
				Forms.RegisterCompatRenderers(
					new[] { typeof(RendererToHandlerShim).Assembly },
					typeof(RendererToHandlerShim).Assembly,
					(controlType) =>
					{
						foreach (var type in ControlsWithHandlers)
						{
							if (type.IsAssignableFrom(controlType))
								return;
						}

						_handlers?.AddHandler(controlType, typeof(RendererToHandlerShim));
					});
			}
#endif

			// register renderer with old registrar so it can get shimmed
			foreach (var (control, renderer) in PendingRenderers)
			{
				Internals.Registrar.Registered.Register(control, renderer);
			}
		}
	}
}
