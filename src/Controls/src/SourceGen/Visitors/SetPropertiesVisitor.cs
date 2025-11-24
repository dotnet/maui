using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

class SetPropertiesVisitor(SourceGenContext context, bool stopOnResourceDictionary = false) : IXamlNodeVisitor
{
	SourceGenContext Context => context;
	IndentedTextWriter Writer => Context.Writer;

	public static readonly IList<XmlName> skips = [
	   XmlName.xArguments,
		XmlName.xClass,
		XmlName.xClassModifier,
		XmlName.xDataType,
		XmlName.xFactoryMethod,
		XmlName.xFieldModifier,
		XmlName.xKey,
		XmlName.xName,
		XmlName.xTypeArguments,
	];
	public bool StopOnResourceDictionary => stopOnResourceDictionary;
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => true;
	public bool VisitNodeOnDataTemplate => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	// Track properties that have been set to detect duplicates
	readonly Dictionary<ElementNode, HashSet<XmlName>> setProperties = new Dictionary<ElementNode, HashSet<XmlName>>();

	void CheckForDuplicateProperty(ElementNode parentNode, XmlName propertyName, IXmlLineInfo lineInfo)
	{
		if (!setProperties.TryGetValue(parentNode, out var props))
		{
			props = new HashSet<XmlName>();
			setProperties[parentNode] = props;
		}

		if (!props.Add(propertyName))
		{
			// Property is being set multiple times - check if this warning should be suppressed via NoWarn
			var noWarn = Context.ProjectItem.NoWarn;
			bool shouldSuppress = !string.IsNullOrEmpty(noWarn) &&
				noWarn.Split(new[] { ',', ';', ' ' })
					.Select(code => code.Trim())
					.Any(code => code == "2015" || code == "MAUIX2015");

			if (!shouldSuppress)
			{
				var propertyDisplayName = $"{parentNode.XmlType.Name}.{propertyName.LocalName}";
				var location = LocationCreate(Context.ProjectItem.RelativePath!, lineInfo, propertyDisplayName);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.MultipleChildrenInContentProperty, location, propertyDisplayName));
			}
		}
	}

	public void Visit(ValueNode node, INode parentNode)
	{
		//TODO support Label text as element

		// if it's implicit content property, get the content property name
		if (!node.TryGetPropertyName(parentNode, out XmlName propertyName))
		{
			if (!parentNode.IsCollectionItem(node))
				return;
			string? contentProperty;
			if (!Context.Variables.ContainsKey((ElementNode)parentNode))
				return;
			var parentVar = Context.Variables[(ElementNode)parentNode];
			if ((contentProperty = parentVar.Type.GetContentPropertyName(context)) != null)
				propertyName = new XmlName(((ElementNode)parentNode).NamespaceURI, contentProperty);
			else
				return;
		}

		if (TrySetRuntimeName(propertyName, Context.Variables[(ElementNode)parentNode], node))
			return;
		if (skips.Contains(propertyName))
			return;
		if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
			return;
		if (propertyName.Equals(XmlName.mcIgnorable))
			return;

		SetPropertyHelpers.SetPropertyValue(Writer, Context.Variables[(ElementNode)parentNode], propertyName, node, Context);
	}

	bool TrySetRuntimeName(XmlName propertyName, ILocalValue localVariable, ValueNode node)
	{
		if (propertyName != XmlName.xName)
			return false;
		var runtimeNameAttr = localVariable.Type.GetAllAttributes(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RuntimeNamePropertyAttribute")!, Context).FirstOrDefault();
		if (runtimeNameAttr == null)
			return false;
		var name = runtimeNameAttr.ConstructorArguments[0].Value as string;
		if (string.IsNullOrEmpty(name))
			return false;
		Writer.WriteLine($"{localVariable.ValueAccessor}.{name} = \"{node.Value}\";");
		return true;
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		NodeSGExtensions.GetNodeValueDelegate getNodeValue = (node, type) => context.Variables[node];
		XmlName propertyName = XmlName.Empty;

		//Simplify ListNodes with single elements
		//TODO: this should be done with a transform
		if (parentNode is ListNode pList && pList.CollectionItems.Count == 1)
		{
			propertyName = pList.XmlName;
			parentNode = parentNode.Parent;
		}

		if ((propertyName != XmlName.Empty || node.TryGetPropertyName(parentNode, out propertyName))
		&& skips.Contains(propertyName) || parentNode is ElementNode epn && epn.SkipProperties.Contains(propertyName))
			return;

		if (propertyName == XmlName._CreateContent)
		{
			var variable = Context.Variables[parentNode];
			Writer.WriteLine($"{variable.ValueAccessor}.LoadTemplate = () =>");
			using (PrePost.NewBlock(Writer, begin: "{", end: "};"))
			{
				var templateContext = new SourceGenContext(Writer, context.Compilation, context.SourceProductionContext, context.XmlnsCache, context.TypeCache, context.RootType!, null, context.ProjectItem)
				{
					ParentContext = context,
				};

				//inflate the template
				node.Accept(new CreateValuesVisitor(templateContext), null);
				node.Accept(new SetNamescopesAndRegisterNamesVisitor(templateContext), null);
				// node.Accept(new SetFieldVisitor(templateContext), null);
				node.Accept(new SetResourcesVisitor(templateContext), null);
				node.Accept(new SetPropertiesVisitor(templateContext, stopOnResourceDictionary: true), null);
				Writer.WriteLine($"return {templateContext.Variables[node].ValueAccessor};");
			}
			return;
		}

		//IMarkupExtension or IValueProvider => ProvideValue()
		node.TryProvideValue(Writer, context, getNodeValue);

		if (propertyName != XmlName.Empty)
		{
			// if (skips.Contains(propertyName))
			//     return;
			// if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
			//     return;

			SetPropertyHelpers.SetPropertyValue(Writer, Context.Variables[(ElementNode)parentNode], propertyName, node, Context);
		}
		else if (parentNode.IsCollectionItem(node) && parentNode is ElementNode)
		{
			var parentVar = Context.Variables[(ElementNode)parentNode];
			string? contentProperty;

			if (SetPropertyHelpers.CanAddToResourceDictionary(parentVar, parentVar.Type, node, Context, getNodeValue))
				SetPropertyHelpers.AddToResourceDictionary(Writer, parentVar, node, Context, getNodeValue);
			else if ((contentProperty = parentVar.Type.GetContentPropertyName(context)) != null)
			{
				var name = new XmlName(node.NamespaceURI, contentProperty);
				if (skips.Contains(name))
					return;
				if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(name))
					return;

				// Only check for duplicate property assignment if the property is not a collection
				// Get the property type and check if it supports Add() for collection behavior
				bool attached = false;
				var localName = name.LocalName;
				var bpFieldSymbol = parentVar.Type.GetBindableProperty(name.NamespaceURI, ref localName, out attached, context, node);
				ITypeSymbol? propertyType = null;
				
				// Try to get the property type
				bool hasProperty = (bpFieldSymbol != null && SetPropertyHelpers.CanGetValue(parentVar, bpFieldSymbol, attached, context, out propertyType))
					|| SetPropertyHelpers.CanGet(parentVar, localName, context, out propertyType, out _);
				
				// Only warn if:
				// 1. We can resolve the property and its type
				// 2. The property type is NOT a collection (doesn't support Add)
				// 3. The property type is not System.Object (unresolved generic)
				bool isObject = propertyType != null && propertyType.SpecialType == Microsoft.CodeAnalysis.SpecialType.System_Object;
				if (hasProperty && propertyType != null && !isObject && !propertyType.CanAdd(context))
					CheckForDuplicateProperty((ElementNode)parentNode, name, node);

				SetPropertyHelpers.SetPropertyValue(Writer, parentVar, name, node, Context);
			}
			else if (parentVar.Type.CanAdd(context))
			{
				// Skip if the node was removed from Variables (e.g., Setter with no value due to OnPlatform)
				if (Context.Variables.TryGetValue(node, out var nodeVar))
					Writer.WriteLine($"{parentVar.ValueAccessor}.Add({nodeVar.ValueAccessor});");
			}
			else
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ((ElementNode)parentNode).XmlType.Name);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, $"Cannot set the content of {((ElementNode)parentNode).XmlType.Name} as it doesn't have a ContentPropertyAttribute"));
			}
		}
		else if (parentNode.IsCollectionItem(node) && parentNode is ListNode parentList)
		{
			if (skips.Contains(parentList.XmlName))
				return;

			var parentVar = Context.Variables[(ElementNode)parentNode.Parent];
			if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
				return;
			var elementType = parentVar.Type;
			var localName = parentList.XmlName.LocalName;
			var bpFieldSymbol = parentVar.Type.GetBindableProperty(parentList.XmlName.NamespaceURI, ref localName, out System.Boolean attached, context, node as IXmlLineInfo);
			var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
			var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);

			var propertyType = typeandconverter?.type ?? propertySymbol?.Type;
			if (propertyType == null)
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, localName);
				//FIXME error should be "propertyType does not support Add()"
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
				return;
			}

			if (!context.VariablesProperties.TryGetValue((parentVar, bpFieldSymbol, propertySymbol), out var variable))
			{
				variable = new LocalVariable(propertyType, NamingHelpers.CreateUniqueVariableName(Context, propertyType));
				Writer.WriteLine($"var {variable.ValueAccessor} = {SetPropertyHelpers.GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, node, Context)};");
				context.VariablesProperties[(parentVar, bpFieldSymbol, propertySymbol)] = variable;
			}
			//TODO if we don't need the var, don't create it (this will likely be optimized by the compiler anyway, but...)


			if (SetPropertyHelpers.CanAddToResourceDictionary(variable, propertyType, node, Context, getNodeValue))
			{
				SetPropertyHelpers.AddToResourceDictionary(Writer, variable, node, Context, getNodeValue);
				return;
			}

			if (propertyType.CanAdd(context))
			{
				// Skip if the node was removed from Variables (e.g., Setter with no value due to OnPlatform)
				if (Context.Variables.TryGetValue(node, out var nodeVar))
					Writer.WriteLine($"{variable.ValueAccessor}.Add({nodeVar.ValueAccessor});");
			}
			else
			//report diagnostic: not a collection
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, localName);
				//FIXME error should be "propertyType does not support Add()"
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
			}
		}
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}
}
