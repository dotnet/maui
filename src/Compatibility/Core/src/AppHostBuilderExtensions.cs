using System;
using System.Collections.Generic;
using Microsoft.Maui.Hosting;

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
#if !WINDOWS
			typeof(ActivityIndicator),
			typeof(DatePicker),
			typeof(Editor),
			typeof(Entry),
			typeof(Picker),
			typeof(ProgressBar),
			typeof(SearchBar),
			typeof(Slider),
			typeof(Stepper),
			typeof(Switch),
			typeof(TimePicker),
#endif
		};

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