using System;
using System.Collections.Generic;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class AppHostBuilderExtensions
	{
		// This won't really be a thing once we have all the handlers built
		static readonly List<Type> ControlsWithHandlers = new List<Type>
		{
			typeof(Button),
			typeof(ContentPage),
			typeof(Page),
			typeof(Label),
			typeof(CheckBox),
			typeof(Entry),
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
		};

		public static IAppHostBuilder UseWindows(this IAppHostBuilder builder)
		{
			builder.ConfigureLifecycleEvents(events =>
			{
				events.AddEvent<Action<IWindow>>(nameof(IApplication.CreateWindow), window =>
					MessagingCenter.Send(builder, nameof(IApplication.CreateWindow), window));
			});
			return builder;
		}

		public static IAppHostBuilder UseFormsCompatibility(this IAppHostBuilder builder, bool registerRenderers = true)
		{
			// TODO: This should not be immediately run, but rather a registered delegate with values
			//       of the Context and LaunchActivatedEventArgs passed in.

#if __ANDROID__
			var options = new InitializationOptions(global::Android.App.Application.Context, null, null);
#elif __IOS__
			var options = new InitializationOptions();
#elif WINDOWS
			var options = new InitializationOptions(MauiWinUIApplication.Current.LaunchActivatedEventArgs);
#endif

			options.Flags |= InitializationFlags.SkipRenderers;

			builder.UseWindows();

			Forms.Init(options);

			if (registerRenderers)
				builder.UseCompatibilityRenderers();

			return builder;
		}

		public static IAppHostBuilder UseCompatibilityRenderers(this IAppHostBuilder builder)
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

					builder.ConfigureMauiHandlers((_, handlersCollection) => handlersCollection.AddHandler(controlType, typeof(RendererToHandlerShim)));
				});

			return builder;
		}
	}
}