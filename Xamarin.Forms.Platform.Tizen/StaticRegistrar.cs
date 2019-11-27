using System;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Platform.Tizen
{
	public class StaticRegistrar<TRegistrable> where TRegistrable : class
	{
		readonly Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();

		public void Register(Type tview, Type trender)
		{
			if (trender == null)
				return;

			_handlers[tview] = trender;
		}

		public TOut GetHandler<TOut>(Type type, params object[] args) where TOut : class, TRegistrable
		{
			if (LookupHandlerType(type, out Type handlerType))
			{
				object handler = Activator.CreateInstance(handlerType, args);
				return (TRegistrable)handler as TOut;
			}
			Log.Error("No handler could be found for that type :" + type);
			return null;

		}

		public bool LookupHandlerType(Type viewType, out Type handlerType)
		{
			while (viewType != null && viewType != typeof(Element))
			{
				if (_handlers.TryGetValue(viewType, out handlerType))
					return true;

				viewType = viewType.GetTypeInfo().BaseType;
			}
			handlerType = null;
			return false;
		}

		public TOut GetHandlerForObject<TOut>(object obj) where TOut : class, TRegistrable
		{
			return GetHandlerForObject<TOut>(obj, null);
		}

		public TOut GetHandlerForObject<TOut>(object obj, params object[] args) where TOut : class, TRegistrable
		{
			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();
			return GetHandler<TOut>(type, args);
		}
	}

	public static class StaticRegistrar
	{
		public static StaticRegistrar<IRegisterable> Registered { get; internal set; }

		static StaticRegistrar()
		{
			Registered = new StaticRegistrar<IRegisterable>();
		}

		public static void RegisterHandlers(Dictionary<Type, Type> customHandlers)
		{
			//Renderers
			Registered.Register(typeof(Layout), typeof(LayoutRenderer));
			Registered.Register(typeof(ScrollView), typeof(ScrollViewRenderer));
			Registered.Register(typeof(CarouselPage), typeof(CarouselPageRenderer));
			Registered.Register(typeof(Page), typeof(PageRenderer));
			Registered.Register(typeof(NavigationPage), typeof(NavigationPageRenderer));
			Registered.Register(typeof(MasterDetailPage), typeof(MasterDetailPageRenderer));
			Registered.Register(typeof(TabbedPage), typeof(TabbedPageRenderer));
			Registered.Register(typeof(Shell), typeof(ShellRenderer));
			Registered.Register(typeof(Label), typeof(LabelRenderer));
			Registered.Register(typeof(Button), typeof(ButtonRenderer));
			Registered.Register(typeof(Image), typeof(ImageRenderer));
			Registered.Register(typeof(Slider), typeof(SliderRenderer));
			Registered.Register(typeof(Picker), typeof(PickerRenderer));
			Registered.Register(typeof(Frame), typeof(FrameRenderer));
			Registered.Register(typeof(Stepper), typeof(StepperRenderer));
			Registered.Register(typeof(DatePicker), typeof(DatePickerRenderer));
			Registered.Register(typeof(TimePicker), typeof(TimePickerRenderer));
			Registered.Register(typeof(ProgressBar), typeof(ProgressBarRenderer));
			Registered.Register(typeof(Switch), typeof(SwitchRenderer));
			Registered.Register(typeof(CheckBox), typeof(CheckBoxRenderer));
			Registered.Register(typeof(ListView), typeof(ListViewRenderer));
			Registered.Register(typeof(BoxView), typeof(BoxViewRenderer));
			Registered.Register(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer));
			Registered.Register(typeof(SearchBar), typeof(SearchBarRenderer));
			Registered.Register(typeof(Entry), typeof(EntryRenderer));
			Registered.Register(typeof(Editor), typeof(EditorRenderer));
			Registered.Register(typeof(TableView), typeof(TableViewRenderer));
			Registered.Register(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer));
			Registered.Register(typeof(WebView), typeof(WebViewRenderer));
			Registered.Register(typeof(ImageButton), typeof(ImageButtonRenderer));
			Registered.Register(typeof(StructuredItemsView), typeof(StructuredItemsViewRenderer));
			Registered.Register(typeof(CarouselView), typeof(CarouselViewRenderer));
			Registered.Register(typeof(SwipeView), typeof(SwipeViewRenderer));

			//ImageSourceHandlers
			Registered.Register(typeof(FileImageSource), typeof(FileImageSourceHandler));
			Registered.Register(typeof(StreamImageSource), typeof(StreamImageSourceHandler));
			Registered.Register(typeof(UriImageSource), typeof(UriImageSourceHandler));

			//Cell Renderers
			Registered.Register(typeof(TextCell), typeof(TextCellRenderer));
			Registered.Register(typeof(ImageCell), typeof(ImageCellRenderer));
			Registered.Register(typeof(SwitchCell), typeof(SwitchCellRenderer));
			Registered.Register(typeof(EntryCell), typeof(EntryCellRenderer));
			Registered.Register(typeof(ViewCell), typeof(ViewCellRenderer));

			//Gesture Handlers
			Registered.Register(typeof(TapGestureRecognizer), typeof(TapGestureHandler));
			Registered.Register(typeof(PinchGestureRecognizer), typeof(PinchGestureHandler));
			Registered.Register(typeof(PanGestureRecognizer), typeof(PanGestureHandler));
			Registered.Register(typeof(SwipeGestureRecognizer), typeof(SwipeGestureHandler));

			//Dependencies
			DependencyService.Register<ISystemResourcesProvider, ResourcesProvider>();
			DependencyService.Register<IDeserializer, Deserializer>();
			DependencyService.Register<INativeBindingService, NativeBindingService>();
			DependencyService.Register<INativeValueConverterService, NativeValueConverterService>();

			//Custom Handlers
			if (customHandlers != null)
			{
				foreach (KeyValuePair<Type, Type> handler in customHandlers)
				{
					Registered.Register(handler.Key, handler.Value);
				}
			}
		}
	}
}