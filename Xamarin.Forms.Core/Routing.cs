using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xamarin.Forms
{
	public static class Routing
	{
		static int s_routeCount = 0;
		static Dictionary<string, RouteFactory> s_routes = new Dictionary<string, RouteFactory>();

		internal const string ImplicitPrefix = "IMPL_";

		internal static string GenerateImplicitRoute (string source)
		{
			if (source.StartsWith(ImplicitPrefix, StringComparison.Ordinal))
				return source;
			return ImplicitPrefix + source;
		}

		internal static bool CompareRoutes(string route, string compare, out bool isImplicit)
		{
			if (isImplicit = route.StartsWith(ImplicitPrefix, StringComparison.Ordinal))
				route = route.Substring(ImplicitPrefix.Length);

			if (compare.StartsWith(ImplicitPrefix, StringComparison.Ordinal))
				throw new Exception();

			return route == compare;
		}

		public static readonly BindableProperty RouteProperty =
			BindableProperty.CreateAttached("Route", typeof(string), typeof(Routing), null, 
				defaultValueCreator: CreateDefaultRoute);

		static object CreateDefaultRoute(BindableObject bindable)
		{
			return bindable.GetType().Name + ++s_routeCount;
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

		public static string GetRoute(Element obj)
		{
			return (string)obj.GetValue(RouteProperty);
		}

		public static void RegisterRoute(string route, RouteFactory factory)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only lowercase letters");

			s_routes[route] = factory;
		}

		public static void RegisterRoute(string route, Type type)
		{
			if (!ValidateRoute(route))
				throw new ArgumentException("Route must contain only lowercase letters");

			s_routes[route] = new TypeRouteFactory(type);
		}

		public static void SetRoute(Element obj, string value)
		{
			obj.SetValue(RouteProperty, value);
		}

		static bool ValidateRoute(string route)
		{
			// Honestly this could probably be expanded to allow any URI allowable character
			// I just dont want to figure out what that validation looks like.
			// It does however need to be lowercase since uri case sensitivity is a bit touchy
			Regex r = new Regex("^[a-z]*$");
			return r.IsMatch(route);
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
		}
	}
}