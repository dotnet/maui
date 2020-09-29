using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	static class ModuleDefinitionExtensions
	{
		static Dictionary<(ModuleDefinition module, string typeKey), TypeReference> TypeRefCache = new Dictionary<(ModuleDefinition module, string typeKey), TypeReference>();
		public static TypeReference ImportReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type)
		{
			var typeKey = type.ToString();
			if (!TypeRefCache.TryGetValue((module, typeKey), out var typeRef))
				TypeRefCache.Add((module, typeKey), typeRef = module.ImportReference(module.GetTypeDefinition(type)));
			return typeRef;
		}

		public static TypeReference ImportReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var typeKey = $"{type}<{string.Join(",", classArguments)}>";
			if (!TypeRefCache.TryGetValue((module, typeKey), out var typeRef))
				TypeRefCache.Add((module, typeKey), typeRef = module.ImportReference(module.ImportReference(type).MakeGenericInstanceType(classArguments.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray())));
			return typeRef;
		}

		public static TypeReference ImportArrayReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type)
		{
			var typeKey = "${type}[]";
			if (!TypeRefCache.TryGetValue((module, typeKey), out var typeRef))
				TypeRefCache.Add((module, typeKey), typeRef = module.ImportReference(module.ImportReference(type).MakeArrayType()));
			return typeRef;
		}

		static Dictionary<(ModuleDefinition module, string methodRefKey), MethodReference> MethodRefCache = new Dictionary<(ModuleDefinition module, string methodRefKey), MethodReference>();
		static MethodReference ImportCtorReference(this ModuleDefinition module, TypeReference type, TypeReference[] classArguments, Func<MethodDefinition, bool> predicate)
		{
			var ctor = module.ImportReference(type).ResolveCached().Methods.FirstOrDefault(md => !md.IsPrivate && !md.IsStatic && md.IsConstructor && (predicate?.Invoke(md) ?? true));
			if (ctor is null)
				return null;
			var ctorRef = module.ImportReference(ctor);
			if (classArguments == null)
				return ctorRef;
			return module.ImportReference(ctorRef.ResolveGenericParameters(type.MakeGenericInstanceType(classArguments), module));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, TypeReference type, TypeReference[] parameterTypes)
		{
			var ctorKey = $"{type}.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes.Select(SerializeTypeReference)))})";
			if (MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				return ctorRef;
			ctorRef = module.ImportCtorReference(type, classArguments: null, predicate: md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, module.ImportReference(parameterTypes[i])))
						return false;
				return true;
			});
			MethodRefCache.Add((module, ctorKey), ctorRef);
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount)
		{
			var ctorKey = $"{type}.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			if (!MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				MethodRefCache.Add((module, ctorKey), ctorRef = module.ImportCtorReference(module.GetTypeDefinition(type), null, md => md.Parameters.Count == paramCount));
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, TypeReference type, int paramCount)
		{
			var ctorKey = $"{type}.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			if (!MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				MethodRefCache.Add((module, ctorKey), ctorRef = module.ImportCtorReference(type, null, md => md.Parameters.Count == paramCount));
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var ctorKey = $"{type}<{(string.Join(",", classArguments))}>.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			if (!MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				MethodRefCache.Add((module, ctorKey), ctorRef = module.ImportCtorReference(module.GetTypeDefinition(type), classArguments.Select(module.GetTypeDefinition).ToArray(), md => md.Parameters.Count == paramCount));
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, TypeReference[] classArguments)
		{
			var ctorKey = $"{type}<{string.Join(",", classArguments.Select(SerializeTypeReference))}>.ctor({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			if (!MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				MethodRefCache.Add((module, ctorKey), ctorRef = module.ImportCtorReference(module.GetTypeDefinition(type), classArguments, predicate: md => md.Parameters.Count == paramCount));
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] parameterTypes, (string assemblyName, string clrNamespace, string typeName)[] classArguments)
		{
			var ctorKey = $"{type}<{(string.Join(",", classArguments))}>.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			if (MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				return ctorRef;
			ctorRef = module.ImportCtorReference(module.GetTypeDefinition(type), classArguments.Select(module.GetTypeDefinition).ToArray(), md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, module.ImportReference(parameterTypes[i])))
						return false;
				return true;
			});
			MethodRefCache.Add((module, ctorKey), ctorRef);
			return ctorRef;
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] parameterTypes)
		{
			var ctorKey = $"{type}.ctor({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			if (MethodRefCache.TryGetValue((module, ctorKey), out var ctorRef))
				return ctorRef;
			ctorRef = module.ImportCtorReference(module.GetTypeDefinition(type), classArguments: null, predicate: md =>
			{
				if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
					return false;
				for (var i = 0; i < md.Parameters.Count; i++)
					if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, module.ImportReference(parameterTypes[i])))
						return false;
				return true;
			});
			MethodRefCache.Add((module, ctorKey), ctorRef);
			return ctorRef;
		}

		static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null, bool flatten = false, bool caseSensitive = true)
		{
			var properties = module.ImportReference(type).Resolve().Properties;
			var getter = module
				.ImportReference(type)
				.ResolveCached()
				.Properties(flatten)
				.FirstOrDefault(pd =>
								   string.Equals(pd.Name, propertyName, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
								&& !pd.GetMethod.IsPrivate
								&& (predicate?.Invoke(pd) ?? true))
				?.GetMethod;
			return getter == null ? null : module.ImportReference(getter);
		}

		public static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, bool isStatic = false, bool flatten = false, bool caseSensitive = true)
		{
			var getterKey = $"{(isStatic ? "static " : "")}{type}.get_{propertyName}{(flatten ? "*" : "")}";
			if (!MethodRefCache.TryGetValue((module, getterKey), out var methodReference))
				MethodRefCache.Add((module, getterKey), methodReference = module.ImportPropertyGetterReference(module.GetTypeDefinition(type), propertyName, pd => pd.GetMethod.IsStatic == isStatic, flatten, caseSensitive: caseSensitive));
			return methodReference;
		}

		static MethodReference ImportPropertySetterReference(this ModuleDefinition module, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null)
		{
			var setter = module
				.ImportReference(type)
				.ResolveCached()
				.Properties
				.FirstOrDefault(pd =>
								   pd.Name == propertyName
								&& !pd.SetMethod.IsPrivate
								&& (predicate?.Invoke(pd) ?? true))
				?.SetMethod;
			return setter == null ? null : module.ImportReference(setter);
		}

		public static MethodReference ImportPropertySetterReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, bool isStatic = false)
		{
			var setterKey = $"{(isStatic ? "static " : "")}{type}.set{propertyName}";
			if (!MethodRefCache.TryGetValue((module, setterKey), out var methodReference))
				MethodRefCache.Add((module, setterKey), methodReference = module.ImportPropertySetterReference(module.GetTypeDefinition(type), propertyName, pd => pd.SetMethod.IsStatic == isStatic));
			return methodReference;
		}

		static MethodReference ImportMethodReference(this ModuleDefinition module, TypeReference type, string methodName, Func<MethodDefinition, bool> predicate = null, TypeReference[] classArguments = null)
		{
			var method = module
				.ImportReference(type)
				.ResolveCached()
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
													 TypeReference type,
													 string methodName,
													 TypeReference[] parameterTypes = null,
													 TypeReference[] classArguments = null,
													 bool isStatic = false)
		{
			return module.ImportMethodReference(type,
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
															(string assemblyName, string clrNamespace, string typeName) type,
															string methodName,
															(string assemblyName, string clrNamespace, string typeName)[] parameterTypes,
															(string assemblyName, string clrNamespace, string typeName)[] classArguments = null,
															bool isStatic = false)
		{
			var methodKey = $"{(isStatic ? "static " : "")}{type}<{(classArguments == null ? "" : string.Join(",", classArguments))}>.({(parameterTypes == null ? "" : string.Join(",", parameterTypes))})";
			if (MethodRefCache.TryGetValue((module, methodKey), out var methodReference))
				return methodReference;
			methodReference = module.ImportMethodReference(module.GetTypeDefinition(type),
														   methodName: methodName,
														   predicate: md =>
														   {
															   if (md.IsStatic != isStatic)
																   return false;
															   if (md.Parameters.Count != (parameterTypes?.Length ?? 0))
																   return false;
															   for (var i = 0; i < md.Parameters.Count; i++)
																   if (!TypeRefComparer.Default.Equals(md.Parameters[i].ParameterType, module.ImportReference(parameterTypes[i])))
																	   return false;
															   return true;
														   },
														   classArguments: classArguments?.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray());
			MethodRefCache.Add((module, methodKey), methodReference);
			return methodReference;
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module,
													(string assemblyName, string clrNamespace, string typeName) type,
													string methodName,
													int paramCount,
													(string assemblyName, string clrNamespace, string typeName)[] classArguments = null,
													bool isStatic = false)
		{
			var methodKey = $"{(isStatic ? "static " : "")}{type}<{(classArguments == null ? "" : string.Join(",", classArguments))}>.({(string.Join(",", Enumerable.Repeat("_", paramCount)))})";
			if (MethodRefCache.TryGetValue((module, methodKey), out var methodReference))
				return methodReference;
			methodReference = module.ImportMethodReference(module.GetTypeDefinition(type),
														   methodName: methodName,
														   predicate: md =>
														   {
															   if (md.IsStatic != isStatic)
																   return false;
															   if (md.Parameters.Count != paramCount)
																   return false;
															   return true;
														   },
														   classArguments: classArguments?.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray());
			MethodRefCache.Add((module, methodKey), methodReference);
			return methodReference;
		}

		static Dictionary<(ModuleDefinition module, string fieldRefKey), FieldReference> FieldRefCache = new Dictionary<(ModuleDefinition module, string fieldRefKey), FieldReference>();
		static FieldReference ImportFieldReference(this ModuleDefinition module, TypeReference type, string fieldName, Func<FieldDefinition, bool> predicate = null, bool caseSensitive = true)
		{
			var field = module
				.ImportReference(type)
				.ResolveCached()
				.Fields
				.FirstOrDefault(fd =>
								   string.Equals(fd.Name, fieldName, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
								&& (predicate?.Invoke(fd) ?? true));
			return field == null ? null : module.ImportReference(field);
		}

		public static FieldReference ImportFieldReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string fieldName, bool isStatic = false, bool caseSensitive = true)
		{
			var fieldKey = $"{(isStatic ? "static " : "")}{type}.{(caseSensitive ? fieldName : fieldName.ToLowerInvariant())}";
			if (!FieldRefCache.TryGetValue((module, fieldKey), out var fieldReference))
				FieldRefCache.Add((module, fieldKey), fieldReference = module.ImportFieldReference(module.GetTypeDefinition(type), fieldName: fieldName, predicate: fd => fd.IsStatic == isStatic, caseSensitive: caseSensitive));
			return fieldReference;
		}

		static Dictionary<(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName)), TypeDefinition> typeDefCache
			= new Dictionary<(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName)), TypeDefinition>();

		public static TypeDefinition GetTypeDefinition(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type)
		{
			if (typeDefCache.TryGetValue((module, type), out TypeDefinition cachedTypeDefinition))
				return cachedTypeDefinition;

			var asm = module.Assembly.Name.Name == type.assemblyName
							? module.Assembly
							: module.AssemblyResolver.Resolve(AssemblyNameReference.Parse(type.assemblyName));
			var typeDef = asm.MainModule.GetType($"{type.clrNamespace}.{type.typeName}");
			if (typeDef != null)
			{
				typeDefCache.Add((module, type), typeDef);
				return typeDef;
			}
			var exportedType = asm.MainModule.ExportedTypes.FirstOrDefault(
				arg => arg.IsForwarder && arg.Namespace == type.clrNamespace && arg.Name == type.typeName);
			if (exportedType != null)
			{
				typeDef = exportedType.Resolve();
				typeDefCache.Add((module, type), typeDef);
				return typeDef;
			}

			//I hate you, netstandard
			if (type.assemblyName == "mscorlib" && type.clrNamespace == "System.Reflection")
				return module.GetTypeDefinition(("System.Reflection", type.clrNamespace, type.typeName));
			return null;
		}

		static IEnumerable<PropertyDefinition> Properties(this TypeDefinition typedef, bool flatten)
		{
			foreach (var property in typedef.Properties)
				yield return property;
			if (!flatten || typedef.BaseType == null)
				yield break;
			foreach (var property in typedef.BaseType.ResolveCached().Properties(true))
				yield return property;
		}

		static string SerializeTypeReference(TypeReference tr)
		{
			var serialized = $"{tr.Scope.Name},{tr.Namespace},{tr.Name}";
			var gitr = tr as GenericInstanceType;
			return gitr == null ? serialized : $"{serialized}<{string.Join(",", gitr.GenericArguments.Select(SerializeTypeReference))}>";
		}
	}
}