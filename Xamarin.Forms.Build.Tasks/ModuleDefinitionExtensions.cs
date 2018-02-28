using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	static class ModuleDefinitionExtensions
	{
		public static TypeReference ImportReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, (string assemblyName, string clrNamespace, string typeName)[] classArguments = null)
		{
			var typeRef = module.ImportReference(module.GetTypeDefinition(type));
			if (classArguments is null)
				return typeRef;
			return module.ImportReference(typeRef.MakeGenericInstanceType(classArguments.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray()));
		}

		public static TypeReference ImportArrayReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type)
		{
			var typeRef = module.ImportReference(type);
			if (typeRef is null)
				return null;
			return module.ImportReference(typeRef.MakeArrayType());
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, TypeReference type, int paramCount, TypeReference[] classArguments = null, Func<MethodDefinition, bool> predicate = null)
		{
			var ctor = module
				.ImportReference(type)
				.ResolveCached()
				.Methods
				.FirstOrDefault(md =>
								   md.IsConstructor
								&& md.Parameters.Count == paramCount
								&& (predicate?.Invoke(md) ?? true));
			if (ctor is null)
				return null;
			var ctorRef = module.ImportReference(ctor);
			if (classArguments == null)
				return ctorRef;
			return module.ImportReference(ctorRef.ResolveGenericParameters(type.MakeGenericInstanceType(classArguments), module));
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, (string assemblyName, string clrNamespace, string typeName)[] classArguments, Func<MethodDefinition, bool> predicate = null)
		{
			return module.ImportCtorReference(module.GetTypeDefinition(type), paramCount, classArguments?.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray(), predicate);
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, TypeReference[] classArguments, Func<MethodDefinition, bool> predicate = null)
		{
			return module.ImportCtorReference(module.GetTypeDefinition(type), paramCount, classArguments, predicate);
		}

		public static MethodReference ImportCtorReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, int paramCount, Func<MethodDefinition, bool> predicate = null)
		{
			return module.ImportCtorReference(module.GetTypeDefinition(type), paramCount, null, predicate);
		}

		public static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null, bool flatten = false)
		{
			var properties = module.ImportReference(type).Resolve().Properties;
			var getter = module
				.ImportReference(type)
				.ResolveCached()
				.Properties(flatten)
				.FirstOrDefault(pd =>
								   pd.Name == propertyName
								&& (predicate?.Invoke(pd) ?? true))
				?.GetMethod;
			return getter == null ? null : module.ImportReference(getter);
		}

		public static MethodReference ImportPropertyGetterReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, Func<PropertyDefinition, bool> predicate = null, bool flatten = false)
		{
			return module.ImportPropertyGetterReference(module.GetTypeDefinition(type), propertyName, predicate, flatten);
		}

		public static MethodReference ImportPropertySetterReference(this ModuleDefinition module, TypeReference type, string propertyName, Func<PropertyDefinition, bool> predicate = null)
		{
			var setter = module
				.ImportReference(type)
				.ResolveCached()
				.Properties
				.FirstOrDefault(pd =>
								   pd.Name == propertyName
								&& (predicate?.Invoke(pd) ?? true))
				?.SetMethod;
			return setter == null ? null : module.ImportReference(setter);
		}

		public static MethodReference ImportPropertySetterReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string propertyName, Func<PropertyDefinition, bool> predicate = null)
		{
			return module.ImportPropertySetterReference(module.GetTypeDefinition(type), propertyName, predicate);
		}

		public static FieldReference ImportFieldReference(this ModuleDefinition module, TypeReference type, string fieldName, Func<FieldDefinition, bool> predicate = null)
		{
			var field = module
				.ImportReference(type)
				.ResolveCached()
				.Fields
				.FirstOrDefault(fd =>
								   fd.Name == fieldName
								&& (predicate?.Invoke(fd) ?? true));
			return field == null ? null : module.ImportReference(field);
		}

		public static FieldReference ImportFieldReference(this ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, string fieldName, Func<FieldDefinition, bool> predicate = null)
		{
			return module.ImportFieldReference(module.GetTypeDefinition(type), fieldName: fieldName, predicate: predicate);
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module, TypeReference type, string methodName, int paramCount, Func<MethodDefinition, bool> predicate = null, TypeReference[] classArguments = null)
		{
			var method = module
				.ImportReference(type)
				.ResolveCached()
				.Methods
				.FirstOrDefault(md =>
								   !md.IsConstructor
								&& md.Name == methodName
								&& md.Parameters.Count == paramCount
								&& (predicate?.Invoke(md) ?? true));
			if (method is null)
				return null;
			var methodRef = module.ImportReference(method);
			if (classArguments == null)
				return methodRef;
			return module.ImportReference(methodRef.ResolveGenericParameters(type.MakeGenericInstanceType(classArguments), module));
		}

		public static MethodReference ImportMethodReference(this ModuleDefinition module,
															(string assemblyName, string clrNamespace, string typeName) type,
															string methodName, int paramCount, Func<MethodDefinition, bool> predicate = null,
															(string assemblyName, string clrNamespace, string typeName)[] classArguments = null)
		{
			return module.ImportMethodReference(module.GetTypeDefinition(type), methodName, paramCount, predicate,
												classArguments?.Select(gp => module.GetTypeDefinition((gp.assemblyName, gp.clrNamespace, gp.typeName))).ToArray());
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
			if (typeDef != null) {
				typeDefCache.Add((module, type), typeDef);
				return typeDef;
			}
			var exportedType = asm.MainModule.ExportedTypes.FirstOrDefault(
				arg => arg.IsForwarder && arg.Namespace == type.clrNamespace && arg.Name == type.typeName);
			if (exportedType != null) {
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
	}
}