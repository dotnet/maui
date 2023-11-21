#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DependencyService.xml" path="Type[@FullName='Microsoft.Maui.Controls.DependencyService']/Docs/*" />
	public static class DependencyService
	{
		static bool s_initialized;

		static readonly object s_dependencyLock = new object();
		static readonly object s_initializeLock = new object();

		static readonly List<DependencyType> DependencyTypes = new List<DependencyType>();
		static readonly Dictionary<Type, DependencyData> DependencyImplementations = new Dictionary<Type, DependencyData>();

		public static T Resolve<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(DependencyFetchTarget fallbackFetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
		{
			var result = DependencyResolver.Resolve(typeof(T)) as T;

			return result ?? Get<T>(fallbackFetchTarget);
		}

		public static T Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(DependencyFetchTarget fetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
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

		public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>() where T : class
		{
			Type type = typeof(T);
			AddDependencyTypeIfNeeded(type);
		}

		public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TImpl>() where T : class where TImpl : class, T
		{
			Type targetType = typeof(T);
			Type implementorType = typeof(TImpl);
			AddDependencyTypeIfNeeded(targetType);

			lock (s_dependencyLock)
				DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType };
		}

		public static void RegisterSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T instance) where T : class
		{
			Type targetType = typeof(T);
			Type implementorType = typeof(T);
			AddDependencyTypeIfNeeded(targetType);

			lock (s_dependencyLock)
				DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType, GlobalInstance = instance };
		}

		static void AddDependencyTypeIfNeeded(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			if (!DependencyTypes.Any(t => t.Type == type))
				DependencyTypes.Add(new DependencyType(type));
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		static Type FindImplementor(Type target) =>
			DependencyTypes.FirstOrDefault(t => target.IsAssignableFrom(t.Type))?.Type;

		// Once we get essentials/cg converted to using startup.cs
		// we will delete the initialize code from here and just use
		// explicit assembly registration via startup code
		internal static void SetToInitialized()
		{
			s_initialized = true;
		}

		static void Initialize()
		{
			if (s_initialized)
				return;

			lock (s_initializeLock)
			{
				if (s_initialized)
					return;

				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				if (Internals.Registrar.ExtraAssemblies != null)
				{
					assemblies = assemblies.Union(Internals.Registrar.ExtraAssemblies).ToArray();
				}

				Initialize(assemblies);
			}
		}

		public static void Register(Assembly[] assemblies)
		{
			lock (s_initializeLock)
			{
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
						AddDependencyTypeIfNeeded(attribute.Implementor);
					}
				}
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

				Register(assemblies);

				s_initialized = true;
			}
		}

		class DependencyData
		{
			public object GlobalInstance { get; set; }

			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
			public Type ImplementorType { get; set; }
		}

		class DependencyType : IEquatable<DependencyType>
		{
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
			public Type Type { get; }

			public DependencyType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
			{
				Type = type;
			}

			public override int GetHashCode() => Type.GetHashCode();
			public override bool Equals(object obj) => obj is DependencyType other && Equals(other);
			public bool Equals(DependencyType other) => other.Type == Type;
		}
	}
}
