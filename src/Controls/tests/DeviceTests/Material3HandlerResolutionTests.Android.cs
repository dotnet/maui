using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Mapper)]
	public class Material3HandlerResolutionTests
	{
		const string IsMaterial3EnabledSwitch = "Microsoft.Maui.RuntimeFeature.IsMaterial3Enabled";

		public static TheoryData<Type, string, string> Material3HandlerTypes => new()
		{
			{ typeof(ActivityIndicator), "Microsoft.Maui.Handlers.ActivityIndicatorHandler", "Microsoft.Maui.Handlers.ActivityIndicatorHandler2" },
			{ typeof(DatePicker), "Microsoft.Maui.Handlers.DatePickerHandler", "Microsoft.Maui.Handlers.DatePickerHandler2" },
			{ typeof(Editor), "Microsoft.Maui.Handlers.EditorHandler", "Microsoft.Maui.Handlers.EditorHandler2" },
			{ typeof(Entry), "Microsoft.Maui.Handlers.EntryHandler", "Microsoft.Maui.Handlers.EntryHandler2" },
			{ typeof(Image), "Microsoft.Maui.Handlers.ImageHandler", "Microsoft.Maui.Handlers.ImageHandler2" },
			{ typeof(Label), "Microsoft.Maui.Handlers.LabelHandler", "Microsoft.Maui.Handlers.LabelHandler2" },
			{ typeof(Picker), "Microsoft.Maui.Handlers.PickerHandler", "Microsoft.Maui.Handlers.PickerHandler2" },
			{ typeof(ProgressBar), "Microsoft.Maui.Handlers.ProgressBarHandler", "Microsoft.Maui.Handlers.ProgressBarHandler2" },
			{ typeof(RadioButton), "Microsoft.Maui.Handlers.RadioButtonHandler", "Microsoft.Maui.Handlers.RadioButtonHandler2" },
			{ typeof(SearchBar), "Microsoft.Maui.Handlers.SearchBarHandler", "Microsoft.Maui.Handlers.SearchBarHandler2" },
			{ typeof(Slider), "Microsoft.Maui.Handlers.SliderHandler", "Microsoft.Maui.Handlers.SliderHandler2" },
			{ typeof(Switch), "Microsoft.Maui.Handlers.SwitchHandler", "Microsoft.Maui.Handlers.SwitchHandler2" },
			{ typeof(TimePicker), "Microsoft.Maui.Handlers.TimePickerHandler", "Microsoft.Maui.Handlers.TimePickerHandler2" },
		};

		[Theory]
		[MemberData(nameof(Material3HandlerTypes))]
		public void ResolvesMaterial3HandlerWhenFeatureSwitchEnabled(Type viewType, string defaultHandlerTypeName, string material3HandlerTypeName)
		{
			AppContext.TryGetSwitch(IsMaterial3EnabledSwitch, out var originalValue);

			try
			{
				AppContext.SetSwitch(IsMaterial3EnabledSwitch, false);
				Assert.Equal(defaultHandlerTypeName, ResolveHandlerType(viewType).FullName);

				AppContext.SetSwitch(IsMaterial3EnabledSwitch, true);
				Assert.Equal(material3HandlerTypeName, ResolveHandlerType(viewType).FullName);
			}
			finally
			{
				AppContext.SetSwitch(IsMaterial3EnabledSwitch, originalValue);
			}
		}

		static Type ResolveHandlerType(Type viewType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<Application>()
				.Build();

			return mauiApp.Services.GetRequiredService<IMauiHandlersFactory>().GetHandlerType(viewType)!;
		}
	}
}
