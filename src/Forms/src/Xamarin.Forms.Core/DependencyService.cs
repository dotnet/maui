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

		static readonly object s_dependencyLock = new object();
		static readonly object s_initializeLock = new object();

		static readonly List<Type> DependencyTypes = new List<Type>();
		static readonly Dictionary<Type, DependencyData> DependencyImplementations = new Dictionary<Type, DependencyData>();

		public static T Resolve<T>(DependencyFetchTarget fallbackFetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
		{
			var result = DependencyResolver.Resolve(typeof(T)) as T;

			return result ?? Get<T>(fallbackFetchTarget);
		}

		public static T Get<T>(DependencyFetchTarget fetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
		{
			Initialize();

			DependencyData dependencyImplementation;
			lock (s_dependencyLock)
			{
				Type targetType = typeof(T);
				if (!DependencyImplementations.TryGetValue(targetType, out dependencyImplementation))
				{
					Type implementor = FindImplementor(targetType);
					DependencyImplementations[targetType] = (dependencyImplementation = implementor != null ? new DependencyData { ImplementorType = implementor } : null);
				}
			}

			if (dependencyImplementation == null)
				return null;

			if (fetchTarget == DependencyFetchTarget.GlobalInstance)
			{
				if (dependencyImplementation.GlobalInstance == null)
				{
					lock (dependencyImplementation)
					{
						if (dependencyImplementation.GlobalInstance == null)
						{
							dependencyImplementation.GlobalInstance = Activator.CreateInstance(dependencyImplementation.ImplementorType);
						}
					}
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

			lock (s_dependencyLock)
				DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType };
		}

		public static void RegisterSingleton<T>(T instance) where T : class
		{
			Type targetType = typeof(T);
			Type implementorType = typeof(T);
			if (!DependencyTypes.Contains(targetType))
				DependencyTypes.Add(targetType);

			lock (s_dependencyLock)
				DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType, GlobalInstance = instance };
		}

		static Type FindImplementor(Type target) =>
			DependencyTypes.FirstOrDefault(t => target.IsAssignableFrom(t));

		static void Initialize()
		{
			if (s_initialized)
				return;

			lock (s_initializeLock)
			{
				if (s_initialized)
					return;

				Assembly[] assemblies = Device.GetAssemblies();
				if (Internals.Registrar.ExtraAssemblies != null)
				{
					assemblies = assemblies.Union(Internals.Registrar.ExtraAssemblies).ToArray();
				}

				Initialize(assemblies);
			}
		}

		internal static void Initialize(Assembly[] assemblies)
		{
			if (s_initialized)
				return;

			lock (s_initializeLock)
			{
				if (s_initialized)
					return;

				// Don't use LINQ for performance reasons
				// Naive implementation can easily take over a second to run
				foreach (Assembly assembly in assemblies)
				{
					object[] attributes = assembly.GetCustomAttributesSafe(typeof(DependencyAttribute));
					if (attributes == null)
						continue;

					for (int i = 0; i < attributes.Length; i++)
					{
						DependencyAttribute attribute = (DependencyAttribute)attributes[i];
						if (!DependencyTypes.Contains(attribute.Implementor))
						{
							DependencyTypes.Add(attribute.Implementor);
						}
					}
				}

				s_initialized = true;
			}
		}

		class DependencyData
		{
			public object GlobalInstance { get; set; }

			public Type ImplementorType { get; set; }
		}
	}
}