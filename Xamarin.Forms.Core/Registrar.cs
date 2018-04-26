using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	// Previewer uses reflection to bind to this method; Removal or modification of visibility will break previewer.
	internal static class Registrar
	{
		internal static void RegisterAll(Type[] attrTypes) => Internals.Registrar.RegisterAll(attrTypes);
	}
}

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Registrar<TRegistrable> where TRegistrable : class
	{
		readonly Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();

		public void Register(Type tview, Type trender)
		{
			//avoid caching null renderers
			if (trender == null)
				return;
			_handlers[tview] = trender;
		}

		internal TRegistrable GetHandler(Type type)
		{
			Type handlerType = GetHandlerType(type);
			if (handlerType == null)
				return null;

			object handler = DependencyResolver.ResolveOrCreate(handlerType);

			return (TRegistrable)handler;
		}

		internal TRegistrable GetHandler(Type type, params object[] args)
		{
			if (args.Length == 0)
			{
				return GetHandler(type);
			}

			Type handlerType = GetHandlerType(type);
			if (handlerType == null)
				return null;

			return (TRegistrable)DependencyResolver.ResolveOrCreate(handlerType, args);
		}

		public TOut GetHandler<TOut>(Type type) where TOut : TRegistrable
		{
			return (TOut)GetHandler(type);
		}

		public TOut GetHandler<TOut>(Type type, params object[] args) where TOut : TRegistrable
		{
			return (TOut)GetHandler(type, args);
		}

		public TOut GetHandlerForObject<TOut>(object obj) where TOut : TRegistrable
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();

			return (TOut)GetHandler(type);
		}

		public TOut GetHandlerForObject<TOut>(object obj, params object[] args) where TOut : TRegistrable
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();

			return (TOut)GetHandler(type, args);
		}

		public Type GetHandlerType(Type viewType)
		{
			// 1. Do we have this specific type registered already?
			if (_handlers.TryGetValue(viewType, out Type specificTypeRenderer))
				return specificTypeRenderer;

			// 2. Do we have a RenderWith for this type or its base types? Register them now.
			RegisterRenderWithTypes(viewType);

			// 3. Do we have a custom renderer for a base type or did we just register an appropriate renderer from RenderWith?
			if (LookupHandlerType(viewType, out Type baseTypeRenderer))
				return baseTypeRenderer;
			else
				return null;
		}

		public Type GetHandlerTypeForObject(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();

			return GetHandlerType(type);
		}

		bool LookupHandlerType(Type viewType, out Type handlerType)
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

		void RegisterRenderWithTypes(Type viewType)
		{
			// We're going to go through each type in this viewType's inheritance chain to look for classes
			// decorated with a RenderWithAttribute. We're going to register each specific type with its
			// renderer. 

			while (viewType != null && viewType != typeof(Element))
			{
				// Only go through this process if we have not registered something for this type;
				// we don't want RenderWith renderers to override ExportRenderers that are already registered.
				// Plus, there's no need to do this again if we already have a renderer registered.
				if (!_handlers.ContainsKey(viewType))
				{
					// get RenderWith attribute for just this type, do not inherit attributes from base types
					var attribute = viewType.GetTypeInfo().GetCustomAttributes<RenderWithAttribute>(false).FirstOrDefault();
					if (attribute == null)
					{
						Register(viewType, null); // Cache this result so we don't have to do GetCustomAttributes again
					}
					else
					{
						Type specificTypeRenderer = attribute.Type;

						if (specificTypeRenderer.Name.StartsWith("_", StringComparison.Ordinal))
						{
							// TODO: Remove attribute2 once renderer names have been unified across all platforms
							var attribute2 = specificTypeRenderer.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
							if (attribute2 != null)
								specificTypeRenderer = attribute2.Type;

							if (specificTypeRenderer.Name.StartsWith("_", StringComparison.Ordinal))
							{
								Register(viewType, null); // Cache this result so we don't work through this chain again

								viewType = viewType.GetTypeInfo().BaseType;
								continue;
							}
						}

						Register(viewType, specificTypeRenderer); // Register this so we don't have to look for the RenderWithAttibute again in the future
					}
				}

				viewType = viewType.GetTypeInfo().BaseType;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class Registrar
	{
		static Registrar()
		{
			Registered = new Registrar<IRegisterable>();
		}

		internal static Dictionary<string, Type> Effects { get; } = new Dictionary<string, Type>();
		internal static Dictionary<string, StyleSheets.StylePropertyAttribute> StyleProperties { get; } = new Dictionary<string, StyleSheets.StylePropertyAttribute>();

		public static IEnumerable<Assembly> ExtraAssemblies { get; set; }

		public static Registrar<IRegisterable> Registered { get; internal set; }

		public static void RegisterAll(Type[] attrTypes)
		{
			Assembly[] assemblies = Device.GetAssemblies();
			if (ExtraAssemblies != null)
				assemblies = assemblies.Union(ExtraAssemblies).ToArray();

			Assembly defaultRendererAssembly = Device.PlatformServices.GetType().GetTypeInfo().Assembly;
			int indexOfExecuting = Array.IndexOf(assemblies, defaultRendererAssembly);

			if (indexOfExecuting > 0)
			{
				assemblies[indexOfExecuting] = assemblies[0];
				assemblies[0] = defaultRendererAssembly;
			}

			// Don't use LINQ for performance reasons
			// Naive implementation can easily take over a second to run
			foreach (Assembly assembly in assemblies)
			{
				foreach (Type attrType in attrTypes)
				{
					Attribute[] attributes;
					try
					{
						attributes = assembly.GetCustomAttributes(attrType).ToArray();
					}
					catch (System.IO.FileNotFoundException)
					{
						// Sometimes the previewer doesn't actually have everything required for these loads to work
						Log.Warning(nameof(Registrar), "Could not load assembly: {0} for Attibute {1} | Some renderers may not be loaded", assembly.FullName, attrType.FullName);
						continue;
					}
					var length = attributes.Length;
					for (var i = 0; i < length;i++)
					{
						var attribute = (HandlerAttribute)attributes[i];
						if (attribute.ShouldRegister())
							Registered.Register(attribute.HandlerType, attribute.TargetType);
					}
				}

				string resolutionName = assembly.FullName;
				var resolutionNameAttribute = (ResolutionGroupNameAttribute)assembly.GetCustomAttribute(typeof(ResolutionGroupNameAttribute));
				if (resolutionNameAttribute != null)
					resolutionName = resolutionNameAttribute.ShortName;

				Attribute[] effectAttributes = assembly.GetCustomAttributes(typeof(ExportEffectAttribute)).ToArray();
				var exportEffectsLength = effectAttributes.Length;
				for (var i = 0; i < exportEffectsLength;i++)
				{
					var effect = (ExportEffectAttribute)effectAttributes[i];
					Effects [resolutionName + "." + effect.Id] = effect.Type;
				}

				Attribute[] styleAttributes = assembly.GetCustomAttributes(typeof(StyleSheets.StylePropertyAttribute)).ToArray();
				var stylePropertiesLength = styleAttributes.Length;
				for (var i = 0; i < stylePropertiesLength; i++)
				{
					var attribute = (StyleSheets.StylePropertyAttribute)styleAttributes[i];
					StyleProperties[attribute.CssPropertyName] = attribute;
				}
			}

			DependencyService.Initialize(assemblies);
		}
	}
}
