using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.StyleSheets;

namespace Xamarin.Forms
{
	[Flags]
	public enum InitializationFlags : long
	{
		DisableCss = 1 << 0
	}


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
		readonly Dictionary<Type, Dictionary<Type, (Type target, short priority)>> _handlers = new Dictionary<Type, Dictionary<Type, (Type target, short priority)>>();
		static Type _defaultVisualType = typeof(VisualMarker.DefaultVisual);
		static Type _materialVisualType = typeof(VisualMarker.MaterialVisual);

		static Type[] _defaultVisualRenderers = new[] { _defaultVisualType };

		public void Register(Type tview, Type trender, Type[] supportedVisuals, short priority)
		{
			supportedVisuals = supportedVisuals ?? _defaultVisualRenderers;
			//avoid caching null renderers
			if (trender == null)
				return;

			if (!_handlers.TryGetValue(tview, out Dictionary<Type, (Type target, short priority)> visualRenderers))
			{
				visualRenderers = new Dictionary<Type, (Type target, short priority)>();
				_handlers[tview] = visualRenderers;
			}

			for (int i = 0; i < supportedVisuals.Length; i++)
			{
				if(visualRenderers.TryGetValue(supportedVisuals[i], out (Type target, short priority) existingTargetValue))
				{
					if(existingTargetValue.priority <= priority)
						visualRenderers[supportedVisuals[i]] = (trender, priority);
				}
				else
					visualRenderers[supportedVisuals[i]] = (trender, priority);
			}
		}

		public void Register(Type tview, Type trender, Type[] supportedVisual) => Register(tview, trender, supportedVisual, 0);

		public void Register(Type tview, Type trender) => Register(tview, trender, _defaultVisualRenderers);

		internal TRegistrable GetHandler(Type type) => GetHandler(type, _defaultVisualType);

		internal TRegistrable GetHandler(Type type, Type visualType)
		{
			Type handlerType = GetHandlerType(type, visualType ?? _defaultVisualType);
			if (handlerType == null)
				return null;

			object handler = DependencyResolver.ResolveOrCreate(handlerType);

			return (TRegistrable)handler;
		}

		internal TRegistrable GetHandler(Type type, object source, IVisual visual, params object[] args)
		{
			if (args.Length == 0)
			{
				return GetHandler(type, visual?.GetType() ?? _defaultVisualType);
			}

			Type handlerType = GetHandlerType(type, visual?.GetType() ?? _defaultVisualType);
			if (handlerType == null)
				return null;

			
			return (TRegistrable)DependencyResolver.ResolveOrCreate(handlerType, source, visual?.GetType(), args);
		}

		public TOut GetHandler<TOut>(Type type) where TOut : class, TRegistrable
		{
			return GetHandler(type) as TOut;
		}

		public TOut GetHandler<TOut>(Type type, params object[] args) where TOut : class, TRegistrable
		{
			return GetHandler(type, null, null, args) as TOut;
		}

		public TOut GetHandlerForObject<TOut>(object obj) where TOut : class, TRegistrable
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();

			return GetHandler(type, (obj as IVisualController)?.EffectiveVisual?.GetType()) as TOut;
		}

		public TOut GetHandlerForObject<TOut>(object obj, params object[] args) where TOut : class, TRegistrable
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var reflectableType = obj as IReflectableType;
			var type = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : obj.GetType();

			return GetHandler(type, obj, (obj as IVisualController)?.EffectiveVisual, args) as TOut;
		}
		

		public Type GetHandlerType(Type viewType) => GetHandlerType(viewType, _defaultVisualType);

		public Type GetHandlerType(Type viewType, Type visualType)
		{
			visualType = visualType ?? _defaultVisualType;

			// 1. Do we have this specific type registered already?
			if (_handlers.TryGetValue(viewType, out Dictionary<Type, (Type target, short priority)> visualRenderers))
				if (visualRenderers.TryGetValue(visualType, out (Type target, short priority) specificTypeRenderer))
					return specificTypeRenderer.target;
				else if (visualType == _materialVisualType)
					VisualMarker.MaterialCheck();

			if (visualType != _defaultVisualType && visualRenderers != null)
				if (visualRenderers.TryGetValue(_defaultVisualType, out (Type target, short priority) specificTypeRenderer))
					return specificTypeRenderer.target;

			// 2. Do we have a RenderWith for this type or its base types? Register them now.
			RegisterRenderWithTypes(viewType, visualType);

			// 3. Do we have a custom renderer for a base type or did we just register an appropriate renderer from RenderWith?
			if (LookupHandlerType(viewType, visualType, out (Type target, short priority) baseTypeRenderer))
				return baseTypeRenderer.target;
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

		bool LookupHandlerType(Type viewType, Type visualType, out (Type target, short priority) handlerType)
		{
			visualType = visualType ?? _defaultVisualType;
			while (viewType != null && viewType != typeof(Element))
			{
				if (_handlers.TryGetValue(viewType, out Dictionary<Type, (Type target, short priority)> visualRenderers))
					if (visualRenderers.TryGetValue(visualType, out handlerType))
						return true;

				if (visualType != _defaultVisualType && visualRenderers != null)
					if (visualRenderers.TryGetValue(_defaultVisualType, out handlerType))
						return true;

				viewType = viewType.GetTypeInfo().BaseType;
			}

			handlerType = (null, 0);
			return false;
		}

		void RegisterRenderWithTypes(Type viewType, Type visualType)
		{
			visualType = visualType ?? _defaultVisualType;

			// We're going to go through each type in this viewType's inheritance chain to look for classes
			// decorated with a RenderWithAttribute. We're going to register each specific type with its
			// renderer.
			while (viewType != null && viewType != typeof(Element))
			{
				// Only go through this process if we have not registered something for this type;
				// we don't want RenderWith renderers to override ExportRenderers that are already registered.
				// Plus, there's no need to do this again if we already have a renderer registered.
				if (!_handlers.TryGetValue(viewType, out Dictionary<Type, (Type target, short priority)> visualRenderers) || 
					!(visualRenderers.ContainsKey(visualType) ||
					  visualRenderers.ContainsKey(_defaultVisualType)))
				{
					// get RenderWith attribute for just this type, do not inherit attributes from base types
					var attribute = viewType.GetTypeInfo().GetCustomAttributes<RenderWithAttribute>(false).FirstOrDefault();
					if (attribute == null)
					{
						// TODO this doesn't appear to do anything. Register just returns as a NOOP if the renderer is null
						Register(viewType, null, new[] { visualType }); // Cache this result so we don't have to do GetCustomAttributes again
					}
					else
					{
						Type specificTypeRenderer = attribute.Type;

						if (specificTypeRenderer.Name.StartsWith("_", StringComparison.Ordinal))
						{
							// TODO: Remove attribute2 once renderer names have been unified across all platforms
							var attribute2 = specificTypeRenderer.GetTypeInfo().GetCustomAttribute<RenderWithAttribute>();
							if (attribute2 != null)
							{
								for (int i = 0; i < attribute2.SupportedVisuals.Length; i++)
								{
									if (attribute2.SupportedVisuals[i] == _defaultVisualType)
										specificTypeRenderer = attribute2.Type;

									if (attribute2.SupportedVisuals[i] == visualType)
									{
										specificTypeRenderer = attribute2.Type;
										break;
									}
								}
							}

							if (specificTypeRenderer.Name.StartsWith("_", StringComparison.Ordinal))
							{
								Register(viewType, null, new[] { visualType }); // Cache this result so we don't work through this chain again

								viewType = viewType.GetTypeInfo().BaseType;
								continue;
							}
						}

						Register(viewType, specificTypeRenderer, new[] { visualType }); // Register this so we don't have to look for the RenderWithAttibute again in the future
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
		internal static Dictionary<string, IList<StyleSheets.StylePropertyAttribute>> StyleProperties { get; } = new Dictionary<string, IList<StyleSheets.StylePropertyAttribute>>();

		public static IEnumerable<Assembly> ExtraAssemblies { get; set; }

		public static Registrar<IRegisterable> Registered { get; internal set; }

		//typeof(ExportRendererAttribute);
		//typeof(ExportCellAttribute);
		//typeof(ExportImageSourceHandlerAttribute);
		public static void RegisterRenderers(HandlerAttribute[] attributes)
		{
			var length = attributes.Length;
			for (var i = 0; i < length; i++)
			{
				var attribute = attributes[i];
				if (attribute.ShouldRegister())
					Registered.Register(attribute.HandlerType, attribute.TargetType, attribute.SupportedVisuals, attribute.Priority);
			}
		}

		public static void RegisterStylesheets()
		{
			var assembly = typeof(StylePropertyAttribute).GetTypeInfo().Assembly;

#if NETSTANDARD2_0
			object[] styleAttributes = assembly.GetCustomAttributes(typeof(StylePropertyAttribute), true);
#else
			object[] styleAttributes = assembly.GetCustomAttributes(typeof(StyleSheets.StylePropertyAttribute)).ToArray();
#endif
			var stylePropertiesLength = styleAttributes.Length;
			for (var i = 0; i < stylePropertiesLength; i++)
			{
				var attribute = (StylePropertyAttribute)styleAttributes[i];
				if (StyleProperties.TryGetValue(attribute.CssPropertyName, out var attrList))
					attrList.Add(attribute);
				else
					StyleProperties[attribute.CssPropertyName] = new List<StylePropertyAttribute> { attribute };
			}
		}

		public static void RegisterEffects(string resolutionName, ExportEffectAttribute[] effectAttributes)
		{
			var exportEffectsLength = effectAttributes.Length;
			for (var i = 0; i < exportEffectsLength; i++)
			{
				var effect = effectAttributes[i];
				Effects[resolutionName + "." + effect.Id] = effect.Type;
			}
		}

		public static void RegisterAll(Type[] attrTypes)
		{
			RegisterAll(attrTypes, default(InitializationFlags));
		}
		public static void RegisterAll(Type[] attrTypes, InitializationFlags flags)
		{
			Profile.FrameBegin();

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
			Profile.FramePartition("Reflect");
			foreach (Assembly assembly in assemblies)
			{
				var assemblyName = assembly.GetName().Name;
				Profile.FrameBegin(assemblyName);

				foreach (Type attrType in attrTypes)
				{
					object[] attributes = assembly.GetCustomAttributesSafe(attrType);
					if (attributes == null || attributes.Length == 0)
						continue;
					
					var length = attributes.Length;
					for (var i = 0; i < length; i++)
					{
						var a = attributes[i];
						var attribute = a as HandlerAttribute;
						if(attribute == null && (a is ExportFontAttribute fa))
						{
							FontRegistrar.Register(fa, assembly);
						}
						else
						{
							if (attribute.ShouldRegister())
								Registered.Register(attribute.HandlerType, attribute.TargetType, attribute.SupportedVisuals, attribute.Priority);
						}
					}
				}

				object[] effectAttributes = assembly.GetCustomAttributesSafe(typeof (ExportEffectAttribute));
				if (effectAttributes == null || effectAttributes.Length == 0)
				{
					Profile.FrameEnd(assemblyName);
					continue;
				}

				string resolutionName = assembly.FullName;
				var resolutionNameAttribute = (ResolutionGroupNameAttribute)assembly.GetCustomAttribute(typeof(ResolutionGroupNameAttribute));
				if (resolutionNameAttribute != null)
					resolutionName = resolutionNameAttribute.ShortName;
				RegisterEffects(resolutionName, (ExportEffectAttribute[])effectAttributes);

				Profile.FrameEnd(assemblyName);
			}

			if ((flags & InitializationFlags.DisableCss) == 0)
				RegisterStylesheets();

			Profile.FramePartition("DependencyService.Initialize");
			DependencyService.Initialize(assemblies);

			Profile.FrameEnd();
		}
	}
}
