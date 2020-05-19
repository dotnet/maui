using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.Maui
{
	public static class Routing
	{
		static int s_routeCount = 0;
		static Dictionary<string, RouteFactory> s_routes = new Dictionary<string, RouteFactory>();

		const string ImplicitPrefix = "IMPL_";
		const string DefaultPrefix = "D_FAULT_";
		const string _pathSeparator = "/";

		internal static string GenerateImplicitRoute(string source)
		{
			if (IsImplicit(source))
				return source;
			return String.Concat(ImplicitPrefix, source);
		}
		internal static bool IsImplicit(string source)
		{
			return source.StartsWith(ImplicitPrefix, StringComparison.Ordinal);
		}
		internal static bool IsImplicit(BindableObject source)
		{
			return IsImplicit(GetRoute(source));
		}
		internal static bool IsDefault(string source)
		{
			return source.StartsWith(DefaultPrefix, StringComparison.Ordinal);
		}

		internal static void Clear()
		{
			s_routes.Clear();
		}

		public static readonly BindableProperty RouteProperty =
			BindableProperty.CreateAttached("Route", typeof(string), typeof(Routing), null,
				defaultValueCreator: CreateDefaultRoute);

		static object CreateDefaultRoute(BindableObject bindable)
		{
			return $"{DefaultPrefix}{bindable.GetType().Name}{++s_routeCount}";
		}

		internal static string[] GetRouteKeys()
		{
			string[] keys = new string[s_routes.Count];
			s_routes.Keys.CopyTo(keys, 0);
			return keys;
		}

		public static Element GetOrCreateContent(string route)
		{
			Element result = null;

			if (s_routes.TryGetValue(route, out var content))
				result = content.GetOrCreate();

			if (result == null)
			{
				// okay maybe its a type, we'll try that just to be nice to the user
				var type = Type.GetType(route);
				if (type != null)
					result = Activator.CreateInstance(type) as Element;
			}

			if (result != null)
				SetRoute(result, route);

			return result;
		}

		public static string GetRoute(BindableObject obj)
		{
			return (string)obj.GetValue(RouteProperty);
		}

		internal static string GetRoutePathIfNotImplicit(Element obj)
		{
			var source = GetRoute(obj);
			if (IsImplicit(source))
				return String.Empty;

			return $"{source}/";
		}

		internal static Uri Remove(Uri uri, bool implicitRoutes, bool defaultRoutes)
		{
			uri = ShellUriHandler.FormatUri(uri, null);

			string[] parts = uri.OriginalString.TrimEnd(_pathSeparator[0]).Split(_pathSeparator[0]);

			bool userDefinedRouteAdded = false;
			List<string> toKeep = new List<string>();
			for (int i = 0; i < parts.Length; i++)
			{
				// This means there are no routes defined on the shell but the user has navigated to a global route
				// so we need to attach the final route where the user left the shell
				if (s_routes.ContainsKey(parts[i]) && !userDefinedRouteAdded && i > 0)
				{
					toKeep.Add(parts[i - 1]);
				}

				if (!(IsDefault(parts[i]) && defaultRoutes) && !(IsImplicit(parts[i]) && implicitRoutes))
				{
					if (!String.IsNullOrWhiteSpace(parts[i]))
						userDefinedRouteAdded = true;

					toKeep.Add(parts[i]);
				}
			}

			if(!userDefinedRouteAdded && parts.Length > 0)
			{
				toKeep.Add(parts[parts.Length - 1]);
			}

			return new Uri(string.Join(_pathSeparator, toKeep), UriKind.Relative);
		}

		public static string FormatRoute(List<string> segments)
		{
			var route = FormatRoute(String.Join(_pathSeparator, segments));
			return route;
		}

		public static string FormatRoute(string route)
		{
			return route;
		}

		public static void RegisterRoute(string route, RouteFactory factory)
		{
			if (!String.IsNullOrWhiteSpace(route))
				route = FormatRoute(route);
			ValidateRoute(route, factory);

			s_routes[route] = factory;
		}

		public static void UnRegisterRoute(string route)
		{
			if (s_routes.TryGetValue(route, out _))
				s_routes.Remove(route);
		}

		public static void RegisterRoute(string route, Type type)
		{
			RegisterRoute(route, new TypeRouteFactory(type));
		}

		public static void SetRoute(Element obj, string value)
		{
			obj.SetValue(RouteProperty, value);
		}

		static void ValidateRoute(string route, RouteFactory routeFactory)
		{
			if (string.IsNullOrWhiteSpace(route))
				throw new ArgumentNullException(nameof(route), "Route cannot be an empty string");

			routeFactory = routeFactory ?? throw new ArgumentNullException(nameof(routeFactory), "Route Factory cannot be null");

			var uri = new Uri(route, UriKind.RelativeOrAbsolute);

			var parts = uri.OriginalString.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var part in parts)
			{
				if (IsImplicit(part))
					throw new ArgumentException($"Route contains invalid characters in \"{part}\"");
			}

			RouteFactory existingRegistration = null;
			if(s_routes.TryGetValue(route, out existingRegistration) && !existingRegistration.Equals(routeFactory))
				throw new ArgumentException($"Duplicated Route: \"{route}\"");
		}

		class TypeRouteFactory : RouteFactory
		{
			readonly Type _type;

			public TypeRouteFactory(Type type)
			{
				_type = type;
			}

			public override Element GetOrCreate()
			{
				return (Element)Activator.CreateInstance(_type);
			}
			public override bool Equals(object obj)
			{
				if ((obj is TypeRouteFactory typeRouteFactory))
					return typeRouteFactory._type == _type;

				return false;
			}

			public override int GetHashCode()
			{
				return _type.GetHashCode();
			}
		}
	}
}