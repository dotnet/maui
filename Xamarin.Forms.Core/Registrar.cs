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

			object handler = Activator.CreateInstance(handlerType);
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

			// This is by no means a general solution to matching with the correct constructor, but it'll
			// do for finding Android renderers which need Context (vs older custom renderers which may still use
			// parameterless constructors)
			if (handlerType.GetTypeInfo().DeclaredConstructors.Any(info => info.GetParameters().Length == args.Length))
			{
				object handler = Activator.CreateInstance(handlerType, args);
				return (TRegistrable)handler;
			}
			
			return GetHandler(type);
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
			Type type;
			if (LookupHandlerType(viewType, out type))
				return type;

			// lazy load render-view association with RenderWithAttribute (as opposed to using ExportRenderer)
			var attribute = viewType.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
			if (attribute == null)
			{
				Register(viewType, null); // Cache this result so we don't have to do GetCustomAttribute again
				return null;
			}

			type = attribute.Type;

			if (type.Name.StartsWith("_", StringComparison.Ordinal))
			{
				// TODO: Remove attribute2 once renderer names have been unified across all platforms
				var attribute2 = type.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
				if (attribute2 != null)
					type = attribute2.Type;

				if (type.Name.StartsWith("_", StringComparison.Ordinal))
				{
					Register(viewType, null); // Cache this result so we don't work through this chain again
					return null;
				}
			}

			Register(viewType, type); // Register this so we don't have to look for the RenderWith Attibute again in the future

			return type;
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
			Type type = viewType;

			while (type != null)
			{
				if (_handlers.ContainsKey(type))
				{
					handlerType = _handlers[type];
					return true;
				}

				type = type.GetTypeInfo().BaseType;
			}

			handlerType = null;
			return false;
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

		public static Registrar<IRegisterable> Registered { get; }

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
					Attribute[] attributes = assembly.GetCustomAttributes(attrType).ToArray();
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
