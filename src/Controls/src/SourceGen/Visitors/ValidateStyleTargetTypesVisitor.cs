using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

/// <summary>
/// Reports elements whose <c>Style</c> is set to a <c>{StaticResource}</c> style declared in the same file
/// with a TargetType the element is not an instance of. Such styles are never applied at runtime.
/// </summary>
class ValidateStyleTargetTypesVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	static readonly XmlName KeyName = new("", "Key");

	SourceGenContext Context => context;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool StopOnStyle => false;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => false;
	public bool IsStyle(ElementNode node) => false;

	public void Visit(ValueNode node, INode parentNode)
	{
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		foreach (var property in node.Properties)
		{
			if (property.Key.LocalName != "Style")
				continue;

			var value = property.Value is ListNode { CollectionItems.Count: 1 } list ? list.CollectionItems[0] : property.Value;
			if (value is ElementNode staticResource && IsStaticResourceExtension(staticResource))
				Validate(node, staticResource);
		}
	}

	public void Visit(RootNode node, INode parentNode) => Visit((ElementNode)node, parentNode);

	public void Visit(ListNode node, INode parentNode)
	{
	}

	void Validate(ElementNode element, ElementNode staticResource)
	{
		// Generic elements are skipped because their constructed type is not resolved here.
		if (element.XmlType.TypeArguments is { Count: > 0 }
			|| !element.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out var elementType)
			|| elementType is null
			|| !HasStyleProperty(elementType))
			return;

		if (GetKey(staticResource) is not string key)
			return;

		var resource = KnownMarkups.GetResourceNodeIncludingLazy(staticResource, Context, key);
		if (resource is null || !resource.IsStyle(Context))
			return;

		var targetType = SetPropertyHelpers.GetStyleTargetType(resource, Context, reportDiagnostics: false);
		if (targetType is null
			|| elementType.InheritsFrom(targetType, Context)
			|| (targetType.IsInterface() && elementType.Implements(targetType)))
			return;

		Context.ReportDiagnostic(Diagnostic.Create(
			Descriptors.StyleTargetTypeMismatch,
			LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)staticResource, key),
			key,
			GetDisplayName(targetType),
			GetDisplayName(elementType)));
	}

	// Matches Type.FullName, as used by the runtime warning for the same mismatch.
	static string GetDisplayName(INamedTypeSymbol type)
		=> type.IsGenericType ? type.ToFQDisplayString() : type.GetMetadataFullName();

	bool IsStaticResourceExtension(ElementNode node)
		=> node.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, Context.TypeCache, out var type)
			&& SymbolEqualityComparer.Default.Equals(type, Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StaticResourceExtension"));

	bool HasStyleProperty(ITypeSymbol elementType)
	{
		var styleType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Style");
		return elementType.GetAllProperties("Style", Context).Any(p => SymbolEqualityComparer.Default.Equals(p.Type, styleType));
	}

	static string? GetKey(ElementNode staticResource)
	{
		if (!staticResource.Properties.TryGetValue(KeyName, out var keyNode) && staticResource.CollectionItems.Count == 1)
			keyNode = staticResource.CollectionItems[0];
		return (keyNode as ValueNode)?.Value as string;
	}
}
