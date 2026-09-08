using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	public class StaticRegistrar<TRegistrable> where TRegistrable : class
	{
		readonly Dictionary<Type, Func<TRegistrable>> _handlers = new Dictionary<Type, Func<TRegistrable>>();

		public void Register(Type tview, Func<TRegistrable> renderer)
		{
			if (renderer == null)
				return;

			_handlers[tview] = renderer;
		}

		public TOut GetHandler<TOut>(Type type, params object[] args) where TOut : class, TRegistrable
		{
			if (LookupHandler(type, out Func<TRegistrable> renderer))
			{
				return (TRegistrable)renderer() as TOut;
			}
			Log.Error("No handler could be found for that type :" + type);
			return null;

		}

		public bool LookupHandler(Type viewType, out Func<TRegistrable> handler)
		{
			while (viewType != null && viewType != typeof(Element))
			{
				if (_handlers.TryGetValue(viewType, out handler))
					return true;

				viewType = viewType.BaseType;
			}
			handler = null;
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

	[Obsolete]
	public static class StaticRegistrar
	{
		public static StaticRegistrar<IRegisterable> Registered { get; internal set; }

		static StaticRegistrar()
		{
			Registered = new StaticRegistrar<IRegisterable>();
		}

		public static void RegisterHandlers(Dictionary<Type, Func<IRegisterable>> customHandlers)
		{
			//Renderers
			Registered.Register(typeof(Layout), () => new LayoutRenderer());
			Registered.Register(typeof(ScrollView), () => new ScrollViewRenderer());
			Registered.Register(typeof(Page), () => new PageRenderer());
			Registered.Register(typeof(NavigationPage), () => new NavigationPageRenderer());
			Registered.Register(typeof(Label), () => new LabelRenderer());
			Registered.Register(typeof(Image), () => new ImageRenderer());

			//ImageSourceHandlers
			Registered.Register(typeof(FileImageSource), () => new FileImageSourceHandler());
			Registered.Register(typeof(StreamImageSource), () => new StreamImageSourceHandler());
			Registered.Register(typeof(UriImageSource), () => new UriImageSourceHandler());

			//Font Loaders
			Registered.Register(typeof(EmbeddedFont), () => new CompatibilityEmbeddedFontLoader());

			//Dependencies
#pragma warning disable CS0612 // Type or member is obsolete
			DependencyService.Register<ISystemResourcesProvider, ResourcesProvider>();
#pragma warning disable CS0612 // Type or member is obsolete
			DependencyService.Register<INativeBindingService, NativeBindingService>();

			//Custom Handlers
			if (customHandlers != null)
			{
				foreach (KeyValuePair<Type, Func<IRegisterable>> handler in customHandlers)
				{
					Registered.Register(handler.Key, handler.Value);
				}
			}
		}
	}
}
