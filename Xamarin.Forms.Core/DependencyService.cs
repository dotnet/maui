using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static class DependencyService
	{
		static bool s_initialized;

		static readonly List<Type> DependencyTypes = new List<Type>();
		static readonly Dictionary<Type, DependencyData> DependencyImplementations = new Dictionary<Type, DependencyData>();

		public static T Get<T>(DependencyFetchTarget fetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
		{
			Initialize();

			Type targetType = typeof(T);

			if (!DependencyImplementations.ContainsKey(targetType))
			{
				Type implementor = FindImplementor(targetType);
				DependencyImplementations[targetType] = implementor != null ? new DependencyData { ImplementorType = implementor } : null;
			}

			DependencyData dependencyImplementation = DependencyImplementations[targetType];
			if (dependencyImplementation == null)
				return null;

			if (fetchTarget == DependencyFetchTarget.GlobalInstance)
			{
				if (dependencyImplementation.GlobalInstance == null)
				{
					dependencyImplementation.GlobalInstance = Activator.CreateInstance(dependencyImplementation.ImplementorType);
				}
				return (T)dependencyImplementation.GlobalInstance;
			}
			return (T)Activator.CreateInstance(dependencyImplementation.ImplementorType);
		}

		public static void Register<T>() where T : class
		{
			Type type = typeof(T);
			if (!DependencyTypes.Contains(type))
				DependencyTypes.Add(type);
		}

		public static void Register<T, TImpl>() where T : class where TImpl : class, T
		{
			Type targetType = typeof(T);
			Type implementorType = typeof(TImpl);
			if (!DependencyTypes.Contains(targetType))
				DependencyTypes.Add(targetType);

			DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType };
		}

		static Type FindImplementor(Type target)
		{
			return DependencyTypes.FirstOrDefault(t => target.IsAssignableFrom(t));
		}

		static void Initialize()
		{
			if (s_initialized)
			{
				return;
			}

			Assembly[] assemblies = Device.GetAssemblies();
			if (Internals.Registrar.ExtraAssemblies != null)
			{
				assemblies = assemblies.Union(Internals.Registrar.ExtraAssemblies).ToArray();
			}

			Initialize(assemblies);
		}

		internal static void Initialize(Assembly[] assemblies)
		{
			if (s_initialized)
			{
				return;
			}

			Type targetAttrType = typeof(DependencyAttribute);

			// Don't use LINQ for performance reasons
			// Naive implementation can easily take over a second to run
			foreach (Assembly assembly in assemblies)
			{
				Attribute[] attributes = assembly.GetCustomAttributes(targetAttrType).ToArray();
				if (attributes.Length == 0)
					continue;

				foreach (DependencyAttribute attribute in attributes)
				{
					if (!DependencyTypes.Contains(attribute.Implementor))
					{
						DependencyTypes.Add(attribute.Implementor);
					}
				}
			}

			s_initialized = true;
		}

		class DependencyData
		{
			public object GlobalInstance { get; set; }

			public Type ImplementorType { get; set; }
		}
	}
}