using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		static readonly Dictionary<Type, Type> DefaultMauiControlHandlers = new Dictionary<Type, Type>
		{
			{ typeof(NavigationPage), typeof(NavigationPageHandler) },
#if WINDOWS
			{ typeof(Shell), typeof(ShellHandler) },
#endif
			{ typeof(ActivityIndicator), typeof(ActivityIndicatorHandler) },
			{ typeof(Button), typeof(ButtonHandler) },
			{ typeof(CheckBox), typeof(CheckBoxHandler) },
			{ typeof(DatePicker), typeof(DatePickerHandler) },
			{ typeof(Editor), typeof(EditorHandler) },
			{ typeof(Entry), typeof(EntryHandler) },
			{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
			{ typeof(Image), typeof(ImageHandler) },
			{ typeof(Label), typeof(LabelHandler) },
			{ typeof(Layout2.Layout), typeof(LayoutHandler) },
			{ typeof(Picker), typeof(PickerHandler) },
			{ typeof(ProgressBar), typeof(ProgressBarHandler) },
			{ typeof(SearchBar), typeof(SearchBarHandler) },
			{ typeof(Slider), typeof(SliderHandler) },
			{ typeof(Stepper), typeof(StepperHandler) },
			{ typeof(Switch), typeof(SwitchHandler) },
			{ typeof(TimePicker), typeof(TimePickerHandler) },
			{ typeof(Page), typeof(PageHandler) },
			{ typeof(Shapes.Ellipse), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Line), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Path), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polygon), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polyline), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Rectangle), typeof(ShapeViewHandler) },
			{ typeof(Layout), typeof(LayoutHandler) },
		};

		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
			=> handlersCollection.AddHandlers(DefaultMauiControlHandlers);
	}
}
