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
		XmlName.xShared,
		XmlName.xTypeArguments,
	];
	public bool StopOnResourceDictionary => stopOnResourceDictionary;
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => true;
	public bool VisitNodeOnDataTemplate => true;
	// Skip children of lazy resources (they'll be created inside lambda), but the lazy resource node itself
	// gets visited (to emit AddFactory). VisitChildrenOfLazyResource handles this distinction.
	public bool SkipChildren(INode node, INode parentNode) => node is ElementNode en && en.IsLazyResource(parentNode, Context);
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

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
			var parentVariable = Context.Variables[(ElementNode)parentNode];
			if ((contentProperty = parentVariable.Type.GetContentPropertyName(context)) != null)
				propertyName = new XmlName(((ElementNode)parentNode).NamespaceURI, contentProperty);
			else
				return;
		}

		// Skip x: properties EXCEPT x:Name (which may need RuntimeName processing)
		// and skip SkipProperties and mc:Ignorable
		if (skips.Contains(propertyName) && propertyName != XmlName.xName)
			return;
		if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
			return;
		if (propertyName.Equals(XmlName.mcIgnorable))
			return;

		// Check that parent has a variable before accessing
		var parentElement = (ElementNode)parentNode;
		if (!Context.Variables.TryGetValue(parentElement, out var parentVar))
		{
			var parentKey = parentElement.Properties.TryGetValue(XmlName.xKey, out var keyNode) && keyNode is ValueNode vn ? vn.Value?.ToString() : "(none)";
			throw new System.InvalidOperationException($"SetPropertiesVisitor.Visit(ValueNode): Parent '{parentElement.XmlType.Name}' x:Key='{parentKey}' not in Variables for propertyName='{propertyName}'");
		}

		// Try to set runtime name (for x:Name with RuntimeNameProperty attribute)
		if (TrySetRuntimeName(propertyName, parentVar, node))
			return;

		// Skip x:Name after TrySetRuntimeName has had a chance to process it
		if (propertyName == XmlName.xName)
			return;

		SetPropertyHelpers.SetPropertyValue(Writer, parentVar, propertyName, node, Context);
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
		NodeSGExtensions.GetNodeValueDelegate getNodeValue = (n, type) => 
		{
			if (!context.Variables.TryGetValue(n, out var val))
			{
				var nodeName = n is ElementNode en ? en.XmlType.Name : n?.GetType().Name ?? "null";
				var nodeKey = n is ElementNode en2 && en2.Properties.TryGetValue(XmlName.xKey, out var kn) && kn is ValueNode vn ? vn.Value?.ToString() : "(none)";
				throw new System.InvalidOperationException($"getNodeValue delegate: Node '{nodeName}' x:Key='{nodeKey}' not in Variables");
			}
			return val;
		};
		XmlName propertyName = XmlName.Empty;

		// Store original parentNode for lazy resource check
		var originalParentNode = parentNode;

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

		// Handle lazy RD resources - they weren't created in CreateValuesVisitor
		if (node.IsLazyResource(originalParentNode, Context))
		{
			// Find the ResourceDictionary parent
			ILocalValue? rdVar = null;
			
			if (parentNode is ElementNode parentElement && Context.Variables.TryGetValue(parentElement, out var pVar))
			{
				var rdType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!;
				if (pVar.Type.InheritsFrom(rdType, Context))
				{
					rdVar = pVar;
				}
				else
				{
					// Check if it's a .Resources property
					var resourcesProperty = pVar.Type.GetAllProperties("Resources", Context).FirstOrDefault();
					if (resourcesProperty != null && resourcesProperty.Type.InheritsFrom(rdType, Context))
					{
						rdVar = new DirectValue(resourcesProperty.Type, $"{pVar.ValueAccessor}.Resources");
					}
				}
			}
			else if (parentNode is ListNode listNode && listNode.Parent is ElementNode grandParentElement && Context.Variables.TryGetValue(grandParentElement, out var gpVar))
			{
				// parentNode is a ListNode (e.g., implicit Resources collection)
				// The grandparent should have a Resources property
				var rdType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!;
				var resourcesProperty = gpVar.Type.GetAllProperties("Resources", Context).FirstOrDefault();
				if (resourcesProperty != null && resourcesProperty.Type.InheritsFrom(rdType, Context))
				{
					rdVar = new DirectValue(resourcesProperty.Type, $"{gpVar.ValueAccessor}.Resources");
				}
			}

			if (rdVar != null)
			{
				SetPropertyHelpers.AddLazyResourceToResourceDictionary(Writer, rdVar, node, Context);
				return;
			}
		}

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
			if (!Context.Variables.TryGetValue((ElementNode)parentNode, out var pVar1))
			{
				var key1 = ((ElementNode)parentNode).Properties.TryGetValue(XmlName.xKey, out var kn1) && kn1 is ValueNode vn1 ? vn1.Value?.ToString() : "(none)";
				throw new System.InvalidOperationException($"SetPropertiesVisitor.Visit(ElementNode) propertyName != Empty: Parent '{((ElementNode)parentNode).XmlType.Name}' x:Key='{key1}' not in Variables");
			}
			SetPropertyHelpers.SetPropertyValue(Writer, pVar1, propertyName, node, Context);
		}
		else if (parentNode.IsCollectionItem(node) && parentNode is ElementNode)
		{
			if (!Context.Variables.TryGetValue((ElementNode)parentNode, out var parentVar))
			{
				var key2 = ((ElementNode)parentNode).Properties.TryGetValue(XmlName.xKey, out var kn2) && kn2 is ValueNode vn2 ? vn2.Value?.ToString() : "(none)";
				throw new System.InvalidOperationException($"SetPropertiesVisitor.Visit(ElementNode) IsCollectionItem(parentNode ElementNode): Parent '{((ElementNode)parentNode).XmlType.Name}' x:Key='{key2}' not in Variables");
			}
			string? contentProperty;

			if (SetPropertyHelpers.CanAddToResourceDictionary(parentVar, parentVar.Type, node, Context, getNodeValue))
				SetPropertyHelpers.AddToResourceDictionary(Writer, parentVar, node, Context, getNodeValue);
			else if ((contentProperty = parentVar.Type.GetContentPropertyName(context)) != null)
			{
				var name = new XmlName(node.NamespaceURI, contentProperty);
				if (skips.Contains(name))
					return;
				if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
					return;
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

			if (!Context.Variables.TryGetValue((ElementNode)parentNode.Parent, out var parentVar))
			{
				var grandParent = (ElementNode)parentNode.Parent;
				var key3 = grandParent.Properties.TryGetValue(XmlName.xKey, out var kn3) && kn3 is ValueNode vn3 ? vn3.Value?.ToString() : "(none)";
				throw new System.InvalidOperationException($"SetPropertiesVisitor.Visit(ElementNode) IsCollectionItem(parentNode ListNode): GrandParent '{grandParent.XmlType.Name}' x:Key='{key3}' not in Variables. parentList.XmlName='{parentList.XmlName}'");
			}
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
