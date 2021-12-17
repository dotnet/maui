using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		static readonly Dictionary<Type, Type> DefaultMauiControlHandlers = new Dictionary<Type, Type>
		{
#if __IOS__ || __ANDROID__
			{ typeof(CollectionView), typeof(CollectionViewHandler) },
#endif

#if WINDOWS
			{ typeof(CollectionView), typeof(CollectionViewHandler) },
#endif

#if WINDOWS || __ANDROID__
			{ typeof(Shell), typeof(ShellHandler) },
#endif
			{ typeof(Application), typeof(ApplicationHandler) },
			{ typeof(ActivityIndicator), typeof(ActivityIndicatorHandler) },
			{ typeof(BoxView), typeof(ShapeViewHandler) },
			{ typeof(Button), typeof(ButtonHandler) },
			{ typeof(CheckBox), typeof(CheckBoxHandler) },
			{ typeof(DatePicker), typeof(DatePickerHandler) },
			{ typeof(Editor), typeof(EditorHandler) },
			{ typeof(Entry), typeof(EntryHandler) },
			{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
			{ typeof(Image), typeof(ImageHandler) },
			{ typeof(Label), typeof(LabelHandler) },
			{ typeof(Layout), typeof(LayoutHandler) },
			{ typeof(Picker), typeof(PickerHandler) },
			{ typeof(ProgressBar), typeof(ProgressBarHandler) },
			{ typeof(ScrollView), typeof(ScrollViewHandler) },
			{ typeof(SearchBar), typeof(SearchBarHandler) },
			{ typeof(Slider), typeof(SliderHandler) },
			{ typeof(Stepper), typeof(StepperHandler) },
			{ typeof(Switch), typeof(SwitchHandler) },
			{ typeof(TimePicker), typeof(TimePickerHandler) },
			{ typeof(Page), typeof(PageHandler) },
			{ typeof(WebView), typeof(WebViewHandler) },
			{ typeof(Border), typeof(BorderHandler) },
			{ typeof(IContentView), typeof(ContentViewHandler) },
			{ typeof(Shapes.Ellipse), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Line), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Path), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polygon), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polyline), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Rectangle), typeof(ShapeViewHandler) },
			{ typeof(Shapes.RoundRectangle), typeof(ShapeViewHandler) },
			{ typeof(Window), typeof(WindowHandler) },
			{ typeof(ImageButton), typeof(ImageButtonHandler) },
			{ typeof(IndicatorView), typeof(IndicatorViewHandler) },
#if __ANDROID__ || __IOS__
			{ typeof(RefreshView), typeof(RefreshViewHandler) },
			
#endif
#if WINDOWS || ANDROID
			{ typeof(NavigationPage), typeof(NavigationViewHandler) },
			{ typeof(Toolbar), typeof(ToolbarHandler) },
#endif
#if __ANDROID__
			{ typeof(TabbedPage), typeof(Controls.Handlers.TabbedPageHandler) },
			{ typeof(FlyoutPage), typeof(FlyoutViewHandler) },
#endif
		};

		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
			=> handlersCollection.AddHandlers(DefaultMauiControlHandlers);

		internal static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<FileImageSource>(svcs => new FileImageSourceService(svcs.GetService<IImageSourceServiceConfiguration>(), svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<FontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<StreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<UriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});

			return builder;
		}

		internal static MauiAppBuilder RemapForControls(this MauiAppBuilder builder)
		{
			// Update the mappings for IView/View to work specifically for Controls
			VisualElement.RemapForControls();
			Label.RemapForControls();
			Button.RemapForControls();
			FlyoutPage.RemapForControls();
			Toolbar.RemapForControls();
			Window.RemapForControls();

			return builder;
		}
	}
}
