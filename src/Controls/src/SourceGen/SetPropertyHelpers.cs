using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;
using static LocationHelpers;

static class SetPropertyHelpers
{
	public static void SetPropertyValue(IndentedTextWriter writer, ILocalValue parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue = null, bool treeOrder = false, IndentedTextWriter? icWriter = null, ILocalValue? inflatorVar = null, bool asCollectionItem = false)
	{
		getNodeValue ??= (node, toType) => context.Variables[node];

		if (propertyName.Equals(XmlName._CreateContent))
			return; //already handled

		//TODO I believe ContentProperty should be resolved here
		var localName = propertyName.LocalName;
		bool attached = false;
		var bpFieldSymbol = !string.IsNullOrEmpty(propertyName.LocalName) ? parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out attached, context, (IXmlLineInfo)valueNode) : null;
		//an attached bindable property resolves a qualified name even when it is not accessible from here
		var hasBindableProperty = bpFieldSymbol != null;
		if (bpFieldSymbol != null && !context.Compilation.IsSymbolAccessibleWithin(bpFieldSymbol, context.RootType))
		{
			//not a diagnostic, as it might have a visible symbol matching for CanSet()
			bpFieldSymbol = null;
		}

		// event
		if (!asCollectionItem && CanConnectEvent(parentVar, localName, valueNode, attached, context))
		{
			ConnectEvent(writer, parentVar, localName, valueNode, context, treeOrder, icWriter, inflatorVar);
			return;
		}

		//DynamicResource
		if (!asCollectionItem && CanSetDynamicResource(bpFieldSymbol, valueNode, context, getNodeValue))
		{
			SetDynamicResource(writer, parentVar, bpFieldSymbol!, valueNode, context, getNodeValue);
			return;
		}

		//If it's a BP and the value is BindingBase, SetBinding
		if (!asCollectionItem && CanSetBinding(bpFieldSymbol, valueNode, parentVar.Type, localName, context, out var explicitPropertyNameForBinding))
		{
			SetBinding(writer, parentVar, bpFieldSymbol, localName, explicitPropertyNameForBinding, valueNode, context, getNodeValue);
			return;
		}

		// C# expression that resolves to x:DataType (binding expression)
		if (!asCollectionItem && TryHandleExpressionBinding(writer, parentVar, bpFieldSymbol, localName, valueNode, context))
		{
			return;
		}

		//If it's a BP, SetValue
		if (!asCollectionItem && CanSetValue(bpFieldSymbol, valueNode, parentVar.Type, localName, context, getNodeValue, out var explicitPropertyNameForValue))
		{
			SetValue(writer, parentVar, bpFieldSymbol, localName, explicitPropertyNameForValue, valueNode, context, getNodeValue);
			return;
		}

		//If the member is qualified with an extension container type, set the extension property
		var extensionPropertyReported = false;
		if (!asCollectionItem && CanSetExtensionProperty(parentVar, propertyName, valueNode, context, out var extensionAccessors, out extensionPropertyReported))
		{
			SetExtensionProperty(writer, parentVar, extensionAccessors!, valueNode, context, getNodeValue);
			return;
		}

		//a diagnostic was already emitted for the extension property, don't also report an unresolved member
		if (extensionPropertyReported)
			return;

		if (!hasBindableProperty && ReportUnresolvedExtensionMember(parentVar, propertyName, valueNode, context))
			return;

		//POCO, set the property
		if (!asCollectionItem && CanSet(parentVar, localName, valueNode, context))
		{
			Set(writer, parentVar, localName, valueNode, context, getNodeValue);
			return;
		}

		//An unqualified name may name an extension property brought in scope by the default xmlns
		var scopedReported = false;
		if (!asCollectionItem && !attached && CanSetScopedExtensionProperty(parentVar, propertyName, valueNode, context, out var scopedAccessors, out scopedReported))
		{
			SetExtensionProperty(writer, parentVar, scopedAccessors!, valueNode, context, getNodeValue);
			return;
		}

		if (scopedReported)
			return;

		if (CanAdd(parentVar, localName, bpFieldSymbol, attached, valueNode, context, getNodeValue))
		{
			Add(writer, parentVar, propertyName, valueNode, context, getNodeValue);
			return;
		}

		// If the node was removed from Variables (e.g., Setter with no value due to OnPlatform), skip silently
		if (valueNode is ElementNode en && !context.Variables.ContainsKey(en))
			return;

		var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, localName);
		context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
	}

	public static bool CanAddToResourceDictionary(ILocalValue parentVar, ITypeSymbol collectionType, ElementNode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		if (!collectionType.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!, context))
			return false;

		if (node.Properties.TryGetValue(XmlName.xKey, out var keyNode))
		{
			if (keyNode is not ValueNode vKeyNode || vKeyNode.Value is not string key)
			{
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)keyNode, ""), "x:Key must be a string literal"));
				//report diagnostic: x:Key must be a string literal
				return false;
			}
			if (!context.KeysInRD.TryGetValue(parentVar, out var keysInUse))
			{
				return true;
			}
			if (keysInUse.Contains(key))
			{
				var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)keyNode, key);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateKeyInRD, location, key));
				return false;
			}

			return true;
		}

		//is there an Add() overload that takes the type of the element ?
		var nodeType = getNodeValue(node, context.Compilation.ObjectType).Type;
		if (collectionType.GetAllMethods("Add", context).FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(nodeType, SymbolEqualityComparer.Default)) != null)
			return true;

		// No x:Key and no typed Add() overload - report error
		var missingKeyLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, "");
		context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, missingKeyLocation, "Resources in ResourceDictionary require a x:Key attribute"));
		return false;
	}

	public static string GetOrGetValue(ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, IPropertySymbol? property, INode node, SourceGenContext context)
	{
		if (bpFieldSymbol != null)
		{
			var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
			return $"({typeandconverter?.type.ToFQDisplayString()}){parentVar.ValueAccessor}.GetValue({bpFieldSymbol.ToFQDisplayString()})";
		}
		else if (property != null)
			return $"{parentVar.ValueAccessor}.{property.Name}";
		else
			return "null";
	}

	public static void AddToResourceDictionary(IndentedTextWriter writer, ILocalValue parentVar, ElementNode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		if (node.Properties.TryGetValue(XmlName.xKey, out var keyNode))
		{
			if (!context.KeysInRD.ContainsKey(parentVar))
				context.KeysInRD[parentVar] = [];
			context.KeysInRD[parentVar].Add((((ValueNode)keyNode).Value as string)!);
			var key = ((ValueNode)keyNode).Value as string;
			var escapedKey = CSharpExpressionHelpers.EscapeForString(key!);
			writer.WriteLine($"{parentVar.ValueAccessor}[\"{escapedKey}\"] = {(getNodeValue(node, context.Compilation.ObjectType)).ValueAccessor};");
			return;
		}
		writer.WriteLine($"{parentVar.ValueAccessor}.Add({getNodeValue(node, context.Compilation.ObjectType).ValueAccessor});");
	}

	/// <summary>
	/// Adds a lazy resource to the ResourceDictionary using AddFactory.
	/// The resource is created inside a lambda function for on-demand instantiation.
	/// </summary>
	public static void AddLazyResourceToResourceDictionary(IndentedTextWriter writer, ILocalValue parentVar, ElementNode node, SourceGenContext context)
	{
		// Get the type of the resource
		if (!node.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out var type) || type is null)
			return;

		// Determine if this is an implicit style
		bool hasKey = node.Properties.TryGetValue(XmlName.xKey, out var keyNode);
		bool isImplicitStyle = !hasKey && node.XmlType.Name == "Style";

		// Validate x:Key if present (same validation as CanAddToResourceDictionary)
		string? key = null;
		if (hasKey)
		{
			if (keyNode is not ValueNode vKeyNode || vKeyNode.Value is not string keyStr)
			{
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)keyNode!, ""), "x:Key must be a string literal"));
				return;
			}
			key = keyStr;

			// Check for duplicate keys
			if (!context.KeysInRD.TryGetValue(parentVar, out var keysInUse))
			{
				context.KeysInRD[parentVar] = keysInUse = [];
			}
			if (keysInUse.Contains(key))
			{
				var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)keyNode, key);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateKeyInRD, location, key));
				return;
			}
			keysInUse.Add(key);
		}

		// Get the x:Shared attribute (default is true)
		bool shared = true;
		if (node.Properties.TryGetValue(XmlName.xShared, out var sharedNode))
		{
			if (sharedNode is ValueNode vn && vn.Value is string sharedStr)
			{
				shared = !sharedStr.Equals("false", StringComparison.OrdinalIgnoreCase);
			}
		}

		// Generate the AddFactory call
		if (isImplicitStyle)
		{
			// Get TargetType from Style
			var targetTypeExpr = GetStyleTargetTypeExpression(node, context);
			if (targetTypeExpr == null)
			{
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ""), "Implicit style requires a TargetType"));
				return;
			}
			
			writer.WriteLine($"{parentVar.ValueAccessor}.AddFactory({targetTypeExpr}, () =>");
		}
		else
		{
			writer.WriteLine($"{parentVar.ValueAccessor}.AddFactory(\"{CSharpExpressionHelpers.EscapeForString(key!)}\", () =>");
		}
		
		using (PrePost.NewBlock(writer, begin: "{", end: $"}}, shared: {shared.ToString().ToLowerInvariant()});"))
		{
			// Create a temporary context for generating the lambda body
			var lambdaContext = new SourceGenContext(
				writer, 
				context.Compilation, 
				context.SourceProductionContext, 
				context.XmlnsCache, 
				context.TypeCache, 
				context.RootType, 
				null, 
				context.ProjectItem,
				context.ReportDiagnostic)
			{
				ParentContext = context
			};

			// First pass: Create all values (node and its descendants) using CreateValuesVisitor
			// This mirrors the normal flow: CreateValuesVisitor walks the entire tree first
			node.Accept(new CreateValuesVisitor(lambdaContext), null);

			// Second pass: Set namescopes and register names in the namescope
			node.Accept(new SetNamescopesAndRegisterNamesVisitor(lambdaContext), null);

			// Third pass: Set resources in ResourceDictionary
			node.Accept(new SetResourcesVisitor(lambdaContext), null);

			// Fourth pass: Set properties on all nodes using SetPropertiesVisitor
			// stopOnResourceDictionary=true prevents infinite recursion if there are nested RDs
			node.Accept(new SetPropertiesVisitor(lambdaContext, stopOnResourceDictionary: true), null);

			// Return the created object
			if (lambdaContext.Variables.TryGetValue(node, out var nodeVar))
			{
				writer.WriteLine($"return {nodeVar.ValueAccessor};");
			}
			else
			{
				// Fallback - shouldn't happen
				writer.WriteLine($"return null!;");
			}
		}
	}

	/// <summary>
	/// Gets the TargetType expression for a Style node.
	/// </summary>
	static string? GetStyleTargetTypeExpression(ElementNode node, SourceGenContext context)
	{
		if (!node.Properties.TryGetValue(new XmlName("", "TargetType"), out var targetTypeNode))
			return null;

		// Case 1: String value - TargetType="Label"
		if (targetTypeNode is ValueNode valueNode && valueNode.Value is string typeName)
		{
			var typeSymbol = typeName.GetTypeSymbol(context, node);
			if (typeSymbol != null)
				return $"typeof({typeSymbol.ToFQDisplayString()})";
			return null;
		}

		// Case 2: TypeExtension markup - TargetType="{x:Type Label}"
		if (targetTypeNode is ElementNode elementNode && 
			(elementNode.XmlType.Name == "TypeExtension" || elementNode.XmlType.Name == "Type"))
		{
			// TypeExtension can have TypeName as property or positional argument
			if (elementNode.Properties.TryGetValue(new XmlName("", "TypeName"), out var typeNameNode) && 
				typeNameNode is ValueNode tn)
			{
				var typeNameStr = tn.Value as string;
				var typeSymbol = typeNameStr!.GetTypeSymbol(context, node);
				if (typeSymbol != null)
					return $"typeof({typeSymbol.ToFQDisplayString()})";
			}
			else if (elementNode.CollectionItems.Count > 0 && elementNode.CollectionItems[0] is ValueNode positionalArg)
			{
				var typeNameStr = positionalArg.Value as string;
				var typeSymbol = typeNameStr!.GetTypeSymbol(context, node);
				if (typeSymbol != null)
					return $"typeof({typeSymbol.ToFQDisplayString()})";
			}
		}

		return null;
	}

	static bool CanSet(ILocalValue parentVar, string localName, INode node, SourceGenContext context)
	{
		if (parentVar.Type.GetAllProperties(localName, context).FirstOrDefault() is not IPropertySymbol property)
			return false;
		if (property.SetMethod is not IMethodSymbol propertySetter || !propertySetter.IsPublic() || propertySetter.IsStatic)
			return false;
		if (node is ValueNode vn && vn.CanConvertTo(property, context))
			return true;
		if (node is not ElementNode elementNode)
			return false;
		if (!context.Variables.TryGetValue(elementNode, out var localVar))
			return false;
		if (localVar.Type.InheritsFrom(property.Type, context))
			return true;
		if (property.Type.IsInterface() && localVar.Type.Implements(property.Type))
			return true;

		if (property.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return true;

		if (context.Compilation.HasImplicitConversion(localVar.Type, property.Type))
			return true;

		if (HasDoubleImplicitConversion(localVar.Type, property.Type, context, out _))
			return true;

		if (HasExplicitConversion(localVar.Type, property.Type, context))
			return true;

		//TODO could we replace this by a runimt check (generating a if/else) ?            
		if (localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return true;

		return false;
	}

	static bool CanSetDynamicResource(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		if (bpFieldSymbol == null)
			return false;
		if (node is not ElementNode en)
			return false;

		//TODO we could get the type directly from the XmlType of the node, so no need to instantiate de extension at all
		if (!context.Variables.TryGetValue(en, out var localVar))
			return false;

		return localVar.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.DynamicResource")!, context);
	}

	static void SetDynamicResource(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol fieldSymbol, INode valueNode, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
		=> writer.WriteLine($"((global::Microsoft.Maui.Controls.Internals.IDynamicResourceHandler){parentVar.ValueAccessor}).SetDynamicResource({fieldSymbol.ToFQDisplayString()}, {(getNodeValue(valueNode, context.Compilation.ObjectType)).ValueAccessor}.Key);");

	static bool CanConnectEvent(ILocalValue parentVar, string localName, INode valueNode, bool attached, SourceGenContext context)
	{
		if (attached)
			return false;
		if (!parentVar.Type.GetAllEvents(localName, context).Any())
			return false;

		// Accept ValueNode with string (method name) or Expression (lambda)
		if (valueNode is ValueNode vn)
		{
			if (vn.Value is string)
				return true;
			if (vn.Value is Expression)
				return true;
		}
		return false;
	}

	static void ConnectEvent(IndentedTextWriter writer, ILocalValue parentVar, string localName, INode valueNode, SourceGenContext context, bool treeOrder, IndentedTextWriter? icWriter, ILocalValue? inflatorVar)
	{
		var eventSymbol = parentVar.Type.GetAllEvents(localName, context).First();
		var eventType = eventSymbol.Type;
		var vn = (ValueNode)valueNode;

		// Handle lambda expressions
		if (vn.Value is Expression expression)
		{
			if (treeOrder && icWriter != null && inflatorVar != null)
			{
				writer = icWriter;
				parentVar = inflatorVar;
			}
			// Transform quotes with semantic context
			var transformedCode = CSharpExpressionHelpers.TransformQuotesWithSemantics(
				expression.Code, context.Compilation, context.RootType);
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.ProjectItem) : PrePost.NoBlock())
			{
				writer.WriteLine($"{parentVar.ValueAccessor}.{localName} += {transformedCode};");
			}
			return;
		}

		// Original method name handler logic
		var handler = (string)vn.Value;
		var handlerSymbol = context.RootType.GetAllMethods(handler, context).FirstOrDefault(m =>
		{
			if (m.Name != handler)
				return false;
			var invoke = eventType.GetAllMethods("Invoke", context).FirstOrDefault();
			if (invoke.Parameters.Length != m.Parameters.Length)
				return false;
			if (!invoke.ReturnType.InheritsFrom(m.ReturnType, context))
				return false;
			for (int i = 0; i < invoke.Parameters.Length; i++)
			{
				if (!invoke.Parameters[i].Type.InheritsFrom(m.Parameters[i].Type, context))
					return false;
			}
			return true;
		});
		if (handlerSymbol == null)
		{
			var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, handler);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingEventHandler, location, handler, context.RootType.ToFQDisplayString()));
			return;
		}
		if (treeOrder && icWriter != null && inflatorVar != null)
		{
			writer = icWriter;
			parentVar = inflatorVar;
		}
		using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.ProjectItem) : PrePost.NoBlock())
		{
			writer.WriteLine($"{parentVar.ValueAccessor}.{localName} += {handler};");
		}
	}

	static bool CanSetValue(IFieldSymbol? bpFieldSymbol, INode node, ITypeSymbol parentType, string localName, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue, out string? explicitPropertyName)
	{
		explicitPropertyName = null;

		if (bpFieldSymbol != null)
		{
			// Normal BP case - apply existing logic
			if (node is ValueNode vn && vn.CanConvertTo(bpFieldSymbol, context))
				return true;
			if (node is not ElementNode en)
				return false;

			var localVar = getNodeValue(en, context.Compilation.ObjectType);

			// If it's an attached BP, there's no second chance to handle IMarkupExtensions, so we try here.
			// Worst case scenario ? InvalidCastException at runtime
			if (localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
				return true;

			var bpTypeAndConverter = bpFieldSymbol.GetBPTypeAndConverter(context);
			if (context.Compilation.HasImplicitConversion(localVar.Type, bpTypeAndConverter?.type))
				return true;

			if (HasDoubleImplicitConversion(localVar.Type, bpTypeAndConverter?.type, context, out _))
				return true;

			if (HasExplicitConversion(localVar.Type, bpTypeAndConverter?.type, context))
				return true;

			if (localVar.Type.InheritsFrom(bpTypeAndConverter?.type!, context))
				return true;

			if (bpFieldSymbol.Type.IsInterface() && localVar.Type.Implements(bpTypeAndConverter?.type!))
				return true;

			return false;
		}

		// Heuristic: If BP is null but the type has a property/field with a BindablePropertyAttribute,
		// assume the BP will be generated by another source generator
		// Only apply this for non-BindingBase nodes (CanSetBinding handles BindingBase)
		if (!string.IsNullOrEmpty(localName) && !IsBindingBaseNode(node, context))
		{
			return parentType.HasBindablePropertyHeuristic(localName, context, out explicitPropertyName);
		}

		return false;
	}

	static void SetValue(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, string localName, string? explicitPropertyName, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		// Determine bindable property name: use BP field symbol if available, otherwise use heuristic
		var bpName = bpFieldSymbol != null
			? bpFieldSymbol.ToFQDisplayString()
			: $"{parentVar.Type.ToFQDisplayString()}.{explicitPropertyName ?? $"{localName}Property"}";

		var pType = bpFieldSymbol?.GetBPTypeAndConverter(context)?.type;
		var property = bpFieldSymbol == null ? parentVar.Type.GetAllProperties(localName, context).FirstOrDefault() : null;

		if (node is ValueNode valueNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var valueString = bpFieldSymbol != null
					? valueNode.ConvertTo(bpFieldSymbol, writer, context, parentVar)
					: (property != null ? valueNode.ConvertTo(property, writer, context, parentVar) : getNodeValue(node, context.Compilation.ObjectType).ValueAccessor);
				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpName}, {valueString});");
			}
		}
		else if (node is ElementNode elementNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var localVar = getNodeValue(elementNode, context.Compilation.ObjectType);
				var cast = string.Empty;

				if (bpFieldSymbol != null)
				{
					// BP case: check for double implicit conversion first
					if (HasDoubleImplicitConversion(localVar.Type, pType, context, out var conv))
					{
						cast = "(" + conv!.ReturnType.ToFQDisplayString() + ")";
					}
					else if (pType != null && !context.Compilation.HasImplicitConversion(localVar.Type, pType) && HasExplicitConversion(localVar.Type, pType, context))
					{
						// Only add cast if the source type is not object (object can be cast to anything at runtime)
						if (!localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
						{
							cast = $"({pType.ToFQDisplayString()})";
						}
					}
				}
				else if (property != null && !context.Compilation.HasImplicitConversion(localVar.Type, property.Type))
				{
					cast = $"({property.Type.ToFQDisplayString()})";
				}

				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpName}, {cast}{localVar.ValueAccessor});");
			}
		}
	}

	internal static bool CanGet(ILocalValue parentVar, string localName, SourceGenContext context, out ITypeSymbol? propertyType, out IPropertySymbol? propertySymbol)
	{
		propertyType = null;
		if ((propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault()) == null)
			return false;
		if (propertySymbol!.GetMethod is not IMethodSymbol propertyGetter || !propertyGetter.IsPublic() || propertyGetter.IsStatic)
			return false;

		propertyType = propertySymbol.Type;
		return true;
	}

	internal static bool CanGetValue(ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, bool attached, SourceGenContext context, out ITypeSymbol? propertyType)
	{
		propertyType = null;
		if (bpFieldSymbol == null)
			return false;

		if (!parentVar.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!, context))
			return false;

		propertyType = bpFieldSymbol.GetBPTypeAndConverter(context)?.type;
		return true;
	}

	static bool HasDoubleImplicitConversion(ITypeSymbol? fromType, ITypeSymbol? toType, SourceGenContext context, out IMethodSymbol? op)
	{
		op = null;
		if (fromType == null || toType == null)
			return false;

		//return false, no need to multiple cast here    
		if (context.Compilation.HasImplicitConversion(fromType, toType))
			return false;

		IMethodSymbol[] implicitOps =
			[
				.. fromType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion),
				.. toType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion)
			];

		foreach (var implicitOp in implicitOps)
		{
			if (context.Compilation.HasImplicitConversion(fromType, implicitOp.Parameters[0].Type)
				&& context.Compilation.HasImplicitConversion(implicitOp.ReturnType, toType))
			{
				op = implicitOp;
				return true;
			}
		}

		return false;
	}

	static bool HasExplicitConversion(ITypeSymbol? fromType, ITypeSymbol? toType, SourceGenContext context)
	{
		if (fromType == null || toType == null)
			return false;

		// If there's already an implicit conversion, we don't need explicit cast
		if (context.Compilation.HasImplicitConversion(fromType, toType))
			return false;

		// Check for explicit conversion operators on both types
		IMethodSymbol[] conversionOps =
			[
				.. fromType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion),
				.. toType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion)
			];

		foreach (var conversionOp in conversionOps)
		{
			// Check if this conversion operator can convert fromType to toType
			if (SymbolEqualityComparer.Default.Equals(conversionOp.Parameters[0].Type, fromType) &&
				SymbolEqualityComparer.Default.Equals(conversionOp.ReturnType, toType))
			{
				return true;
			}
		}

		// Check for valid reference type casts (inheritance or interface)
		// Only allow if both are collections or both are non-collections
		if (fromType.IsReferenceType && toType.IsReferenceType)
		{
			var fromIsCollection = fromType.AllInterfaces.Any(i => i.ToString() == "System.Collections.IEnumerable") && fromType.SpecialType != SpecialType.System_String;
			var toIsCollection = toType.AllInterfaces.Any(i => i.ToString() == "System.Collections.IEnumerable") && toType.SpecialType != SpecialType.System_String;

			// Both must be collections, or both must be non-collections
			if (fromIsCollection == toIsCollection)
			{
				// Same inheritance chain or one is an interface
				if (fromType.InheritsFrom(toType, context) ||
					toType.InheritsFrom(fromType, context) ||
					toType.TypeKind == TypeKind.Interface ||
					fromType.TypeKind == TypeKind.Interface)
				{
					return true;
				}
			}
		}

		return false;
	}

	static void Set(IndentedTextWriter writer, ILocalValue parentVar, string localName, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		var property = parentVar.Type.GetAllProperties(localName, context).First();

		if (node is ValueNode valueNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var valueString = valueNode.ConvertTo(property, writer, context, parentVar);
				writer.WriteLine($"{parentVar.ValueAccessor}.{EscapeIdentifier(localName)} = {valueString};");
			}
		}
		else if (node is ElementNode elementNode)
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var localVar = getNodeValue(elementNode, context.Compilation.ObjectType);
				string intermediateCast = HasDoubleImplicitConversion(localVar.Type, property.Type, context, out var conv) ? "(" + conv!.ReturnType.ToFQDisplayString() + ")" : string.Empty;
				writer.WriteLine($"{parentVar.ValueAccessor}.{EscapeIdentifier(localName)} = ({property.Type.ToFQDisplayString()}){intermediateCast}{localVar.ValueAccessor};");
			}
	}

	static bool CanSetBinding(IFieldSymbol? bpFieldSymbol, INode node, ITypeSymbol parentType, string localName, SourceGenContext context, out string? explicitPropertyName)
	{
		explicitPropertyName = null;

		// Check if it's a BindingBase node
		if (!IsBindingBaseNode(node, context))
			return false;

		// If we have a BP field symbol, we can set binding
		if (bpFieldSymbol != null)
			return true;

		// Heuristic: If BP is null but the type has a property/field with a BindablePropertyAttribute,
		// assume the BP will be generated by another source generator
		if (!string.IsNullOrEmpty(localName))
			return parentType.HasBindablePropertyHeuristic(localName, context, out explicitPropertyName);

		return false;
	}

	static void SetBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, string localName, string? explicitPropertyName, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		var localVariable = getNodeValue((ElementNode)node, context.Compilation.ObjectType);

		if (bpFieldSymbol != null)
		{
			// Normal case: we have the BP field symbol
			writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({bpFieldSymbol.ToFQDisplayString()}, {localVariable.ValueAccessor});");
		}
		else
		{
			// Heuristic case: generate SetBinding call using the expected BindableProperty name
			// Use explicit property name if provided by attribute, otherwise use the default {localName}Property format
			var bpName = explicitPropertyName ?? $"{localName}Property";
			writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({parentVar.Type.ToFQDisplayString()}.{bpName}, {localVariable.ValueAccessor});");
		}
	}

	static bool CanAdd(ILocalValue parentVar, string localName, IFieldSymbol? bpFieldSymbol, bool attached, INode valueNode, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		//if localname is null, we're adding to the parentVar, not one of it's property
		if (valueNode is not ElementNode en)
			return false;

		ITypeSymbol? propertyType = null;
		if (localName != null
			&& !CanGetValue(parentVar, bpFieldSymbol, attached, context, out propertyType)
			&& !CanGet(parentVar, localName, context, out propertyType, out _))
			return false;

		if (!context.Variables.TryGetValue(en, out var childVar))
			return false;

		if (CanAddToResourceDictionary(parentVar, localName != null ? propertyType! : parentVar.Type, en, context, getNodeValue))
			return true;

		//FIXME should be better and test for value type
		if ((localName != null ? propertyType! : parentVar.Type).CanAdd(context))
			return true;

		return false;

	}

	static void Add(IndentedTextWriter writer, ILocalValue parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		//if localname is null, we're adding to the parentVar, not one of it's property
		var localName = propertyName.LocalName;
		bool attached = false;
		var bpFieldSymbol = localName != null ? parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out attached, context, valueNode as IXmlLineInfo) : null;
		IPropertySymbol? propertySymbol = null;
		ITypeSymbol? propertyType = null;

		if (localName != null)
			//one of those will return true, but we need the propertyType
			_ = CanGetValue(parentVar, bpFieldSymbol, attached, context, out propertyType) || CanGet(parentVar, localName, context, out propertyType, out propertySymbol);

		else
			propertyType = parentVar.Type;

		if (CanAddToResourceDictionary(parentVar, propertyType!, (ElementNode)valueNode, context, getNodeValue))
		{
			ILocalValue rdAccessor;
			if (localName != null)
				rdAccessor = new DirectValue(propertyType!, GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, valueNode, context));
			else
				rdAccessor = parentVar;

			AddToResourceDictionary(writer, rdAccessor, (ElementNode)valueNode, context, getNodeValue);
			return;
		}

		ITypeSymbol itemType;
		if (propertyType!.ImplementsGeneric(context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")!, out var typeArguments))
			itemType = typeArguments[0];
		else
			itemType = context.Compilation.ObjectType;

		var adder = propertyType!.GetAllMethods("Add", context).First(m => m.Parameters.Length == 1);
		var receiverType = adder.ReceiverType;

		var parentObj = $"{parentVar.ValueAccessor}.{localName}";
		if (bpFieldSymbol != null)
		{
			var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
			parentObj = $"(({typeandconverter?.type.ToFQDisplayString()}){parentVar.ValueAccessor}.GetValue({bpFieldSymbol.ToFQDisplayString()}))";
		}

		if (receiverType is not null && !propertyType!.Equals(receiverType, SymbolEqualityComparer.Default))
			parentObj = $"(({receiverType.ToFQDisplayString()}){parentObj})";

		//look for intermediate implicit casts
		string cast = string.Empty;
		if (HasDoubleImplicitConversion(getNodeValue(valueNode, context.Compilation.ObjectType).Type, itemType, context, out var conv))
			cast = "(" + conv!.ReturnType.ToFQDisplayString() + ")";

		using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.ProjectItem) : PrePost.NoBlock())
			writer.WriteLine($"{parentObj}.Add(({itemType.ToFQDisplayString()}){cast}{getNodeValue(valueNode, context.Compilation.ObjectType).ValueAccessor});");
	}

	static bool IsBindingBaseNode(INode node, SourceGenContext context)
	{
		if (node is not ElementNode en)
			return false;
		if (!context.Variables.TryGetValue(en, out var localVariable))
			return false;

		var bindingBaseSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingBase")!;

		if (localVariable.Type.InheritsFrom(bindingBaseSymbol, context))
			return true;

		if (context.Compilation.HasImplicitConversion(localVariable.Type, bindingBaseSymbol))
			return true;

		return false;
	}

	/// <summary>
	/// Handles C# expressions that should generate TypedBindings (when referencing x:DataType members).
	/// Returns true if the expression was handled, false if it should fall through to other handlers.
	/// </summary>
	static bool TryHandleExpressionBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, string localName, INode valueNode, SourceGenContext context)
	{
		// Only handle ValueNode with Expression value
		if (valueNode is not ValueNode vn || vn.Value is not Expression expression)
			return false;

		// Need a BindableProperty to set a binding
		if (bpFieldSymbol == null)
			return false;

		// Get the parent ElementNode to find x:DataType
		var parentElement = valueNode.Parent as ElementNode;
		if (parentElement == null)
			return false;

		// Try to get x:DataType from the element tree
		if (!XDataTypeResolver.TryGetXDataType(parentElement, context, out var dataTypeSymbol) || dataTypeSymbol == null)
		{
			// No x:DataType - expression is a local 'this' reference, let SetValue handle it
			return false;
		}

		// Analyze the expression for mixed local+binding scenarios
		var analysis = ExpressionAnalyzer.Analyze(expression.Code, "__source", dataTypeSymbol, context.RootType);

		// Check for ambiguity first - resolve the expression
		var resolution = MemberResolver.Resolve(expression.Code, context.RootType, dataTypeSymbol, context.Compilation);

		// Handle ambiguous case - always an error
		if (resolution.Location == MemberLocation.Both)
		{
			var bothLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.AmbiguousMemberExpression, bothLocation, resolution.RootIdentifier, context.RootType?.Name ?? "this", dataTypeSymbol.Name));
			return true; // Handled (with error)
		}

		// Warn if member name conflicts with a well-known static type
		if (resolution.ConflictsWithStaticType)
		{
			var staticLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
			var typeName = resolution.Location == MemberLocation.This ? (context.RootType?.Name ?? "this") : dataTypeSymbol.Name;
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.AmbiguousMemberWithStaticType, staticLocation, resolution.RootIdentifier, typeName));
			// Continue processing - this is just a warning
		}

		// Handle not-found case for simple identifiers
		if (resolution.Location == MemberLocation.Neither &&
			!string.IsNullOrEmpty(resolution.RootIdentifier) &&
			MemberResolver.IsSimpleIdentifier(expression.Code))
		{
			var neitherLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberNotFound, neitherLocation, resolution.RootIdentifier, context.RootType?.Name ?? "this", dataTypeSymbol.Name));
			return true; // Handled (with error)
		}

		// If we have binding handlers, this needs a TypedBinding
		// This covers complex expressions like (Price * TaxRate) where MemberResolver returns Neither
		if (analysis.HasBindingProperties)
		{
			SetExpressionBinding(writer, parentVar, bpFieldSymbol, expression.Code, dataTypeSymbol, context, vn);
			return true;
		}

		// Handle based on member location
		switch (resolution.Location)
		{
			case MemberLocation.This:
			case MemberLocation.ForcedThis:
				// Local expression - let SetValue handle it
				return false;

			case MemberLocation.DataType:
			case MemberLocation.ForcedDataType:
				// Binding expression - generate TypedBinding
				SetExpressionBinding(writer, parentVar, bpFieldSymbol, resolution.Expression, dataTypeSymbol, context, vn);
				return true;

			case MemberLocation.Neither:
				// Complex expression with no binding properties - let SetValue handle it
				return false;

			default:
				return false;
		}
	}

	/// <summary>
	/// Generates a TypedBinding for a C# expression that references x:DataType members.
	/// Overload for UC: accepts string accessors instead of IC's ILocalValue/IFieldSymbol.
	/// </summary>
	internal static void SetExpressionBindingForUC(IndentedTextWriter writer, string targetAccessor, string bpFieldFqn, string expression, ITypeSymbol dataTypeSymbol, INamedTypeSymbol rootType, Compilation compilation)
	{
		var sourceTypeName = dataTypeSymbol.ToFQDisplayString();

		var transformedExpression = CSharpExpressionHelpers.TransformQuotesWithSemantics(
			expression, compilation, dataTypeSymbol, rootType);

		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, rootType);
		var handlers = analysis.Handlers;

		var expressionType = ResolveExpressionType(expression, dataTypeSymbol, context: null);
		var propertyTypeName = expressionType?.ToFQDisplayString() ?? "object";

		bool hasCaptures = analysis.Captures.Count > 0;
		if (hasCaptures)
		{
			writer.WriteLine("{");
			writer.Indent++;
			foreach (var capture in analysis.Captures)
			{
				writer.WriteLine($"var {capture.CaptureVariable} = this.{capture.InvocationExpression};");
			}
		}

		writer.WriteLine($"{targetAccessor}.SetBinding({bpFieldFqn},");
		writer.Indent++;
		writer.WriteLine($"new global::Microsoft.Maui.Controls.Internals.TypedBinding<{sourceTypeName}, {propertyTypeName}>(");
		writer.Indent++;

		var getterExpression = analysis.TransformedExpression;
		if (getterExpression.Contains("?.") && !getterExpression.EndsWith("!", StringComparison.Ordinal))
			getterExpression += "!";
		writer.WriteLine($"__source => ({getterExpression}, true),");

		// UC intentionally skips the ExpressionNotSettable diagnostic (Descriptors.ExpressionNotSettable)
		// that IC emits for TwoWay-default BPs with non-settable expressions. During hot reload,
		// the developer is iterating rapidly and a warning would be noise — the IC already warned at build time.
		if (analysis.IsSettable && IsExpressionWritable(expression, dataTypeSymbol, context: null))
			writer.WriteLine($"(__source, __value) => {analysis.TransformedExpression} = __value,");
		else
			writer.WriteLine("null,");

		if (handlers.Count == 0)
		{
			writer.WriteLine($"null));");
		}
		else
		{
			writer.WriteLine($"new global::System.Tuple<global::System.Func<{sourceTypeName}, object>, string>[] {{");
			writer.Indent++;
			for (int i = 0; i < handlers.Count; i++)
			{
				var handler = handlers[i];
				var comma = i < handlers.Count - 1 ? "," : "";
				writer.WriteLine($"new(static __source => {handler.ParentExpression}, \"{handler.PropertyName}\"){comma}");
			}
			writer.Indent--;
			writer.WriteLine($"}}));");
		}
		writer.Indent -= 2;

		if (hasCaptures)
		{
			writer.Indent--;
			writer.WriteLine("}");
		}
	}

	/// <summary>
	/// Generates a TypedBinding for a C# expression that references x:DataType members.
	/// </summary>
	static void SetExpressionBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol bpFieldSymbol, string expression, ITypeSymbol dataTypeSymbol, SourceGenContext context, ValueNode valueNode)
	{
		var bpName = bpFieldSymbol.ToFQDisplayString();

		var sourceTypeName = dataTypeSymbol.ToFQDisplayString();

		// Transform quotes with semantic context - char literals stay as char only if target expects char
		var transformedExpression = CSharpExpressionHelpers.TransformQuotesWithSemantics(
			expression, context.Compilation, dataTypeSymbol, context.RootType);

		// Analyze expression for mixed local+binding scenarios
		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, context.RootType);
		var handlers = analysis.Handlers;

		// Resolve the expression's result type for TProperty.
		// TypedBinding<TSource, TProperty> should use the expression type (e.g., decimal for Price),
		// NOT the target BindableProperty type (e.g., string for Entry.TextProperty).
		// The binding infrastructure handles type conversion at runtime.
		var expressionType = ResolveExpressionType(expression, dataTypeSymbol, context);
		var propertyTypeName = expressionType?.ToFQDisplayString() ?? "object";

		// Wrap in scoped block if we have captures to avoid duplicate variable names
		// when multiple expressions capture the same local member
		bool hasCaptures = analysis.Captures.Count > 0;
		if (hasCaptures)
		{
			writer.WriteLine("{");
			writer.Indent++;
			// Generate capture statements for local values (this.X or this.Method())
			foreach (var capture in analysis.Captures)
			{
				writer.WriteLine($"var {capture.CaptureVariable} = this.{capture.InvocationExpression};");
			}
		}

		// The getter must return (TProperty value, bool success) tuple
		using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.ProjectItem) : PrePost.NoBlock())
		{
			writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({bpName},");
			writer.Indent++;
			writer.WriteLine($"new global::Microsoft.Maui.Controls.Internals.TypedBinding<{sourceTypeName}, {propertyTypeName}>(");
			writer.Indent++;
			// TransformedExpression already has identifiers prefixed with __source. where needed
			// Add null-forgiving operator if expression contains ?. to suppress nullability warnings
			var getterExpression = analysis.TransformedExpression;
			if (getterExpression.Contains("?.") && !getterExpression.EndsWith("!", StringComparison.Ordinal))
				getterExpression += "!";
			writer.WriteLine($"__source => ({getterExpression}, true),");

			// Generate setter if expression is a simple property chain AND the terminal property is writable
			if (analysis.IsSettable && IsExpressionWritable(expression, dataTypeSymbol, context))
			{
				writer.WriteLine($"(__source, __value) => {analysis.TransformedExpression} = __value,");
			}
			else
			{
				writer.WriteLine($"null,");
				// Emit info diagnostic when binding a complex expression to a TwoWay property
				if (IsTwoWayByDefault(bpFieldSymbol))
				{
					var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression);
					context.ReportDiagnostic(Diagnostic.Create(
						Descriptors.ExpressionNotSettable,
						location,
						expression,
						bpFieldSymbol.Name));
				}
			}

			// Generate handlers array
			if (handlers.Count == 0)
			{
				writer.WriteLine($"null));");
			}
			else
			{
				writer.WriteLine($"new global::System.Tuple<global::System.Func<{sourceTypeName}, object>, string>[] {{");
				writer.Indent++;
				for (int i = 0; i < handlers.Count; i++)
				{
					var handler = handlers[i];
					var comma = i < handlers.Count - 1 ? "," : "";
					// Lambda parameter is always __source, body is the parent expression
					writer.WriteLine($"new(static __source => {handler.ParentExpression}, \"{handler.PropertyName}\"){comma}");
				}
				writer.Indent--;
				writer.WriteLine($"}}));");
			}
			writer.Indent -= 2;
		}

		if (hasCaptures)
		{
			writer.Indent--;
			writer.WriteLine("}");
		}
	}

	/// <summary>
	/// Resolves the result type of a C# expression by walking the property chain on the dataType.
	/// For example, "Price" on SimpleViewModel resolves to decimal, "User.DisplayName" resolves to string.
	/// For complex expressions (operators, method calls, interpolation), returns null to fall back to object.
	/// </summary>
	internal static ITypeSymbol? ResolveExpressionType(string expression, ITypeSymbol dataType, SourceGenContext? context)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return null;

		var expr = expression.Trim();

		// Strip leading dot prefix (e.g., ".Name" → "Name"), but avoid the ".." range operator
		if (expr.StartsWith(".", StringComparison.Ordinal) &&
			!expr.StartsWith("..", StringComparison.Ordinal))
			expr = expr.Substring(1);

		// Strip "BindingContext." prefix only when it refers to BindableObject.BindingContext,
		// not a user-defined member with the same name (e.g., a POCO with `public Person BindingContext;`)
		if (expr.StartsWith("BindingContext.", StringComparison.Ordinal)
			&& !dataType.GetAllMembers("BindingContext", context).Any())
			expr = expr.Substring("BindingContext.".Length);

		if (string.IsNullOrEmpty(expr))
			return null;

		// Walk the dot-separated property chain (also handle ?. null-conditional access)
		var parts = expr.Replace("?.", ".").Split('.');
		var currentType = dataType;

		foreach (var part in parts)
		{
			var memberName = part.Trim().TrimEnd('!');

			// If it contains parens, operators, or special chars, it's not a simple property chain
			if (memberName.Contains('(') || memberName.Contains(' ') || memberName.Contains('[') || string.IsNullOrEmpty(memberName))
				return null;

			var member = currentType.GetAllMembers(memberName, context).FirstOrDefault();
			if (member is IPropertySymbol prop)
				currentType = prop.Type;
			else if (member is IFieldSymbol field)
				currentType = field.Type;
			else
				return null;
		}

		return currentType;
	}

	/// <summary>
	/// Known BindableProperties that default to TwoWay binding mode.
	/// </summary>
	static readonly HashSet<string> TwoWayBindableProperties = new HashSet<string>
	{
		"global::Microsoft.Maui.Controls.Entry.TextProperty",
		"global::Microsoft.Maui.Controls.Editor.TextProperty",
		"global::Microsoft.Maui.Controls.SearchBar.TextProperty",
		"global::Microsoft.Maui.Controls.InputView.TextProperty",
		"global::Microsoft.Maui.Controls.DatePicker.DateProperty",
		"global::Microsoft.Maui.Controls.TimePicker.TimeProperty",
		"global::Microsoft.Maui.Controls.Picker.SelectedIndexProperty",
		"global::Microsoft.Maui.Controls.Picker.SelectedItemProperty",
		"global::Microsoft.Maui.Controls.Slider.ValueProperty",
		"global::Microsoft.Maui.Controls.Stepper.ValueProperty",
		"global::Microsoft.Maui.Controls.Switch.IsToggledProperty",
		"global::Microsoft.Maui.Controls.CheckBox.IsCheckedProperty",
		"global::Microsoft.Maui.Controls.RadioButton.IsCheckedProperty",
		"global::Microsoft.Maui.Controls.ListView.SelectedItemProperty",
		"global::Microsoft.Maui.Controls.CollectionView.SelectedItemProperty",
		"global::Microsoft.Maui.Controls.CollectionView.SelectedItemsProperty",
		"global::Microsoft.Maui.Controls.MultiPage<TPage>.CurrentPageProperty",
	};

	/// <summary>
	/// Checks if the BindableProperty defaults to TwoWay binding mode.
	/// </summary>
	static bool IsTwoWayByDefault(IFieldSymbol bpFieldSymbol)
	{
		var fullName = $"{bpFieldSymbol.ContainingType.ToFQDisplayString()}.{bpFieldSymbol.Name}";
		return TwoWayBindableProperties.Contains(fullName);
	}

	/// <summary>
	/// Checks if the terminal property in a C# expression chain is writable (has a public setter).
	/// For example, "Name" is writable if Name has a public set accessor, but "ReadOnlyProp" is not
	/// if it only has a getter (expression-bodied or getter-only property).
	/// Returns false for complex expressions that are not simple property chains.
	/// </summary>
	static bool IsExpressionWritable(string expression, ITypeSymbol dataType, SourceGenContext? context)
	{
		if (string.IsNullOrWhiteSpace(expression))
			return false;

		var expr = expression.Trim();

		// Strip leading dot prefix (e.g., ".Name" → "Name"), but avoid the ".." range operator
		if (expr.StartsWith(".", StringComparison.Ordinal) &&
			!expr.StartsWith("..", StringComparison.Ordinal))
			expr = expr.Substring(1);

		// Strip "BindingContext." prefix only when it refers to BindableObject.BindingContext,
		// not a user-defined member with the same name (e.g., a POCO with `public Person BindingContext;`)
		if (expr.StartsWith("BindingContext.", StringComparison.Ordinal)
			&& !dataType.GetAllMembers("BindingContext", context).Any())
			expr = expr.Substring("BindingContext.".Length);

		if (string.IsNullOrEmpty(expr))
			return false;

		// Walk the dot-separated property chain (also handle ?. null-conditional access)
		var parts = expr.Replace("?.", ".").Split('.');
		var currentType = dataType;
		IPropertySymbol? lastProperty = null;

		foreach (var part in parts)
		{
			var memberName = part.Trim().TrimEnd('!');

			// If it contains parens, operators, or special chars, it's not a simple property chain
			if (memberName.Contains('(') || memberName.Contains(' ') || memberName.Contains('[') || string.IsNullOrEmpty(memberName))
				return false;

			var member = currentType.GetAllMembers(memberName, context).FirstOrDefault();
			if (member is IPropertySymbol prop)
			{
				lastProperty = prop;
				currentType = prop.Type;
			}
			else if (member is IFieldSymbol field)
			{
				if (field.IsReadOnly)
					return false;
				lastProperty = null;
				currentType = field.Type;
			}
			else
			{
				return false;
			}
		}

		// Check if the terminal property has a public, non-init setter
		if (lastProperty is not null)
		{
			return lastProperty.SetMethod is not null
				&& lastProperty.SetMethod.DeclaredAccessibility == Accessibility.Public
				&& !lastProperty.SetMethod.IsInitOnly;
		}

		// For fields, assume writable
		return true;
	}

	sealed class ExtensionPropertyAccessors
	{
		public IMethodSymbol Setter { get; set; } = null!;
		public ITypeSymbol PropertyType { get; set; } = null!;
		public ITypeSymbol ReceiverType { get; set; } = null!;
	}

	//<Label local:LabelExtensions.MyTag="..."/> -> owner is local:LabelExtensions, member is MyTag
	static bool TryGetQualifiedOwner(XmlName propertyName, SourceGenContext context, out INamedTypeSymbol? owner, out string memberName)
	{
		owner = null;
		memberName = propertyName.LocalName;
		var dotIdx = memberName.IndexOf('.');
		if (dotIdx <= 0)
			return false;

		var typename = memberName.Substring(0, dotIdx);
		memberName = memberName.Substring(dotIdx + 1);
		owner = new XmlType(propertyName.NamespaceURI, typename, null).GetTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache);
		return owner != null;
	}

	/// <summary>
	/// Resolves the <c>set_Name</c> implementation method of a C# extension property declared by
	/// <paramref name="container"/> and applicable to <paramref name="targetType"/>.
	/// See <see cref="ExtensionPropertyConventions"/> for the rules shared with the runtime inflator and XamlC.
	/// </summary>
	static ExtensionPropertyAccessors? ResolveExtensionProperty(INamedTypeSymbol container, ITypeSymbol targetType, string propertyName, SourceGenContext context, IXmlLineInfo lineInfo, out bool reported, bool report = true)
	{
		reported = false;

		if (!container.IsStatic || container.IsGenericType)
			return null;

		if (!DeclaresExtensionProperty(container, propertyName))
			return null;

		var setterName = ExtensionPropertyConventions.SetterName(propertyName);
		List<IMethodSymbol>? setters = null;

		foreach (var method in container.GetMembers(setterName).OfType<IMethodSymbol>())
		{
			//only assignment is supported, so the setter alone defines the receiver and the value type
			if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public || method.IsGenericMethod)
				continue;
			if (!method.ReturnsVoid || method.Parameters.Length != 2)
				continue;

			var receiver = method.Parameters[0];
			if (receiver.RefKind != RefKind.None || !IsReceiverApplicable(receiver.Type, targetType, context))
				continue;

			(setters ??= []).Add(method);
		}

		var setter = MostSpecific(setters, context);
		if (setter == null || !context.Compilation.IsSymbolAccessibleWithin(container, context.RootType))
		{
			reported = report && ReportExtensionPropertyResolution(propertyName, container, targetType, context, lineInfo);
			return null;
		}

		return new ExtensionPropertyAccessors
		{
			Setter = setter,
			PropertyType = setter.Parameters[1].Type,
			ReceiverType = setter.Parameters[0].Type,
		};
	}

	/// <summary>
	/// The compiler nests an unspeakable "extension declaration" type in the container for every extension
	/// block, and that type declares the extension properties. See <see cref="ExtensionPropertyConventions"/>.
	/// </summary>
	/// <remarks><paramref name="propertyName"/> is the extension property to look for, or <c>null</c> for any.</remarks>
	static bool DeclaresExtensionProperty(INamedTypeSymbol container, string? propertyName)
	{
		foreach (var nested in container.GetTypeMembers())
		{
			if (nested.CanBeReferencedByName && ExtensionPropertyConventions.IsSpeakable(nested.MetadataName))
				continue;
			if ((propertyName == null ? nested.GetMembers() : nested.GetMembers(propertyName)).OfType<IPropertySymbol>().Any())
				return true;
			if (DeclaresExtensionProperty(nested, propertyName))
				return true;
		}

		return false;
	}

	static bool ReportExtensionPropertyResolution(string propertyName, INamedTypeSymbol container, ITypeSymbol targetType, SourceGenContext context, IXmlLineInfo lineInfo)
	{
		var location = LocationCreate(context.ProjectItem.RelativePath!, lineInfo, propertyName);
		context.ReportDiagnostic(Diagnostic.Create(Descriptors.ExtensionPropertyResolution, location, propertyName, container.ToFQDisplayString(), targetType.ToFQDisplayString()));
		return true; //always reported: callers use the return value to flag the member as handled
	}

	static bool IsReceiverApplicable(ITypeSymbol receiverType, ITypeSymbol targetType, SourceGenContext context)
	{
		if (SymbolEqualityComparer.Default.Equals(receiverType, targetType))
			return true;
		if (targetType.InheritsFrom(receiverType, context))
			return true;
		return receiverType.TypeKind == TypeKind.Interface && targetType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, receiverType));
	}

	/// <summary>
	/// Same rule as the setter overload, applied across the containers a xmlns brings in scope: the one
	/// candidate whose receiver is at least as specific as every other one wins. Comparing every candidate
	/// with every other, rather than folding them pairwise, is what makes the outcome independent of the
	/// order the containers are enumerated in.
	/// </summary>
	static ExtensionPropertyAccessors? MostSpecific(List<ExtensionPropertyAccessors> candidates, SourceGenContext context)
	{
		if (candidates.Count == 1)
			return candidates[0];

		ExtensionPropertyAccessors? best = null;
		foreach (var candidate in candidates)
		{
			var isBest = true;
			foreach (var other in candidates)
			{
				if (ReferenceEquals(other, candidate))
					continue;
				if (!IsReceiverApplicable(other.ReceiverType, candidate.ReceiverType, context))
				{
					isBest = false;
					break;
				}
			}
			if (!isBest)
				continue;
			//two candidates for the same receiver, or none dominating the others, is not something to guess
			if (best != null)
				return null;
			best = candidate;
		}

		return best;
	}

	//the setter whose receiver type is more derived than every other applicable one wins, as it would in C#
	static IMethodSymbol? MostSpecific(List<IMethodSymbol>? candidates, SourceGenContext context)
	{
		if (candidates == null)
			return null;
		if (candidates.Count == 1)
			return candidates[0];

		IMethodSymbol? best = null;
		foreach (var candidate in candidates)
		{
			var candidateType = candidate.Parameters[0].Type;
			var isBest = true;
			foreach (var other in candidates)
			{
				if (ReferenceEquals(other, candidate))
					continue;
				if (!IsReceiverApplicable(other.Parameters[0].Type, candidateType, context))
				{
					isBest = false;
					break;
				}
			}
			if (!isBest)
				continue;
			if (best != null)
				return null; //ambiguous
			best = candidate;
		}

		return best;
	}

	//the containers a xmlns brings in scope. Enumerating a clr namespace is expensive enough to remember
	static INamedTypeSymbol[] GetExtensionContainersInScope(string xmlNamespace, SourceGenContext context)
	{
		context.extensionContainersInScope ??= [];
		if (context.extensionContainersInScope.TryGetValue(xmlNamespace, out var cached))
			return cached;

		if (!context.XmlnsCache.ClrNamespacesForXmlns.TryGetValue(xmlNamespace, out var clrNamespaces))
		{
			XmlnsHelper.ParseXmlns(xmlNamespace, out _, out var ns, out _, out _);
			clrNamespaces = ns == null || ns.StartsWith("http", StringComparison.Ordinal) ? [] : [ns];
		}

		var containers = new List<INamedTypeSymbol>();
		foreach (var clrNamespace in clrNamespaces)
		{
			var namespaceSymbol = ResolveNamespace(context.Compilation.GlobalNamespace, clrNamespace);
			if (namespaceSymbol == null)
				continue;

			foreach (var type in namespaceSymbol.GetTypeMembers())
			{
				if (type.IsStatic && !type.IsGenericType && DeclaresExtensionProperty(type, propertyName: null))
					containers.Add(type);
			}
		}

		return context.extensionContainersInScope[xmlNamespace] = [.. containers];
	}

	static INamespaceSymbol? ResolveNamespace(INamespaceSymbol root, string clrNamespace)
	{
		var current = root;
		foreach (var part in clrNamespace.Split('.'))
		{
			current = current.GetNamespaceMembers().FirstOrDefault(ns => ns.Name == part);
			if (current == null)
				return null;
		}

		return current;
	}

	/// <summary>
	/// Resolves an unqualified name against the extension containers the element's default xmlns brings in
	/// scope, the way C# resolves an extension member against the namespaces a file imports.
	/// </summary>
	static bool CanSetScopedExtensionProperty(ILocalValue parentVar, XmlName propertyName, INode node, SourceGenContext context, out ExtensionPropertyAccessors? accessors, out bool reported)
	{
		accessors = null;
		reported = false;

		//unprefixed attributes carry no namespace, the scope is the default xmlns declared for the element
		if (propertyName.NamespaceURI != string.Empty || node.NamespaceResolver == null)
			return false;

		var xmlNamespace = node.NamespaceResolver.LookupNamespace(string.Empty);
		if (string.IsNullOrEmpty(xmlNamespace))
			return false;

		var lineInfo = (IXmlLineInfo)node;
		List<ExtensionPropertyAccessors>? candidates = null;

		foreach (var container in GetExtensionContainersInScope(xmlNamespace!, context))
		{
			if (!DeclaresExtensionProperty(container, propertyName.LocalName))
				continue;

			var candidate = ResolveExtensionProperty(container, parentVar.Type, propertyName.LocalName, context, lineInfo, out var candidateReported, report: false);
			if (candidateReported || candidate == null)
				continue;

			(candidates ??= []).Add(candidate);
		}

		if (candidates == null)
			return false;

		//C# picks the most specific receiver, and refuses to guess between unrelated ones
		accessors = MostSpecific(candidates, context);
		if (accessors == null)
		{
			var location = LocationCreate(context.ProjectItem.RelativePath!, lineInfo, propertyName.LocalName);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.ExtensionPropertyAmbiguous, location, propertyName.LocalName, xmlNamespace, parentVar.Type.ToFQDisplayString()));
			reported = true;
			return false;
		}

		var propertyType = accessors.PropertyType;

		if (node is ValueNode vn)
			return vn.CanConvertTo(propertyType, context);

		if (node is not ElementNode elementNode || !context.Variables.TryGetValue(elementNode, out var localVar))
			return false;

		return localVar.Type.InheritsFrom(propertyType, context)
			|| (propertyType.IsInterface() && localVar.Type.Implements(propertyType))
			|| propertyType.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default)
			|| context.Compilation.HasImplicitConversion(localVar.Type, propertyType)
			|| HasDoubleImplicitConversion(localVar.Type, propertyType, context, out _)
			|| HasExplicitConversion(localVar.Type, propertyType, context)
			|| localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default);
	}

	//`reported` tells the caller a diagnostic was already emitted, so it must not also report an unresolved member
	static bool CanSetExtensionProperty(ILocalValue parentVar, XmlName propertyName, INode node, SourceGenContext context, out ExtensionPropertyAccessors? accessors, out bool reported)
	{
		accessors = null;
		reported = false;
		if (!TryGetQualifiedOwner(propertyName, context, out var container, out var memberName))
			return false;

		var lineInfo = (IXmlLineInfo)node;
		accessors = ResolveExtensionProperty(container!, parentVar.Type, memberName, context, lineInfo, out reported);
		if (accessors == null)
			return false;

		var propertyType = accessors.PropertyType;

		if (node is ValueNode vn)
			return vn.CanConvertTo(propertyType, context);

		if (node is not ElementNode elementNode || !context.Variables.TryGetValue(elementNode, out var localVar))
			return false;

		//mirrors CanSet: the same conversions must be accepted for an extension property
		if (localVar.Type.InheritsFrom(propertyType, context))
			return true;
		if (propertyType.IsInterface() && localVar.Type.Implements(propertyType))
			return true;
		if (propertyType.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return true;
		if (context.Compilation.HasImplicitConversion(localVar.Type, propertyType))
			return true;
		if (HasDoubleImplicitConversion(localVar.Type, propertyType, context, out _))
			return true;
		if (HasExplicitConversion(localVar.Type, propertyType, context))
			return true;
		//a value typed as object is cast at runtime, exactly like an ordinary property assignment
		if (localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return true;

		return false;
	}

	/// <summary>
	/// An extension container is only ever named for its extension properties, so when none applies the name
	/// must not fall back to a member of the target itself. Any other explicitly named owner keeps resolving
	/// the way it always did.
	/// </summary>
	static bool ReportUnresolvedExtensionMember(ILocalValue parentVar, XmlName propertyName, INode node, SourceGenContext context)
	{
		if (!TryGetQualifiedOwner(propertyName, context, out var owner, out var memberName))
			return false;
		if (!owner!.IsStatic || owner.IsGenericType || !DeclaresExtensionProperty(owner, propertyName: null))
			return false;

		return ReportExtensionPropertyResolution(memberName, owner, parentVar.Type, context, (IXmlLineInfo)node);
	}

	//emits LabelExtensions.set_MyTag(label, "value"), the implementation method the C# compiler emits for the extension property
	static void SetExtensionProperty(IndentedTextWriter writer, ILocalValue parentVar, ExtensionPropertyAccessors accessors, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		var setter = accessors.Setter;
		var setterAccessor = $"{setter.ContainingType.ToFQDisplayString()}.{setter.Name}";
		var receiverCast = SymbolEqualityComparer.Default.Equals(parentVar.Type, accessors.ReceiverType) ? string.Empty : $"({accessors.ReceiverType.ToFQDisplayString()})";

		if (node is ValueNode valueNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var valueString = valueNode.ConvertTo(accessors.PropertyType, writer, context, parentVar);
				writer.WriteLine($"{setterAccessor}({receiverCast}{parentVar.ValueAccessor}, {valueString});");
			}
		}
		else if (node is ElementNode elementNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var localVar = getNodeValue(elementNode, context.Compilation.ObjectType);
				//mirrors Set: an intermediate cast is needed when the conversion goes through an operator
				var intermediateCast = HasDoubleImplicitConversion(localVar.Type, accessors.PropertyType, context, out var conv) ? $"({conv!.ReturnType.ToFQDisplayString()})" : string.Empty;
				var cast = context.Compilation.HasImplicitConversion(localVar.Type, accessors.PropertyType) && intermediateCast.Length == 0 ? string.Empty : $"({accessors.PropertyType.ToFQDisplayString()})";
				writer.WriteLine($"{setterAccessor}({receiverCast}{parentVar.ValueAccessor}, {cast}{intermediateCast}{localVar.ValueAccessor});");
			}
		}
	}
}
