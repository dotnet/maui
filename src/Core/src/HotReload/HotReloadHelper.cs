#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public static class MauiHotReloadHelper
	{
		static IMauiHandlersCollection? HandlerService;
		//static IMauiHandlersServiceProvider? HandlerServiceProvider;
		public static void RegisterHandlers(IMauiHandlersCollection handlerService)
		{
			HandlerService = handlerService;
		}
		public static void AddActiveView(IHotReloadableView view) => ActiveViews.Add(view);
		public static void Reset()
		{
			replacedViews.Clear();
			activatedHandlerTypes.Clear();

			lock (handlerReplacementLock)
			{
				replacedHandlers.Clear();
				pendingHandlerReplacements.Clear();
			}
		}
		public static bool IsEnabled { get; set; } = Debugger.IsAttached;

		internal static bool IsSupported
#if !NETSTANDARD
			=> System.Reflection.Metadata.MetadataUpdater.IsSupported;
#else
			=> true;
#endif

		public static void Register(IHotReloadableView view, params object[] parameters)
		{
			// Check separately to avoid trim warnings
			if (!IsSupported)
				return;

			if (!IsEnabled)
				return;

			currentViews[view] = parameters;
		}

		public static void UnRegister(IHotReloadableView view)
		{
			// Check separately to avoid trim warnings
			if (!IsSupported)
				return;

			if (!IsEnabled)
				return;

			currentViews.Remove(view);
		}
		public static bool IsReplacedView(IHotReloadableView view, IView newView)
		{
			// Check separately to avoid trim warnings
			if (!IsSupported)
				return false;

			if (!IsEnabled)
				return false;

			if (view == null || newView == null)
				return false;

			if (!replacedViews.TryGetValue(view.GetType().FullName!, out var newViewType))
				return false;
			return newView.GetType() == newViewType;
		}
		public static IView GetReplacedView(IHotReloadableView view)
		{
			// Check separately to avoid trim warnings
			if (!IsSupported)
				return view;

			if (!IsEnabled)
				return view;

			var viewType = view.GetType();
			if (!replacedViews.TryGetValue(viewType.FullName!, out var newViewType) || viewType == newViewType)
				return view;

			currentViews.TryGetValue(view, out var parameters);
			try
			{
				//TODO: Add in a way to use IoC and DI
				var newView = (IView)(parameters?.Length > 0 ? Activator.CreateInstance(newViewType, args: parameters) : Activator.CreateInstance(newViewType))!;
				TransferState(view, newView);
				return newView;
			}
			catch (MissingMethodException)
			{
				Debug.WriteLine("You are using trying to HotReload a view that requires Parameters. Please call `HotReloadHelper.Register(this, params);` in the constructor;");
				//TODO: Notify that we couldnt hot reload.
				return view;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error Hotreloading type: {newViewType}");
				Debug.WriteLine(ex);
				//TODO: Notify that we couldnt hot reload.
				return view;
			}

		}

		static void TransferState(IHotReloadableView oldView, IView newView)
		{
			oldView.TransferState(newView);
		}

		static internal readonly WeakList<IHotReloadableView> ActiveViews = new WeakList<IHotReloadableView>();
		static Dictionary<string, Type> replacedViews = new(StringComparer.Ordinal);
		static Dictionary<IHotReloadableView, object[]> currentViews = new Dictionary<IHotReloadableView, object[]>();
		static Dictionary<string, List<KeyValuePair<Type, Type>>> replacedHandlers = new(StringComparer.Ordinal);
		static ConcurrentDictionary<string, Type> pendingHandlerReplacements = new(StringComparer.Ordinal);
		static ConcurrentDictionary<ServiceDescriptor, Type> activatedHandlerTypes = new();
		static readonly object handlerReplacementLock = new();

		[UnconditionalSuppressMessage("Trimming", "IL2026",
			Justification = "Pending replacements are only populated by the trim-incompatible Hot Reload path.")]
		[UnconditionalSuppressMessage("AOT", "IL3050",
			Justification = "Pending replacements are only populated by the AOT-incompatible Hot Reload path.")]
		internal static void RegisterHandlerType(ServiceDescriptor descriptor, Type handlerType)
		{
			activatedHandlerTypes[descriptor] = handlerType;

			if (pendingHandlerReplacements.IsEmpty || handlerType.FullName is not string handlerTypeName)
				return;

			lock (handlerReplacementLock)
			{
				if (!pendingHandlerReplacements.TryGetValue(handlerTypeName, out var newHandlerType))
					return;

				var handler = new KeyValuePair<Type, Type>(descriptor.ServiceType, handlerType);
				if (!replacedHandlers.TryGetValue(handlerTypeName, out var views))
				{
					views = new List<KeyValuePair<Type, Type>>();
					replacedHandlers[handlerTypeName] = views;
				}

				if (views.Contains(handler))
					return;

				views.Add(handler);
				RegisterHandler(handler, newHandlerType);
			}
		}

		[RequiresUnreferencedCode("Hot Reload is not trim compatible")]
#if !NETSTANDARD
		[RequiresDynamicCode("Hot Reload is not AOT compatible")]
#endif
		public static void RegisterReplacedView(string oldViewType, Type newViewType)
		{
			// Check separately to avoid trim warnings
			if (!IsSupported)
				return;

			if (!IsEnabled)
				return;

			Action<MethodInfo> executeStaticMethod = (method) =>
			{
				try
				{
					method?.Invoke(null, null);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Error calling {method.Name} on type: {newViewType}");
					Debug.WriteLine(ex);
					//TODO: Notify that we couldnt execute OnHotReload for the Method;
				}
			};

			var onHotReloadMethods = newViewType.GetOnHotReloadMethods();
			onHotReloadMethods.ForEach(x => executeStaticMethod(x));

			if (typeof(IHotReloadableView).IsAssignableFrom(newViewType))
				replacedViews[oldViewType] = newViewType;

			if (typeof(IViewHandler).IsAssignableFrom(newViewType))
				RegisterHandlerReplacement(oldViewType, newViewType);
		}

		[RequiresUnreferencedCode("Hot Reload is not trim compatible")]
#if !NETSTANDARD
		[RequiresDynamicCode("Hot Reload is not AOT compatible")]
#endif
		internal static void RegisterHandlerReplacement(string oldHandlerType, Type newHandlerType)
		{
			_ = HandlerService ?? throw new ArgumentNullException(nameof(HandlerService));

			List<KeyValuePair<Type, Type>> views;
			lock (handlerReplacementLock)
			{
				pendingHandlerReplacements[oldHandlerType] = newHandlerType;
				if (replacedHandlers.TryGetValue(oldHandlerType, out var registeredViews))
				{
					views = registeredViews.ToList();
				}
				else
				{
					var assemblies = AppDomain.CurrentDomain.GetAssemblies();
					var handlerType = assemblies.Select(x => x.GetType(oldHandlerType)).FirstOrDefault(x => x != null);

					views = handlerType is null
						? new List<KeyValuePair<Type, Type>>()
						: HandlerService
							.Select(x => new KeyValuePair<Type, Type?>(x.ServiceType, GetRegisteredHandlerType(x)))
							.Where(x => x.Value == handlerType)
							.Select(x => new KeyValuePair<Type, Type>(x.Key, x.Value!))
							.Distinct()
							.ToList();

					if (views.Count > 0)
						replacedHandlers[oldHandlerType] = views.ToList();
				}

				foreach (var view in views)
					RegisterHandler(view, newHandlerType);
			}
		}

		static Type? GetRegisteredHandlerType(ServiceDescriptor descriptor) =>
			descriptor.ImplementationType
				?? (activatedHandlerTypes.TryGetValue(descriptor, out var handlerType) ? handlerType : null);

		[RequiresUnreferencedCode("Hot Reload is not trim compatible")]
#if !NETSTANDARD
		[RequiresDynamicCode("Hot Reload is not AOT compatible")]
#endif
		static void RegisterHandler(KeyValuePair<Type, Type> pair, Type newHandler)
		{
			_ = HandlerService ?? throw new ArgumentNullException(nameof(HandlerService));
			var view = pair.Key;
			var newType = newHandler;
			if (pair.Value.IsGenericType)
				newType = pair.Value.GetGenericTypeDefinition().MakeGenericType(newHandler);
			HandlerService.AddHandler(view, newType);
		}

		public static void TriggerReload()
		{
			List<IHotReloadableView>? roots = null;
			while (roots == null)
			{
				try
				{
					roots = ActiveViews.Where(x => x != null && x.Parent == null).ToList();
				}
				catch
				{
					//Sometimes we get list changed exception.
				}
			}

			foreach (var view in roots)
			{
				view!.Reload();
			}
		}
		#region Metadata Update Handler
		[RequiresUnreferencedCode("Hot Reload is not trim compatible")]
#if !NETSTANDARD
		[RequiresDynamicCode("Hot Reload is not AOT compatible")]
#endif
		public static void UpdateApplication(Type[] types)
		{
			IsEnabled = true;
			foreach (var t in types)
				RegisterReplacedView(t.FullName ?? "", t);
		}
		public static void ClearCache(Type[] types) => TriggerReload();
		#endregion
	}
}
