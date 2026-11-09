using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class InferOnIdiomTypeArgumentsVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	static readonly XmlName DefaultProperty = new("", "Default");
	static readonly HashSet<string> ValueProperties =
	[
		"Default",
		"Phone",
		"Tablet",
		"Desktop",
		"TV",
		"Watch",
	];

	SourceGenContext Context => context;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		if (parentNode is not ElementNode parentElement
			|| !node.TryGetPropertyName(parentElement, out var propertyName)
			|| !IsOnIdiomExtension(node)
			|| !TryGetTargetType(parentElement, propertyName, out var targetType, out var targetConverter)
			|| !CanUseGenericOnIdiom(node, targetType, targetConverter))
		{
			return;
		}

		var onIdiomType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.OnIdiom`1");
		if (onIdiomType is null)
			return;

		// The constructed symbol is authoritative; the synthetic argument only gives the cache key stable identity.
		var typeArgument = new XmlType(
			"urn:microsoft-maui-sourcegen",
			$"{targetType.ToFQDisplayString()},{targetType.ContainingAssembly?.Identity}",
			null);
		var genericXmlType = new XmlType(XamlParser.MauiUri, "OnIdiom", [typeArgument]);
		Context.TypeCache[genericXmlType] = onIdiomType.Construct(targetType);

		var replacement = new ElementNode(
			genericXmlType,
			node.NamespaceURI,
			node.NamespaceResolver,
			node.LineNumber,
			node.LinePosition)
		{
			Parent = parentElement,
		};

		foreach (var property in node.Properties)
		{
			replacement.Properties[property.Key] = property.Value;
			property.Value.Parent = replacement;
		}

		if (node.CollectionItems.Count == 1)
		{
			var defaultValue = node.CollectionItems[0];
			replacement.Properties[DefaultProperty] = defaultValue;
			defaultValue.Parent = replacement;
		}

		parentElement.Properties[propertyName] = replacement;
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}

	bool IsOnIdiomExtension(ElementNode node)
	{
		if (!node.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out var type))
			return false;

		return SymbolEqualityComparer.Default.Equals(
			type,
			Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.OnIdiomExtension"));
	}

	bool TryGetTargetType(ElementNode parent, XmlName propertyName, out ITypeSymbol targetType, out ITypeSymbol? targetConverter)
	{
		targetType = null!;
		targetConverter = null;

		if (parent.XmlType.IsOfAnyType("Setter")
			&& propertyName.LocalName == "Value"
			&& parent.Properties.TryGetValue("Property", out var propertyNode)
			&& propertyNode is ValueNode bindablePropertyNode)
		{
			var bindableProperty = bindablePropertyNode.GetBindableProperty(Context);
			if (bindableProperty?.GetBPTypeAndConverter(Context) is { } setterPropertyInfo)
			{
				targetType = setterPropertyInfo.type;
				targetConverter = setterPropertyInfo.converter;
				return true;
			}

			return false;
		}

		if (!parent.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out var parentType)
			|| parentType is null)
		{
			return false;
		}

		var property = parentType.GetAllProperties(propertyName.LocalName, Context)
			.FirstOrDefault(property => Context.Compilation.IsSymbolAccessibleWithin(property, Context.RootType));
		if (property is not null)
		{
			targetType = property.Type;
			targetConverter = property.GetAttributes()
				.Concat(property.Type.GetAttributes())
				.FirstOrDefault(attribute => attribute.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")
				?.ConstructorArguments[0].Value as ITypeSymbol;
			return true;
		}

		var localName = propertyName.LocalName;
		var bindablePropertyField = parentType.GetBindableProperty(propertyName.NamespaceURI, ref localName, out _, Context, null);
		if (bindablePropertyField is not null
			&& Context.Compilation.IsSymbolAccessibleWithin(bindablePropertyField, Context.RootType)
			&& bindablePropertyField.GetBPTypeAndConverter(Context) is { } bindablePropertyInfo)
		{
			targetType = bindablePropertyInfo.type;
			targetConverter = bindablePropertyInfo.converter;
			return true;
		}

		return false;
	}

	bool CanUseGenericOnIdiom(ElementNode node, ITypeSymbol targetType, ITypeSymbol? targetConverter)
	{
		// A standard conversion to object would assign the wrapper instead of invoking OnIdiom<T>'s operator.
		if (!IsSupportedTargetType(targetType))
			return false;

		var hasDefaultProperty = node.Properties.Keys.Any(name => name.LocalName == DefaultProperty.LocalName);

		if (node.CollectionItems.Count > 1
			|| node.CollectionItems.Count == 1 && hasDefaultProperty
			// Without Default, OnIdiomExtension can return a BindableProperty's default value, unlike OnIdiom<T>.
			|| node.CollectionItems.Count == 0 && !hasDefaultProperty)
		{
			return false;
		}

		foreach (var property in node.Properties)
		{
			if (!ValueProperties.Contains(property.Key.LocalName)
				|| property.Value is not ValueNode)
			{
				return false;
			}
		}

		if (node.CollectionItems.Count == 1 && node.CollectionItems[0] is not ValueNode)
			return false;

		var converter = GetTypeConverter(targetType);
		if (targetConverter is not null
			&& !SymbolEqualityComparer.Default.Equals(targetConverter, converter)
			&& !CanIgnoreTargetConverter(node, targetType, targetConverter))
		{
			return false;
		}

		foreach (var property in node.Properties)
		{
			if (!((ValueNode)property.Value).CanConvertTo(targetType, converter, Context))
			{
				return false;
			}
		}

		return node.CollectionItems.Count == 0
			|| node.CollectionItems[0] is ValueNode defaultValue
				&& defaultValue.CanConvertTo(targetType, converter, Context);
	}

	static ITypeSymbol? GetTypeConverter(ITypeSymbol type)
		=> type.GetAttributes()
			.FirstOrDefault(attribute => attribute.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")
			?.ConstructorArguments[0].Value as ITypeSymbol;

	bool IsSupportedTargetType(ITypeSymbol type)
	{
		switch (type.SpecialType)
		{
			case SpecialType.System_Boolean:
			case SpecialType.System_Byte:
			case SpecialType.System_Char:
			case SpecialType.System_DateTime:
			case SpecialType.System_Decimal:
			case SpecialType.System_Double:
			case SpecialType.System_Int16:
			case SpecialType.System_Int32:
			case SpecialType.System_Int64:
			case SpecialType.System_SByte:
			case SpecialType.System_Single:
			case SpecialType.System_String:
			case SpecialType.System_UInt16:
			case SpecialType.System_UInt32:
			case SpecialType.System_UInt64:
				return true;
		}

		if (type.Equals(Context.Compilation.GetTypeByMetadataName("System.TimeSpan"), SymbolEqualityComparer.Default)
			|| type.Equals(Context.Compilation.GetTypeByMetadataName("System.Uri"), SymbolEqualityComparer.Default))
		{
			return true;
		}

		if (type.ContainingAssembly.Name is not ("Microsoft.Maui" or "Microsoft.Maui.Controls" or "Microsoft.Maui.Graphics"))
			return false;

		if (type.TypeKind == TypeKind.Enum)
			return true;

		var converter = GetTypeConverter(type);
		return converter is not null && NodeSGExtensions.GetKnownSGTypeConverters(Context).ContainsKey(converter);
	}

	bool CanIgnoreTargetConverter(ElementNode node, ITypeSymbol targetType, ITypeSymbol targetConverter)
	{
		if (targetType.SpecialType != SpecialType.System_Double
			|| !targetConverter.Equals(
				Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.FontSizeConverter"),
				SymbolEqualityComparer.Default))
		{
			return false;
		}

		return node.Properties.Values
				.Concat(node.CollectionItems)
				.Cast<ValueNode>()
				.All(valueNode =>
				valueNode.Value is string value
					&& double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _));
	}
}
