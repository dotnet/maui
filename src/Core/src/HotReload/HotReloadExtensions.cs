#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public static class HotReloadExtensions
	{
		public static void CheckHandlers(this IView? view)
		{
			if (view?.Handler == null)
				return;
			//So we can be smart and keep all old handlers
			//However with the Old Legacy Shim layouts, this causes issues.
			//So for now I am just going to kill all handlers, so everything needs rebuilt
			//var handlerType = handlerServiceProvider.GetHandlerType(view.GetType());
			//if (handlerType != view.Handler.GetType()){
			//	view.Handler = null;
			//}
			view.Handler = null;

			if (view is IContentView p)
			{
				CheckHandlers(p.PresentedContent);
			}

			if (view is IContainer layout)
			{
				foreach (var v in layout)
					CheckHandlers(v);
			}
		}

		public static List<MethodInfo> GetOnHotReloadMethods(this Type type) => getOnHotReloadMethods(type).Distinct(new ReflectionMethodComparer()).ToList();

		static IEnumerable<MethodInfo> getOnHotReloadMethods(Type type, bool isSubclass = false)
		{
			var flags = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic;
			if (isSubclass)
				flags = BindingFlags.Static | BindingFlags.NonPublic;
			var foos = type.GetMethods(flags).Where(x => x.GetCustomAttributes(typeof(OnHotReloadAttribute), true).Any()).ToList();
			foreach (var foo in foos)
				yield return foo;

			if (type.BaseType != null)
				foreach (var foo in getOnHotReloadMethods(type.BaseType, true))
					yield return foo;
		}

		class ReflectionMethodComparer : IEqualityComparer<MethodInfo>
		{
			public bool Equals(MethodInfo? g1, MethodInfo? g2) => g1?.MethodHandle == g2?.MethodHandle;

			public int GetHashCode(MethodInfo obj) => obj.MethodHandle.GetHashCode();
		}

	}
}
