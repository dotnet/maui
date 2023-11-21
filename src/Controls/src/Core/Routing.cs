#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="Type[@FullName='Microsoft.Maui.Controls.Routing']/Docs/*" />
	public static class Routing
	{
		static int s_routeCount = 0;
		static Dictionary<string, RouteFactory> s_routes = new(StringComparer.Ordinal);
		static Dictionary<string, Page> s_implicitPageRoutes = new(StringComparer.Ordinal);
		static HashSet<string> s_routeKeys;

		const string ImplicitPrefix = "IMPL_";
		const string DefaultPrefix = "D_FAULT_";
		internal const string PathSeparator = "/";

		// We only need these while a navigation is happening 
		internal static void ClearImplicitPageRoutes()
		{
			s_implicitPageRoutes.Clear();
			s_routeKeys = null;
		}

		internal static void RegisterImplicitPageRoute(Page page)
		{
			var route = GetRoute(page);
			if (!IsUserDefined(route))
			{
				s_implicitPageRoutes[route] = page;
				s_routeKeys = null;
			}
		}

		// Shell works much better if the entire nav stack can be represented by a string
		// If the users pushes pages without using routes we want these page keys tracked
		internal static void RegisterImplicitPageRoutes(Shell shell)
		{
			foreach (var item in shell.Items)
			{
				foreach (var section in item.Items)
				{
					var navigationStackCount = section.Navigation.NavigationStack.Count;
					for (int i = 1; i < navigationStackCount; i++)
					{
						RegisterImplicitPageRoute(section.Navigation.NavigationStack[i]);
					}
					var navigationModalStackCount = section.Navigation.ModalStack.Count;
					for (int i = 0; i < navigationModalStackCount; i++)
					{
						var page = section.Navigation.ModalStack[i];
						RegisterImplicitPageRoute(page);

						if (page is INavigationPageController np)
						{
							foreach (var npPages in np.Pages)
							{
								RegisterImplicitPageRoute(npPages);
							}
						}
					}
				}
			}
		}

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

		internal static bool IsDefault(BindableObject source)
		{
			return IsDefault(GetRoute(source));
		}

		internal static bool IsUserDefined(BindableObject source)
		{
			if (source == null)
				return false;

			return IsUserDefined(GetRoute(source));
		}

		internal static bool IsUserDefined(string route)
		{
			if (route == null)
				return false;

			return !(IsDefault(route) || IsImplicit(route));
		}

		internal static void Clear()
		{
			s_implicitPageRoutes.Clear();
			s_routes.Clear();
			s_routeKeys = null;
		}

		/// <summary>Bindable property for attached property <c>Route</c>.</summary>
		public static readonly BindableProperty RouteProperty = CreateRouteProperty();

		[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2111:ReflectionToDynamicallyAccessedMembers",
			Justification = "The CreateAttached method has a DynamicallyAccessedMembers annotation for all public methods"
			+ "on the declaring type. This includes the Routing.RegisterRoute(string, Type) method which also has a "
			+ "DynamicallyAccessedMembers annotation and the trimmer can't guarantee the availability of the requirements"
			+ "of the method. The RegisterRoute method is never used via reflection so we can safely ignore the warning.")]
		private static BindableProperty CreateRouteProperty()
			=> BindableProperty.CreateAttached("Route", typeof(string), typeof(Routing), null,
				defaultValueCreator: CreateDefaultRoute);

		static object CreateDefaultRoute(BindableObject bindable)
		{
			return $"{DefaultPrefix}{bindable.GetType().Name}{++s_routeCount}";
		}

		internal static HashSet<string> GetRouteKeys()
		{
			var keys = s_routeKeys;
			if (keys != null)
				return keys;

			keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (var key in s_routes.Keys)
			{
				keys.Add(ShellUriHandler.FormatUri(key));
			}
			foreach (var key in s_implicitPageRoutes.Keys)
			{
				keys.Add(ShellUriHandler.FormatUri(key));
			}
			return s_routeKeys = keys;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='GetOrCreateContent']/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public static Element GetOrCreateContent(string route, IServiceProvider services = null)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			Element result = null;

			if (s_implicitPageRoutes.TryGetValue(route, out var page))
			{
				return page;
			}

			if (s_routes.TryGetValue(route, out var content))
				result = content.GetOrCreate(services);

			if (result != null)
				SetRoute(result, route);

			return result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='GetRoute']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='FormatRoute'][1]/Docs/*" />
		public static string FormatRoute(List<string> segments)
		{
			var route = FormatRoute(String.Join(PathSeparator, segments));
			return route;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='FormatRoute'][2]/Docs/*" />
		public static string FormatRoute(string route)
		{
			return route;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='RegisterRoute'][2]/Docs/*" />
		public static void RegisterRoute(string route, RouteFactory factory)
		{
			if (!String.IsNullOrWhiteSpace(route))
				route = FormatRoute(route);
			ValidateRoute(route, factory);

			s_routes[route] = factory;
			s_routeKeys = null;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='UnRegisterRoute']/Docs/*" />
		public static void UnRegisterRoute(string route)
		{
			if (s_routes.Remove(route))
			{
				s_routeKeys = null;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='RegisterRoute'][1]/Docs/*" />
		public static void RegisterRoute(
			string route,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			RegisterRoute(route, new TypeRouteFactory(type));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Routing.xml" path="//Member[@MemberName='SetRoute']/Docs/*" />
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
			if (s_routes.TryGetValue(route, out existingRegistration) && !existingRegistration.Equals(routeFactory))
				throw new ArgumentException($"Duplicated Route: \"{route}\"");
		}

		class TypeRouteFactory : RouteFactory
		{
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
			readonly Type _type;

			public TypeRouteFactory(
				[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
			{
				_type = type;
			}

			public override Element GetOrCreate()
			{
				return (Element)Activator.CreateInstance(_type);
			}

			public override Element GetOrCreate(IServiceProvider services)
			{
				if (services != null)
				{
					return (Element)(services.GetService(_type) ?? Activator.CreateInstance(_type));
				}

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
