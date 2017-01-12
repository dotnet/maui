using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	internal class Registrar<TRegistrable> where TRegistrable : class
	{
		readonly Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();

		public void Register(Type tview, Type trender)
		{
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

		internal TOut GetHandler<TOut>(Type type) where TOut : TRegistrable
		{
			return (TOut)GetHandler(type);
		}

		internal Type GetHandlerType(Type viewType)
		{
			Type type;
			if(LookupHandlerType(viewType, out type))
				return type;

			// lazy load render-view association with RenderWithAttribute (as opposed to using ExportRenderer)
			var attribute = viewType.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
			if (attribute == null)
			{
				Register(viewType, null); // Cache this result so we don't have to do GetCustomAttribute again
				return null;
			}

			type = attribute.Type;

			if (type.Name.StartsWith("_"))
			{
				// TODO: Remove attribute2 once renderer names have been unified across all platforms
				var attribute2 = type.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
				if (attribute2 != null)
					type = attribute2.Type;

				if (type.Name.StartsWith("_"))
				{
					Register(viewType, null); // Cache this result so we don't work through this chain again
					return null;
				}
			}

			Register(viewType, type); // Register this so we don't have to look for the RenderWith Attibute again in the future

			return type;
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

	internal static class Registrar
	{
		static Registrar()
		{
			Registered = new Registrar<IRegisterable>();
		}

		internal static Dictionary<string, Type> Effects { get; } = new Dictionary<string, Type>();

		internal static IEnumerable<Assembly> ExtraAssemblies { get; set; }

		internal static Registrar<IRegisterable> Registered { get; }

		internal static void RegisterAll(Type[] attrTypes)
		{
			Assembly[] assemblies = Device.GetAssemblies();
			if (ExtraAssemblies != null)
			{
				assemblies = assemblies.Union(ExtraAssemblies).ToArray();
			}

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
					if (attributes.Length == 0)
						continue;

					foreach (HandlerAttribute attribute in attributes)
					{
						if (attribute.ShouldRegister())
							Registered.Register(attribute.HandlerType, attribute.TargetType);
					}
				}

				string resolutionName = assembly.FullName;

				Attribute[] effectAttributes = assembly.GetCustomAttributes(typeof(ExportEffectAttribute)).ToArray();
				if (effectAttributes.Length > 0)
				{
					var resolutionNameAttribute = (ResolutionGroupNameAttribute)assembly.GetCustomAttribute(typeof(ResolutionGroupNameAttribute));
					if (resolutionNameAttribute != null)
					{
						resolutionName = resolutionNameAttribute.ShortName;
					}

					foreach (Attribute attribute in effectAttributes)
					{
						var effect = (ExportEffectAttribute)attribute;
						Effects [resolutionName + "." + effect.Id] = effect.Type;
					}
				}
			}

			DependencyService.Initialize(assemblies);
		}
	}
}