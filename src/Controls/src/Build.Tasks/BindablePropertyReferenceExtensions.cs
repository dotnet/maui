using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	static class BindablePropertyReferenceExtensions
	{
		public static TypeReference GetBindablePropertyType(this FieldReference bpRef, XamlCache cache, IXmlLineInfo iXmlLineInfo, ModuleDefinition module)
		{
			if (!bpRef.Name.EndsWith("Property", StringComparison.InvariantCulture))
				throw new BuildException(BuildExceptionCode.BPName, iXmlLineInfo, null, bpRef.Name);
			var bpName = bpRef.Name.Substring(0, bpRef.Name.Length - 8);
			var owner = bpRef.DeclaringType;

			var getter = owner.GetProperty(cache, pd => pd.Name == bpName, out TypeReference declaringTypeRef)?.GetMethod;
			if (getter == null || getter.IsStatic || !getter.IsPublic)
				getter = null;
			getter = getter ?? owner.GetMethods(cache, md => md.Name == $"Get{bpName}" &&
												md.IsStatic &&
												md.IsPublic &&
												md.Parameters.Count == 1 &&
												md.Parameters[0].ParameterType.InheritsFromOrImplements(cache, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"))), module).FirstOrDefault()?.Item1;
			if (getter == null)
				throw new BuildException(BuildExceptionCode.BPName, iXmlLineInfo, null, bpName, bpRef.DeclaringType);

			return getter.ResolveGenericReturnType(declaringTypeRef, module);
		}

		public static TypeReference GetBindablePropertyTypeConverter(this FieldReference bpRef, XamlCache cache, ModuleDefinition module)
		{
			var owner = bpRef.DeclaringType;
			var bpName = bpRef.Name.EndsWith("Property", StringComparison.Ordinal) ? bpRef.Name.Substring(0, bpRef.Name.Length - 8) : bpRef.Name;
			var property = owner.GetProperty(cache, pd => pd.Name == bpName, out TypeReference propertyDeclaringType);
			var propertyType = property?.PropertyType?.ResolveGenericParameters(propertyDeclaringType);
			var staticGetter = owner.GetMethods(cache, md => md.Name == $"Get{bpName}" &&
												md.IsStatic &&
												md.IsPublic &&
												md.Parameters.Count == 1 &&
												md.Parameters[0].ParameterType.InheritsFromOrImplements(cache, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "BindableObject"))), module).FirstOrDefault()?.Item1;

			var attributes = new List<CustomAttribute>();
			if (property != null && property.HasCustomAttributes)
				attributes.AddRange(property.CustomAttributes);
			if (propertyType != null && propertyType.ResolveCached(cache).HasCustomAttributes)
				attributes.AddRange(propertyType.ResolveCached(cache).CustomAttributes);
			if (staticGetter != null && staticGetter.HasCustomAttributes)
				attributes.AddRange(staticGetter.CustomAttributes);
			if (staticGetter != null && staticGetter.ReturnType.ResolveGenericParameters(bpRef.DeclaringType).ResolveCached(cache).HasCustomAttributes)
				attributes.AddRange(staticGetter.ReturnType.ResolveGenericParameters(bpRef.DeclaringType).ResolveCached(cache).CustomAttributes);

			if (attributes.FirstOrDefault(cad => cad.AttributeType.FullName == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value is TypeReference typeConverter)
				return typeConverter;

			propertyType = propertyType ?? staticGetter?.ReturnType;
			return null;
		}
	}

	static class KVPExtensions
	{
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
		{
			key = kvp.Key;
			value = kvp.Value;
		}
	}
}