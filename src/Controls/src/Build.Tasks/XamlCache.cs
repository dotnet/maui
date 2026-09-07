using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.XamlC;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Microsoft.Maui.Controls.Build.Tasks;

/// <summary>
/// Class for caching various Mono.Cecil objects
/// </summary>
class XamlCache
{
	readonly Dictionary<ModuleDefinition, IList<XmlnsDefinitionAttribute>> _xmlnsDefinitions = new();
	readonly Dictionary<TypeReference, TypeDefinition> _resolvedTypes = new();
	readonly Dictionary<(ModuleDefinition module, string fieldRefKey), FieldReference> _fieldReferenceCache = new();
	readonly Dictionary<(ModuleDefinition module, string typeKey), TypeReference> _typeReferenceCache = new();
	readonly Dictionary<(ModuleDefinition module, string methodRefKey), MethodReference> _methodReferenceCache = new();
	readonly Dictionary<(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName)), TypeDefinition> _typeDefinitionCache = new();
	readonly Dictionary<VariableDefinition, ICollection<string>> _resourceNamesInUse = new();
	Dictionary<TypeReference, Type> _knownCompiledTypeConverters;

	static TValue GetOrAdd<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
	{
		if (!dictionary.TryGetValue(key, out TValue value))
		{
			value = dictionary[key] = valueFactory(key);
		}
		return value;
	}

	public IList<XmlnsDefinitionAttribute> GetXmlnsDefinitions(ModuleDefinition module, Func<ModuleDefinition, IList<XmlnsDefinitionAttribute>> valueFactory) =>
		GetOrAdd(_xmlnsDefinitions, module, valueFactory);

	public TypeDefinition Resolve(TypeReference typeReference) =>
		GetOrAdd(_resolvedTypes, typeReference, t => t.Resolve());

	public FieldReference GetOrAddFieldReference((ModuleDefinition module, string fieldRefKey) key, Func<(ModuleDefinition module, string fieldRefKey), FieldReference> valueFactory) =>
		GetOrAdd(_fieldReferenceCache, key, valueFactory);

	public TypeReference GetOrAddTypeReference(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type) => GetOrAdd(_typeReferenceCache, (module, type.ToString()), x =>
	{
		if (type.typeName.EndsWith("[]", StringComparison.InvariantCultureIgnoreCase))
			return x.module.GetTypeDefinition(this, (type.assemblyName, type.clrNamespace, type.typeName.Substring(0, type.typeName.Length - 2))).MakeArrayType();
		else
			return x.module.ImportReference(x.module.GetTypeDefinition(this, type));
	});

	public TypeReference GetOrAddTypeReference(ModuleDefinition module, string typeKey, Func<(ModuleDefinition module, string typeKey), TypeReference> valueFactory) =>
		GetOrAdd(_typeReferenceCache, (module, typeKey), valueFactory);

	public MethodReference GetOrAddMethodReference(ModuleDefinition module, string methodRefKey, Func<(ModuleDefinition module, string methodRefKey), MethodReference> valueFactory) =>
		GetOrAdd(_methodReferenceCache, (module, methodRefKey), valueFactory);

	public TypeDefinition GetOrAddTypeDefinition(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName) type, Func<(ModuleDefinition module, (string assemblyName, string clrNamespace, string typeName)), TypeDefinition> valueFactory) =>
		GetOrAdd(_typeDefinitionCache, (module, type), valueFactory);

	public ICollection<string> GetResourceNamesInUse(VariableDefinition variableDefinition) =>
		GetOrAdd(_resourceNamesInUse, variableDefinition, _ => new HashSet<string>(StringComparer.Ordinal));

	public Dictionary<TypeReference, Type> GetKnownCompiledTypeConverters(ModuleDefinition module) => _knownCompiledTypeConverters ??= new(TypeRefComparer.Default)
	{
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "ThicknessTypeConverter")), typeof(ThicknessTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "CornerRadiusTypeConverter")), typeof(CornerRadiusTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "EasingTypeConverter")), typeof(EasingTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics.Converters", "ColorTypeConverter")), typeof(ColorTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics.Converters", "PointTypeConverter")), typeof(PointTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics.Converters", "RectTypeConverter")), typeof(RectangleTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexJustifyTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexJustify>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexDirectionTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexDirection>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexAlignContentTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexAlignContent>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexAlignItemsTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexAlignItems>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexAlignSelfTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexAlignSelf>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexWrapTypeConverter")), typeof(EnumTypeConverter<Layouts.FlexWrap>) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "FlexBasisTypeConverter")), typeof(FlexBasisTypeConverter) },
		{ module.ImportReference(this, ("Microsoft.Maui", "Microsoft.Maui.Converters", "GridLengthTypeConverter")), typeof(Microsoft.Maui.Controls.XamlC.GridLengthTypeConverter) },

	};

	// State used by SetPropertiesVisitor
	public int DataTemplateCount { get; set; }
	public int TypedBindingCount { get; set; }
}
