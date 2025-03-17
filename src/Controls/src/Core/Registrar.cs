#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/InitializationFlags.xml" path="Type[@FullName='Microsoft.Maui.Controls.InitializationFlags']/Docs/*" />
	[Flags]
	public enum InitializationFlags : long
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/InitializationFlags.xml" path="//Member[@MemberName='DisableCss']/Docs/*" />
		DisableCss = 1 << 0,
		SkipRenderers = 1 << 1,
	}
}

namespace Microsoft.Maui.Controls.Internals
{
	internal struct HandlerType
	{
		internal const DynamicallyAccessedMemberTypes TargetMembers = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;

		[DynamicallyAccessedMembers(TargetMembers)]
		public readonly Type Target;
		public readonly short Priority;

		public HandlerType(
			[DynamicallyAccessedMembers(TargetMembers)] Type target,
			short priority)
		{
			Target = target;
			Priority = priority;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Registrar<TRegistrable> where TRegistrable : class
	{
		readonly Dictionary<Type, Dictionary<Type, HandlerType>> _handlers = new Dictionary<Type, Dictionary<Type, HandlerType>>();
		static Type _defaultVisualType = typeof(VisualMarker.DefaultVisual);
		//static Type _materialVisualType = typeof(VisualMarker.MaterialVisual);

		static Type[] _defaultVisualRenderers = new[] { _defaultVisualType };

		public void Register(
			Type tview,
			[DynamicallyAccessedMembers(HandlerType.TargetMembers)] Type trender,
			Type[] supportedVisuals,
			short priority)
		{
			supportedVisuals = supportedVisuals ?? _defaultVisualRenderers;
			//avoid caching null renderers
			if (trender == null)
				return;

			if (!_handlers.TryGetValue(tview, out Dictionary<Type, HandlerType> visualRenderers))
			{
				visualRenderers = new Dictionary<Type, HandlerType>();
				_handlers[tview] = visualRenderers;
			}

			for (int i = 0; i < supportedVisuals.Length; i++)
			{
				if (visualRenderers.TryGetValue(supportedVisuals[i], out HandlerType existingTargetValue))
				{
					if (existingTargetValue.Priority <= priority)
						visualRenderers[supportedVisuals[i]] = new(trender, priority);
				}
				else
					visualRenderers[supportedVisuals[i]] = new(trender, priority);
			}

			// This registers a factory into the Handler version of the registrar.
			// This way if you are running a .NET MAUI app but want to use legacy renderers
			// the Handler.Registrar will use this factory to resolve a RendererToHandlerShim for the given type
			// This only comes into play if users "Init" a legacy set of renderers
			// The default for a .NET MAUI application will be to not do this but 3rd party vendors or
			// customers with large custom renderers will be able to use this to easily shim their renderer to a handler
			// to ease the migration process
			// TODO: This implementation isn't currently compatible with Visual but we have no concept of visual inside
			// .NET MAUI currently.
			// TODO: We need to implemnt this with the new AppHostBuilder
			//Microsoft.Maui.Registrar.Handlers.Register(tview,
			//	(viewType) =>
			//	{
			//		return Registrar.RendererToHandlerShim?.Invoke(null);
			//	});
		}

		public void Register(Type tview, [DynamicallyAccessedMembers(HandlerType.TargetMembers)] Type trender, Type[] supportedVisual)
			=> Register(tview, trender, supportedVisual, 0);

		public void Register(Type tview, [DynamicallyAccessedMembers(HandlerType.TargetMembers)] Type trender)
			=> Register(tview, trender, _defaultVisualRenderers);

		internal TRegistrable GetHandler(Type type) => GetHandler(type, _defaultVisualType);

		internal TRegistrable GetHandler(Type type, Type visualType)
		{
			Type handlerType = GetHandlerType(type, visualType ?? _defaultVisualType);
			if (handlerType == null)
				return null;

			Registrar.CheckIfRendererIsCompatibilityRenderer(handlerType);

			object handler = DependencyResolver.ResolveOrCreate(handlerType);

			return (TRegistrable)handler;
		}

		internal TRegistrable GetHandler(Type type, object source, IVisual visual, params object[] args)
		{
			TRegistrable returnValue = default(TRegistrable);
			if (args.Length == 0)
			{
				returnValue = GetHandler(type, visual?.GetType() ?? _defaultVisualType);
			}
			else
			{
				Type handlerType = GetHandlerType(type, visual?.GetType() ?? _defaultVisualType);
				Registrar.CheckIfRendererIsCompatibilityRenderer(handlerType);

				if (handlerType != null)
					returnValue = (TRegistrable)DependencyResolver.ResolveOrCreate(handlerType, source, visual?.GetType(), args);
			}

			return returnValue;
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

		[return: DynamicallyAccessedMembers(HandlerType.TargetMembers)]
		public Type GetHandlerType(Type viewType, Type visualType)
		{
			visualType = visualType ?? _defaultVisualType;

			// 1. Do we have this specific type registered already?
			if (_handlers.TryGetValue(viewType, out Dictionary<Type, HandlerType> visualRenderers))
				if (visualRenderers.TryGetValue(visualType, out HandlerType specificTypeRenderer))
					return specificTypeRenderer.Target;
			//else if (visualType == _materialVisualType)
			//	VisualMarker.MaterialCheck();

			if (visualType != _defaultVisualType && visualRenderers != null)
				if (visualRenderers.TryGetValue(_defaultVisualType, out HandlerType specificTypeRenderer))
					return specificTypeRenderer.Target;

			// 2. Do we have a RenderWith for this type or its base types? Register them now.
			RegisterRenderWithTypes(viewType, visualType);

			// 3. Do we have a custom renderer for a base type or did we just register an appropriate renderer from RenderWith?
			if (LookupHandlerType(viewType, visualType, out HandlerType baseTypeRenderer))
				return baseTypeRenderer.Target;
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

		bool LookupHandlerType(Type viewType, Type visualType, out HandlerType handlerType)
		{
			visualType = visualType ?? _defaultVisualType;
			while (viewType != null && viewType != typeof(Element))
			{
				if (_handlers.TryGetValue(viewType, out Dictionary<Type, HandlerType> visualRenderers))
					if (visualRenderers.TryGetValue(visualType, out handlerType))
						return true;

				if (visualType != _defaultVisualType && visualRenderers != null)
					if (visualRenderers.TryGetValue(_defaultVisualType, out handlerType))
						return true;

				viewType = viewType.BaseType;
			}

			handlerType = new(null, 0);
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
				if (!_handlers.TryGetValue(viewType, out Dictionary<Type, HandlerType> visualRenderers) ||
					!(visualRenderers.ContainsKey(visualType) ||
					  visualRenderers.ContainsKey(_defaultVisualType)))
				{
					// get RenderWith attribute for just this type, do not inherit attributes from base types
					var attribute = viewType.GetCustomAttributes<RenderWithAttribute>(false).FirstOrDefault();
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
							var attribute2 = specificTypeRenderer.GetCustomAttribute<RenderWithAttribute>();
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

								viewType = viewType.BaseType;
								continue;
							}
						}

						Register(viewType, specificTypeRenderer, new[] { visualType }); // Register this so we don't have to look for the RenderWithAttibute again in the future
					}
				}

				viewType = viewType.BaseType;
			}
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.Registrar' and position()=1]/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class Registrar
	{
		static Registrar()
		{
			Registered = new Registrar<IRegisterable>();
		}

		internal struct EffectType
		{
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
			public readonly Type Type;

			public EffectType(
				[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
			{
				Type = type;
			}
		}

		internal static Dictionary<string, EffectType> Effects { get; } = new(StringComparer.Ordinal);

		internal static Dictionary<string, IList<StylePropertyAttribute>> StyleProperties => LazyStyleProperties.Value;

		static bool DisableCSS = false;
		static readonly Lazy<Dictionary<string, IList<StylePropertyAttribute>>> LazyStyleProperties = new Lazy<Dictionary<string, IList<StylePropertyAttribute>>>(LoadStyleSheets);

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='ExtraAssemblies']/Docs/*" />
		public static IEnumerable<Assembly> ExtraAssemblies { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='Registered']/Docs/*" />
		public static Registrar<IRegisterable> Registered { get; internal set; }

		//typeof(ExportRendererAttribute);
		//typeof(ExportCellAttribute);
		//typeof(ExportImageSourceHandlerAttribute);
		//TODO this is no longer used?
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='RegisterRenderers']/Docs/*" />
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

		// This is used when you're running an app that only knows about handlers (.NET MAUI app)
		// If the user has called Forms.Init() this will register all found types 
		// into the handlers registrar and then it will use this factory to create a shim
		internal static Func<object, IViewHandler> RendererToHandlerShim { get; private set; }
		public static void RegisterRendererToHandlerShim(Func<object, IViewHandler> handlerShim)
		{
			RendererToHandlerShim = handlerShim;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='RegisterStylesheets']/Docs/*" />
		public static void RegisterStylesheets(InitializationFlags flags)
		{
			if ((flags & InitializationFlags.DisableCss) == InitializationFlags.DisableCss)
				DisableCSS = true;
		}

		static Dictionary<string, IList<StylePropertyAttribute>> LoadStyleSheets()
		{
			var properties = new Dictionary<string, IList<StylePropertyAttribute>>(StringComparer.Ordinal);
			if (DisableCSS)
				return properties;
			var assembly = typeof(StylePropertyAttribute).Assembly;
			var styleAttributes = assembly.GetCustomAttributesSafe(typeof(StylePropertyAttribute));
			var stylePropertiesLength = styleAttributes?.Length ?? 0;
			for (var i = 0; i < stylePropertiesLength; i++)
			{
				var attribute = (StylePropertyAttribute)styleAttributes[i];
				if (properties.TryGetValue(attribute.CssPropertyName, out var attrList))
					attrList.Add(attribute);
				else
					properties[attribute.CssPropertyName] = new List<StylePropertyAttribute> { attribute };
			}
			return properties;
		}

		internal static void RegisterEffects(Assembly[] assemblies)
		{
			foreach (Assembly assembly in assemblies)
			{
				object[] effectAttributes = assembly.GetCustomAttributesSafe(typeof(ExportEffectAttribute));
				if (effectAttributes == null || effectAttributes.Length == 0)
				{
					continue;
				}

				string resolutionName = assembly.FullName;
				var resolutionNameAttribute = (ResolutionGroupNameAttribute)assembly.GetCustomAttribute(typeof(ResolutionGroupNameAttribute));
				if (resolutionNameAttribute != null)
					resolutionName = resolutionNameAttribute.ShortName;

				//NOTE: a simple cast to ExportEffectAttribute[] failed on UWP, hence the Array.Copy
				var typedEffectAttributes = new ExportEffectAttribute[effectAttributes.Length];
				Array.Copy(effectAttributes, typedEffectAttributes, effectAttributes.Length);
				RegisterEffects(resolutionName, typedEffectAttributes);
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='RegisterEffects']/Docs/*" />
		public static void RegisterEffects(string resolutionName, ExportEffectAttribute[] effectAttributes)
		{
			var exportEffectsLength = effectAttributes.Length;
			for (var i = 0; i < exportEffectsLength; i++)
			{
				var effect = effectAttributes[i];
				RegisterEffect(resolutionName, effect.Id, effect.Type);
			}
		}

		public static void RegisterEffect(
			string resolutionName,
			string id,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type effectType)
		{
			Effects[resolutionName + "." + id] = new(effectType);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='RegisterAll'][1]/Docs/*" />
		[Obsolete]
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public static void RegisterAll(Type[] attrTypes, IFontRegistrar fontRegistrar = null)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			RegisterAll(attrTypes, default(InitializationFlags), fontRegistrar);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/Registrar.xml" path="//Member[@MemberName='RegisterAll'][2]/Docs/*" />
		[Obsolete]
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public static void RegisterAll(Type[] attrTypes, InitializationFlags flags, IFontRegistrar fontRegistrar = null)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			RegisterAll(
				AppDomain.CurrentDomain.GetAssemblies(),
				Device.DefaultRendererAssembly,
				attrTypes,
				flags,
				null,
				fontRegistrar);
		}

		internal static void RegisterAll(
			Assembly[] assemblies,
			Assembly defaultRendererAssembly,
			Type[] attrTypes,
			InitializationFlags flags,
			Action<(Type handler, Type target)> viewRegistered,
			IFontRegistrar fontRegistrar = null)
		{
			Profile.FrameBegin();

			if (ExtraAssemblies != null)
				assemblies = assemblies.Union(ExtraAssemblies).ToArray();

			if (defaultRendererAssembly != null)
			{
				int indexOfExecuting = Array.IndexOf(assemblies, defaultRendererAssembly);
				if (indexOfExecuting > 0)
				{
					assemblies[indexOfExecuting] = assemblies[0];
					assemblies[0] = defaultRendererAssembly;
				}
			}

			if (fontRegistrar == null)
				fontRegistrar = Application.Current?.FindMauiContext()?.Services?.GetService<IFontRegistrar>();

			// Don't use LINQ for performance reasons
			// Naive implementation can easily take over a second to run
			Profile.FramePartition("Reflect");
			foreach (Assembly assembly in assemblies)
			{
				string frameName = Profile.IsEnabled ? assembly.GetName().Name : "Assembly";
				Profile.FrameBegin(frameName);

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
						if (attribute == null && (a is ExportFontAttribute fa))
						{
							fontRegistrar?.Register(fa.FontFileName, fa.Alias, assembly);
						}
						else
						{
							if (attribute.ShouldRegister())
							{
								Registered.Register(attribute.HandlerType, attribute.TargetType, attribute.SupportedVisuals, attribute.Priority);

								// I realize these names seem wrong from the name of the action but in Xamarin.Forms we were calling
								// the View types (Button, Image, etc..) handlers
								viewRegistered?.Invoke((attribute.TargetType, attribute.HandlerType));
							}
						}
					}
				}

				Profile.FrameEnd(frameName);
			}

			RegisterStylesheets(flags);
			Profile.FramePartition("DependencyService.Initialize");
			DependencyService.Initialize(assemblies);

			Profile.FrameEnd();
		}

		internal static void CheckIfRendererIsCompatibilityRenderer(Type rendererType)
		{
			if (typeof(IRegisterable).IsAssignableFrom(rendererType))
				return;

			if (typeof(IElementHandler).IsAssignableFrom(rendererType))
			{
				throw new InvalidOperationException($"{rendererType} will work with AddHandler. Please use AddHandler instead of AddCompatibilityRenderer.");
			}
		}
	}
}
