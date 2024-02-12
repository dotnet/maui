#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			if (!IsSupported || !IsEnabled)
				return;
			currentViews[view] = parameters;
		}

		public static void UnRegister(IHotReloadableView view)
		{
			if (!IsSupported || !IsEnabled)
				return;
			currentViews.Remove(view);
		}
		public static bool IsReplacedView(IHotReloadableView view, IView newView)
		{
			if (!IsSupported || !IsEnabled)
				return false;
			if (view == null || newView == null)
				return false;

			if (!replacedViews.TryGetValue(view.GetType().FullName!, out var newViewType))
				return false;
			return newView.GetType() == newViewType;
		}
		public static IView GetReplacedView(IHotReloadableView view)
		{
			if (!IsSupported || !IsEnabled)
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
		public static void RegisterReplacedView(string oldViewType, Type newViewType)
		{
			if (!IsSupported || !IsEnabled)
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
			{
				if (replacedHandlers.TryGetValue(oldViewType, out var vTypes))
				{
					foreach (var vType in vTypes)
						RegisterHandler(vType, newViewType);
					return;
				}

				_ = HandlerService ?? throw new ArgumentNullException(nameof(HandlerService));
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				var t = assemblies.Select(x => x.GetType(oldViewType)).FirstOrDefault(x => x != null);

				var views = HandlerService!.Where(x => x.ImplementationType == t).Select(x => new KeyValuePair<Type, Type>(x.ServiceType, x.ImplementationType!)).ToList();


				replacedHandlers[oldViewType] = views.ToList();
				foreach (var h in views)
				{
					RegisterHandler(h, newViewType);
				}
			}

		}


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
