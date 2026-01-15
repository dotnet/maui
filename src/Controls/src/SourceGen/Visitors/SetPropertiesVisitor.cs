using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;

class SetPropertiesVisitor : IXamlNodeVisitor
{
	public SetPropertiesVisitor(SourceGenContext context, bool stopOnResourceDictionary = false, bool stopOnStyle = true)
	{
		Context = context;
		StopOnResourceDictionary = stopOnResourceDictionary;
		StopOnStyle = stopOnStyle;
	}

	SourceGenContext Context { get; }
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
	public bool StopOnResourceDictionary { get; }
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => true;
	public bool VisitNodeOnDataTemplate => true;
	public bool StopOnStyle { get; } // Skip children by default; Style initializer can override
	public bool VisitNodeOnStyle => true; // But still visit the Style node itself to generate the initializer
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);
	public bool IsStyle(ElementNode node) => node.IsStyle(Context);

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
			if ((contentProperty = parentVar.Type.GetContentPropertyName(Context)) != null)
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
		NodeSGExtensions.GetNodeValueDelegate getNodeValue = (node, type) => Context.Variables[node];
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
				var templateContext = new SourceGenContext(Writer, Context.Compilation, Context.SourceProductionContext, Context.XmlnsCache, Context.TypeCache, Context.RootType!, null, Context.ProjectItem)
				{
					ParentContext = Context,
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
		node.TryProvideValue(Writer, Context, getNodeValue);

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
			else if ((contentProperty = parentVar.Type.GetContentPropertyName(Context)) != null)
			{
				var name = new XmlName(node.NamespaceURI, contentProperty);
				if (skips.Contains(name))
					return;
				if (parentNode is ElementNode node1 && node1.SkipProperties.Contains(propertyName))
					return;
				SetPropertyHelpers.SetPropertyValue(Writer, parentVar, name, node, Context);
			}
			else if (parentVar.Type.CanAdd(Context))
			{
				Writer.WriteLine($"{parentVar.ValueAccessor}.Add({Context.Variables[node].ValueAccessor});");
			}
			else
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ((ElementNode)parentNode).XmlType.Name);
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, location, $"Cannot set the content of {((ElementNode)parentNode).XmlType.Name} as it doesn't have a ContentPropertyAttribute"));
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
			var bpFieldSymbol = parentVar.Type.GetBindableProperty(parentList.XmlName.NamespaceURI, ref localName, out System.Boolean attached, Context, node as IXmlLineInfo);
			var propertySymbol = parentVar.Type.GetAllProperties(localName, Context).FirstOrDefault();
			var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(Context);

			var propertyType = typeandconverter?.type ?? propertySymbol?.Type;
			if (propertyType == null)
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, localName);
				//FIXME error should be "propertyType does not support Add()"
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
				return;
			}

			if (!Context.VariablesProperties.TryGetValue((parentVar, bpFieldSymbol, propertySymbol), out var variable))
			{
				variable = new LocalVariable(propertyType, NamingHelpers.CreateUniqueVariableName(Context, propertyType));
				Writer.WriteLine($"var {variable.ValueAccessor} = {SetPropertyHelpers.GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, node, Context)};");
				Context.VariablesProperties[(parentVar, bpFieldSymbol, propertySymbol)] = variable;
			}
			//TODO if we don't need the var, don't create it (this will likely be optimized by the compiler anyway, but...)


			if (SetPropertyHelpers.CanAddToResourceDictionary(variable, propertyType, node, Context, getNodeValue))
			{
				SetPropertyHelpers.AddToResourceDictionary(Writer, variable, node, Context, getNodeValue);
				return;
			}

			if (propertyType.CanAdd(Context))
			{
				Writer.WriteLine($"{variable.ValueAccessor}.Add({Context.Variables[node].ValueAccessor});");

			}
			else
			//report diagnostic: not a collection
			{
				var location = LocationCreate(Context.ProjectItem.RelativePath!, (IXmlLineInfo)node, localName);
				//FIXME error should be "propertyType does not support Add()"
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
			}
		}

		// Handle Style content - generate the initializer lambda
		// This runs after normal property handling, and only for Style nodes that have content
		if (node.IsStyle(Context) && node.Properties.ContainsKey(XmlName._StyleContent))
		{
			GenerateStyleInitializer(node);
		}
	}

	void GenerateStyleInitializer(ElementNode styleNode)
	{
		if (!Context.Variables.TryGetValue(styleNode, out var styleLocalValue))
			return;
		var styleVariable = (LocalVariable)styleLocalValue;

		// Get the target type that was stored in CreateValuesVisitor
		if (!Context.Types.TryGetValue(styleNode, out var targetType))
			return;

		// Generate the initializer assignment
		Writer.WriteLine($"{styleVariable.ValueAccessor}.Initializer = static (__style, __target) =>");
		using (PrePost.NewBlock(Writer, begin: "{", end: "};"))
		{
			// Generate type guard
			Writer.WriteLine($"if (__target is not {targetType.ToFQDisplayString()}) return;");
			Writer.WriteLine();

			var styleContext = new SourceGenContext(Writer, Context.Compilation, Context.SourceProductionContext, Context.XmlnsCache, Context.TypeCache, Context.RootType!, null, Context.ProjectItem)
			{
				ParentContext = Context,
			};

			// Register __style as the Style variable in this context
			var styleType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Style");
			styleContext.Variables[styleNode] = new LocalVariable(styleType!, "__style");

			// Visit Style's content nodes (Setters, Behaviors, Triggers, CollectionItems)
			// We need to process:
			// 1. Explicit Setters/Behaviors/Triggers properties
			// 2. Implicit CollectionItems (which are Setters)
			
			// Remove the marker property so normal visitors don't try to process it
			styleNode.Properties.Remove(XmlName._StyleContent);
			
			// Clear SkipProperties so the content properties are processed
			var contentPropertyNames = new[] { new XmlName("", "Setters"), new XmlName("", "Behaviors"), new XmlName("", "Triggers") };
			foreach (var propName in contentPropertyNames)
			{
				styleNode.SkipProperties.Remove(propName);
			}

			// Run the same visitor sequence as _CreateContent, but allow Style children to be visited.
			styleNode.Accept(new CreateValuesVisitor(styleContext, stopOnStyle: false), null);
			styleNode.Accept(new SetNamescopesAndRegisterNamesVisitor(styleContext, stopOnStyle: false), null);
			styleNode.Accept(new SetResourcesVisitor(styleContext, stopOnStyle: false), null);
			styleNode.Accept(new SetPropertiesVisitor(styleContext, stopOnResourceDictionary: true, stopOnStyle: false), null);
		}
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}
}
