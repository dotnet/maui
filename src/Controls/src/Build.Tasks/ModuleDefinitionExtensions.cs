using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class ModuleDefinitionExtensions
	{
		public static TypeReference ImportReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type) => cache.GetOrAddTypeReference(module, type);

		public static TypeReference ImportReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var typeKey = $"{type}<{string.Join(",", classArguments)}>";
			return cache.GetOrAddTypeReference(module, typeKey, x => module.ImportReference(module.ImportReference(cache, type).MakeGenericInstanceType(classArguments.Select(gp => module.GetTypeDefinition(cache, (gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray())));
		}

		public static TypeReference ImportArrayReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type)
		{
			var typeKey = $"{type}[]";
			return cache.GetOrAddTypeReference(module, typeKey, x => module.ImportReference(module.ImportReference(cache, type).MakeArrayType()));
		}

		static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, TypeReference type, TypeReference[] classArguments, Func<MethodDefinition, bool> predicate)
		{
			var ctor = module.ImportReference(type).ResolveCached(cache).Methods.FirstOrDefault(md => !md.IsPrivate && !md.IsStatic && md.IsConstructor && (predicate?.Invoke(md) ?? true));
			if (ctor is null)
				return null;
			var ctorRef = module.ImportReference(ctor);
			if (classArguments == null)
				return ctorRef;
			return module.ImportReference(ctorRef.ResolveGenericParameters(type.MakeGenericInstanceType(classArguments), module));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, TypeReference type, TypeReference[] parameterTypes)
		{
			var ctorKey = $"{type}.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes.Select(SerializeTypeReference)))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, type, classArguments: null, predicate: md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, x.module.ImportReference(parameterTypes[i])))
						return false;
				return true;
			}));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, int paramCount)
		{
			var ctorKey = $"{type}.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, x.module.GetTypeDefinition(cache, type), null, md => md.Parameters.Count == paramCount));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, TypeReference type, int paramCount)
		{
			var ctorKey = $"{type}.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, type, null, md => md.Parameters.Count == paramCount));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var ctorKey = $"{type}<{(string.Join(",", classArguments))}>.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, x.module.GetTypeDefinition(cache, type), classArguments.Select(args => x.module.GetTypeDefinition(cache, args)).ToArray(), md => md.Parameters.Count == paramCount));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, TypeReference[] classArguments)
		{
			var ctorKey = $"{type}<{string.Join(",", classArguments.Select(SerializeTypeReference))}>.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, x.module.GetTypeDefinition(cache, type), classArguments, predicate: md => md.Parameters.Count == paramCount));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] parameterTypes, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var ctorKey = $"{type}<{(string.Join(",", classArguments))}>.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, x.module.GetTypeDefinition(cache, type), classArguments.Select(args => x.module.GetTypeDefinition(cache, args)).ToArray(), md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, x.module.ImportReference(cache, parameterTypes[i])))
						return false;
				return true;
			}));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] parameterTypes)
		{
			var ctorKey = $"{type}.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			return cache.GetOrAddMethodReference(module, ctorKey, x => x.module.ImportCtorReference(cache, x.module.GetTypeDefinition(cache, type), classArguments: null, predicate: md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, x.module.ImportReference(cache, parameterTypes[i])))
						return false;
				return true;
			}));
		}

		static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, XamlCache cache, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null, bool flatten = false, bool caseSensitive = true)
		{
			var properties = module.ImportReference(type).Resolve().Properties;
			var getter = module
				.ImportReference(type)
				.ResolveCached(cache)
				.Properties(cache, flatten)
				.FirstOrDefault(pd =>
								   string.Equals(pd.Name, propertyName, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
								&& !pd.GetMethod.IsPrivate
								&& (predicate?.Invoke(pd) ?? true))
				?.GetMethod;
			return getter == null ? null : module.ImportReference(getter);
		}

		public static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, bool isStatic = false, bool flatten = false, bool caseSensitive = true)
		{
			var getterKey = $"{(isStatic ? "static " : "")}{type}.get_{propertyName}{(flatten ? "*" : "")}";
			return cache.GetOrAddMethodReference(module, getterKey, x => x.module.ImportPropertyGetterReference(cache, x.module.GetTypeDefinition(cache, type), propertyName, pd => pd.GetMethod.IsStatic == isStatic, flatten, caseSensitive: caseSensitive));
		}

		static MethodReference ImportPropertySetterReference(this ModuleDefinition module, XamlCache cache, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null)
		{
			var setter = module
				.ImportReference(type)
				.ResolveCached(cache)
				.Properties
				.FirstOrDefault(pd =>
								   pd.Name == propertyName
								&& !pd.SetMethod.IsPrivate
								&& (predicate?.Invoke(pd) ?? true))
				?.SetMethod;
			return setter == null ? null : module.ImportReference(setter);
		}

		public static MethodReference ImportPropertySetterReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, bool isStatic = false)
		{
			var setterKey = $"{(isStatic ? "static " : "")}{type}.set{propertyName}";
			return cache.GetOrAddMethodReference(module, setterKey, x => x.module.ImportPropertySetterReference(cache, x.module.GetTypeDefinition(cache, type), propertyName, pd => pd.SetMethod.IsStatic == isStatic));
		}

		static MethodReference ImportMethodReference(this ModuleDefinition module, XamlCache cache, TypeReference type, string methodName, Func<MethodDefinition, bool> predicate = null, TypeReference[] classArguments = null)
		{
			var method = module
				.ImportReference(type)
				.ResolveCached(cache)
				.Methods
				.FirstOrDefault(md =>
								   !md.IsConstructor
								&& !md.IsPrivate
								&& md.Name == methodName
								&& (predicate?.Invoke(md) ?? true));
			if (method is null)
				return null;
			var methodRef = module.ImportReference(method);
			if (classArguments == null)
				return methodRef;
			return module.ImportReference(methodRef.ResolveGenericParameters(type.MakeGenericInstanceType(classArguments), module));
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module,
													 XamlCache cache,
													 TypeReference type,
													 string methodName,
													 TypeReference[] parameterTypes = null,
													 TypeReference[] classArguments = null,
													 bool isStatic = false)
		{
			return module.ImportMethodReference(cache, type,
												methodName: methodName,
												predicate: md =>
												{
													if (md.IsStatic != isStatic)
														return false;
													if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
														return false;
													for (var i = 0; i < md.Parameters.Count; i++)
														if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, parameterTypes[i]))
															return false;
													return true;
												},
												classArguments: classArguments);
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module,
															XamlCache cache,
															(string assemblyName, string clrNamespace, string typeName) type,
															string methodName,
															(string assemblyName, string clrNamespace, string typeName)[] parameterTypes,
															(string assemblyName, string clrNamespace, string typeName)[] classArguments = null,
															bool isStatic = false)
		{
			var methodKey = $"{(isStatic ? "static " : "")}{type}<{(classArguments == null ? "" : string.Join(",", classArguments))}>.({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			return cache.GetOrAddMethodReference(module, methodKey, x => x.module.ImportMethodReference(cache,
														   x.module.GetTypeDefinition(cache, type),
														   methodName: methodName,
														   predicate: md =>
														   {
															   if (md.IsStatic != isStatic)
																   return false;
															   if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
																   return false;
															   for (var i = 0; i < md.Parameters.Count; i++)
																   if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, x.module.ImportReference(cache, parameterTypes[i])))
																	   return false;
															   return true;
														   },
														   classArguments: classArguments?.Select(gp => x.module.GetTypeDefinition(cache, (gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray()));
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module,
													XamlCache cache,
													(string assemblyName, string clrNamespace, string typeName) type,
													string methodName,
													int paramCount,
													(string assemblyName, string clrNamespace, string typeName)[] classArguments = null,
													bool isStatic = false)
		{
			var methodKey = $"{(isStatic ? "static " : "")}{type}<{(classArguments == null ? "" : string.Join(",", classArguments))}>.({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			return cache.GetOrAddMethodReference(module, methodKey, x => x.module.ImportMethodReference(cache,
														   x.module.GetTypeDefinition(cache, type),
														   methodName: methodName,
														   predicate: md =>
														   {
															   if (md.IsStatic != isStatic)
																   return false;
															   if (md.Parameters.Count != paramCount)
																   return false;
															   return true;
														   },
														   classArguments: classArguments?.Select(gp => x.module.GetTypeDefinition(cache, (gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray()));
		}

		static FieldReference ImportFieldReference(this ModuleDefinition module, XamlCache cache, TypeReference type, string fieldName, Func<FieldDefinition, bool> predicate = null, bool caseSensitive = true)
		{
			var field = module
				.ImportReference(type)
				.ResolveCached(cache)
				.Fields
				.FirstOrDefault(fd =>
								   string.Equals(fd.Name, fieldName, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
								&& (predicate?.Invoke(fd) ?? true));
			return field == null ? null : module.ImportReference(field);
		}

		public static FieldReference ImportFieldReference(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type, string fieldName, bool isStatic = false, bool caseSensitive = true)
		{
			var fieldKey = $"{(isStatic ? "static " : "")}{type}.{(caseSensitive ? fieldName : fieldName.ToLowerInvariant())}";
			return cache.GetOrAddFieldReference((module, fieldKey), _ => module.ImportFieldReference(cache, module.GetTypeDefinition(cache, type), fieldName: fieldName, predicate: fd => fd.IsStatic == isStatic, caseSensitive: caseSensitive));
		}

		public static TypeDefinition GetTypeDefinition(this ModuleDefinition module, XamlCache cache, (string assemblyName, string clrNamespace, string typeName) type)
		{
			return cache.GetOrAddTypeDefinition(module, type, x =>
			{
				var assemblyName = module.Assembly.Name;
				var asm = (assemblyName.Name == type.assemblyName || assemblyName.FullName == type.assemblyName)
								? module.Assembly
								: module.AssemblyResolver.Resolve(AssemblyNameReference.Parse(type.assemblyName));
				var typeDef = asm.MainModule.GetType($"{type.clrNamespace}.{type.typeName}");
				if (typeDef != null)
				{
					return typeDef;
				}
				var exportedType = asm.MainModule.ExportedTypes.FirstOrDefault(
					arg => arg.IsForwarder && arg.Namespace == type.clrNamespace && arg.Name == type.typeName);
				if (exportedType != null)
				{
					typeDef = exportedType.Resolve();
					return typeDef;
				}

				//I hate you, netstandard
				if (type.assemblyName == "mscorlib" && type.clrNamespace == "System.Reflection")
					return module.GetTypeDefinition(cache, ("System.Reflection", type.clrNamespace, type.typeName));

				return null;
			});
		}

		static IEnumerable<PropertyDefinition> Properties(this TypeDefinition typedef, XamlCache cache, bool flatten)
		{
			foreach (var property in typedef.Properties)
				yield return property;
			if (!flatten || typedef.BaseType == null)
				yield break;
			foreach (var property in typedef.BaseType.ResolveCached(cache).Properties(cache, flatten: true))
				yield return property;
		}

		static string SerializeTypeReference(TypeReference tr)
		{
			var serialized = $"{tr.Scope.Name},{tr.Namespace},{tr.Name}";
			var gitr = tr as GenericInstanceType;
			return gitr == null ? serialized : $"{serialized}<{string.Join(",", gitr.GenericArguments.Select(SerializeTypeReference))}>";
		}

		public static bool IsVisibleInternal(this ModuleDefinition from, ModuleDefinition to) =>
			from.GetCustomAttributes().Any(ca =>
				ca.AttributeType.FullName == "System.Runtime.CompilerServices.InternalsVisibleToAttribute" &&
				ca.HasConstructorArguments &&
				ca.ConstructorArguments[0].Value is string visibleTo &&
				visibleTo.StartsWith(to.Assembly.Name.Name, StringComparison.InvariantCulture));
	}
}