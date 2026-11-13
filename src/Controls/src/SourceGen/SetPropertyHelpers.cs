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
		if (!asCollectionItem && !attached && !hasBindableProperty && CanSetScopedExtensionProperty(parentVar, propertyName, valueNode, context, out var scopedAccessors, out scopedReported))
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
	/// Creates the resource on demand using a lambda, or a stable local function when replacing an entry.
	/// </summary>
	public static void AddLazyResourceToResourceDictionary(IndentedTextWriter writer, ILocalValue parentVar, ElementNode node, SourceGenContext context, string? replacementFactoryName = null)
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

		string resourceKeyExpression;
		if (isImplicitStyle)
		{
			// Get TargetType from Style
			var targetTypeExpr = GetStyleTargetTypeExpression(node, context);
			if (targetTypeExpr == null)
			{
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ""), "Implicit style requires a TargetType"));
				return;
			}

			resourceKeyExpression = targetTypeExpr;
		}
		else
		{
			resourceKeyExpression = $"\"{CSharpExpressionHelpers.EscapeForString(key!)}\"";
		}

		if (replacementFactoryName is not null)
		{
			// A stable local function avoids anonymous-lambda identity churn across EnC deltas.
			var returnType = isImplicitStyle ? "global::Microsoft.Maui.Controls.Style" : "object";
			writer.WriteLine($"{returnType} {replacementFactoryName}()");
		}
		else
			writer.WriteLine($"{parentVar.ValueAccessor}.AddFactory({resourceKeyExpression}, () =>");

		using (PrePost.NewBlock(writer, begin: "{", end: replacementFactoryName is not null
			? "}" : $"}}, shared: {shared.ToString().ToLowerInvariant()});"))
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

		if (replacementFactoryName is not null)
		{
			var keyExpression = isImplicitStyle ? $"{resourceKeyExpression}.FullName!" : resourceKeyExpression;
			writer.WriteLine($"{parentVar.ValueAccessor}.Remove({keyExpression});");
			writer.WriteLine($"{parentVar.ValueAccessor}.AddFactory({resourceKeyExpression}, {replacementFactoryName}, shared: {shared.ToString().ToLowerInvariant()});");
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
			// Check for method group reference in lambda body (missing parentheses)
			var methodGroupRef = CSharpExpressionHelpers.DetectLambdaMethodGroupReference(expression.Code);
			if (methodGroupRef != null)
			{
				var (methodGroup, suggestion) = methodGroupRef.Value;
				var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.LambdaMethodGroupReference, location, methodGroup, suggestion));
				return;
			}

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
		var analysis = ExpressionAnalyzer.Analyze(expression.Code, "__source", dataTypeSymbol, context.RootType, context.Compilation);

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
			MemberResolver.IsSimpleIdentifier(expression.Code) &&
			!resolution.ResolvesToStaticType)
		{
			var neitherLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberNotFound, neitherLocation, resolution.RootIdentifier, context.RootType?.Name ?? "this", dataTypeSymbol.Name));
			return true; // Handled (with error)
		}

		// Validate identifiers inside interpolated string holes
		if (expression.Code.StartsWith("$\"", StringComparison.Ordinal) || expression.Code.StartsWith("$@\"", StringComparison.Ordinal))
		{
			var interpolatedIds = CSharpExpressionHelpers.ExtractInterpolatedStringIdentifierReferences(expression.Code);
			foreach (var idReference in interpolatedIds)
			{
				var id = idReference.Identifier;
				var idResolution = MemberResolver.Resolve(id, context.RootType, dataTypeSymbol, context.Compilation);
				if (idResolution.Location == MemberLocation.Neither &&
					!MemberResolver.HasMember(context.RootType, id, includeMethods: true) &&
					!MemberResolver.HasMember(dataTypeSymbol, id, includeMethods: true) &&
					!MemberResolver.ResolvesToType(context.Compilation, idReference.TypeReferenceCandidate, MemberResolver.GetContainingNamespace(context.RootType)))
				{
					var idLocation = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression.Code);
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberNotFound, idLocation, id, context.RootType?.Name ?? "this", dataTypeSymbol.Name));
					return true; // Handled (with error)
				}
			}
		}

		if (resolution.Location == MemberLocation.Neither &&
			resolution.ResolvesToStaticType &&
			MemberResolver.IsSimpleIdentifier(expression.Code))
		{
			// Pure static member access should be emitted as a SetValue expression, not a binding.
			return false;
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

		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, rootType, compilation);
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
		var writeInfo = GetExpressionWriteInfo(expression, dataTypeSymbol, context: null);
		if (analysis.IsSettable && writeInfo.Kind == ExpressionWriteKind.DirectAssignment)
			writer.WriteLine($"(__source, __value) => {analysis.TransformedExpression} = __value,");
		else if (analysis.IsSettable && writeInfo.Kind == ExpressionWriteKind.StructCopyWriteback)
			WriteStructCopyWritebackSetter(writer, writeInfo.Chain!);
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
		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, context.RootType, context.Compilation);
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
			var writeInfo = GetExpressionWriteInfo(expression, dataTypeSymbol, context);
			if (analysis.IsSettable && writeInfo.Kind == ExpressionWriteKind.DirectAssignment)
			{
				writer.WriteLine($"(__source, __value) => {analysis.TransformedExpression} = __value,");
			}
			else if (analysis.IsSettable && writeInfo.Kind == ExpressionWriteKind.StructCopyWriteback)
			{
				WriteStructCopyWritebackSetter(writer, writeInfo.Chain!);
			}
			else
			{
				writer.WriteLine($"null,");
				// Emit diagnostics when binding a complex expression to a TwoWay property
				if (IsTwoWayByDefault(bpFieldSymbol))
				{
					var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, expression);
					context.ReportDiagnostic(Diagnostic.Create(
						writeInfo.Kind == ExpressionWriteKind.StructIntermediateNotSettable
							? Descriptors.StructIntermediateNotSettable
							: Descriptors.ExpressionNotSettable,
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
	/// Describes how a C# expression can be used as a binding setter.
	/// </summary>
	enum ExpressionWriteKind
	{
		/// <summary>Cannot generate a setter (terminal not writable, or not a property chain).</summary>
		NotWritable,
		/// <summary>Simple direct assignment: __source.Prop = __value.</summary>
		DirectAssignment,
		/// <summary>Needs copy-modify-writeback pattern for struct intermediates.</summary>
		StructCopyWriteback,
		/// <summary>Has struct intermediate but the intermediate property has no setter.</summary>
		StructIntermediateNotSettable,
	}

	readonly struct ExpressionWriteInfo
	{
		public ExpressionWriteKind Kind { get; init; }
		/// <summary>
		/// For StructCopyWriteback: the chain of property names from root to terminal.
		/// Each bool indicates whether that property is a value-type intermediate requiring writeback.
		/// </summary>
		public (string Name, bool IsStructIntermediate)[]? Chain { get; init; }
	}

	/// <summary>
	/// Analyzes whether a C# expression property chain can generate a setter,
	/// and if so, what kind of setter is needed.
	/// Handles value type (struct) intermediates that require copy-modify-writeback.
	/// </summary>
	static ExpressionWriteInfo GetExpressionWriteInfo(string expression, ITypeSymbol dataType, SourceGenContext? context)
	{
		var notWritable = new ExpressionWriteInfo { Kind = ExpressionWriteKind.NotWritable };

		if (string.IsNullOrWhiteSpace(expression))
			return notWritable;

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
			return notWritable;

		// Walk the dot-separated property chain (also handle ?. null-conditional access)
		var parts = expr.Replace("?.", ".").Split('.');
		var currentType = dataType;
		IPropertySymbol? lastProperty = null;
		var chain = new (string Name, bool IsStructIntermediate)[parts.Length];
		bool hasStructIntermediate = false;
		bool hasUnsettableStructIntermediate = false;

		for (int i = 0; i < parts.Length; i++)
		{
			var memberName = parts[i].Trim().TrimEnd('!');

			// If it contains parens, operators, or special chars, it's not a simple property chain
			if (memberName.Contains('(') || memberName.Contains(' ') || memberName.Contains('[') || string.IsNullOrEmpty(memberName))
				return notWritable;

			var member = currentType.GetAllMembers(memberName, context).FirstOrDefault();
			if (member is IPropertySymbol prop)
			{
				lastProperty = prop;
				currentType = prop.Type;

				bool isStructIntermediate = i < parts.Length - 1 && currentType.IsValueType;
				chain[i] = (memberName, isStructIntermediate);

				if (isStructIntermediate)
				{
					hasStructIntermediate = true;
					// Check if this intermediate property has a setter (needed for writeback)
					bool intermediateSettable = prop.SetMethod is not null
						&& prop.SetMethod.DeclaredAccessibility == Accessibility.Public
						&& !prop.SetMethod.IsInitOnly;
					if (!intermediateSettable)
						hasUnsettableStructIntermediate = true;
				}
			}
			else if (member is IFieldSymbol field)
			{
				if (field.IsReadOnly)
					return notWritable;
				lastProperty = null;
				chain[i] = (memberName, false); // Fields don't need writeback
				currentType = field.Type;
			}
			else
			{
				return notWritable;
			}
		}

		// Check if the terminal property has a public, non-init setter
		if (lastProperty is not null)
		{
			bool terminalWritable = lastProperty.SetMethod is not null
				&& lastProperty.SetMethod.DeclaredAccessibility == Accessibility.Public
				&& !lastProperty.SetMethod.IsInitOnly;
			if (!terminalWritable)
				return notWritable;
		}

		if (hasUnsettableStructIntermediate)
			return new ExpressionWriteInfo { Kind = ExpressionWriteKind.StructIntermediateNotSettable };

		if (hasStructIntermediate)
			return new ExpressionWriteInfo { Kind = ExpressionWriteKind.StructCopyWriteback, Chain = chain };

		return new ExpressionWriteInfo { Kind = ExpressionWriteKind.DirectAssignment };
	}

	/// <summary>
	/// Generates a copy-modify-writeback setter for expressions with value type (struct) intermediates.
	/// For example, {Margin.Top} generates:
	/// <c>(__source, __value) =&gt; { var __t0 = __source.Margin; __t0.Top = __value; __source.Margin = __t0; },</c>
	/// </summary>
	static void WriteStructCopyWritebackSetter(IndentedTextWriter writer, (string Name, bool IsStructIntermediate)[] chain)
	{
		writer.Write("(__source, __value) => { ");

		string currentAccessor = "__source";
		var structStack = new System.Collections.Generic.List<(string TempVar, string ParentAccessor, string PropertyName)>();
		int tempIndex = 0;

		// Process non-terminal parts
		for (int i = 0; i < chain.Length - 1; i++)
		{
			var (name, isStruct) = chain[i];
			if (isStruct)
			{
				var tempVar = $"__t{tempIndex++}";
				writer.Write($"var {tempVar} = {currentAccessor}.{name}; ");
				structStack.Add((tempVar, currentAccessor, name));
				currentAccessor = tempVar;
			}
			else
			{
				currentAccessor = $"{currentAccessor}.{name}";
			}
		}

		// Terminal assignment
		writer.Write($"{currentAccessor}.{chain[chain.Length - 1].Name} = __value; ");

		// Write back struct copies in reverse order
		for (int i = structStack.Count - 1; i >= 0; i--)
		{
			var (tempVar, parent, propName) = structStack[i];
			writer.Write($"{parent}.{propName} = {tempVar}; ");
		}

		writer.WriteLine("},");
	}

	sealed class ExtensionPropertyAccessors
	{
		public IMethodSymbol Setter { get; set; } = null!;
		public IPropertySymbol Property { get; set; } = null!;
		public ITypeSymbol PropertyType => Property.Type;
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
		return new XmlType(propertyName.NamespaceURI, typename, null).TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out owner) && owner != null;
	}

	/// <summary>
	/// Resolves the <c>set_Name</c> implementation method of a C# extension property declared by
	/// <paramref name="container"/> and applicable to <paramref name="targetType"/>.
	/// See <see cref="ExtensionPropertyConventions"/>.
	/// </summary>
	static ExtensionPropertyAccessors? ResolveExtensionProperty(INamedTypeSymbol container, ITypeSymbol targetType, string propertyName, SourceGenContext context, IXmlLineInfo lineInfo, out bool reported, bool report = true)
	{
		reported = false;

		if (!DeclaresExtensionProperty(container, propertyName))
			return null;

		var candidates = new List<ExtensionPropertyAccessors>();
		CollectExtensionSetters(container, targetType, propertyName, context, candidates);

		var accessors = MostSpecific(candidates, context);
		if (accessors == null)
			reported = report && ReportExtensionPropertyResolution(propertyName, container, targetType, context, lineInfo);

		return accessors;
	}

	/// <summary>
	/// Adds, for every writable instance extension property named <paramref name="propertyName"/> that
	/// <paramref name="container"/> declares for a receiver applicable to <paramref name="targetType"/>, the
	/// implementation method the compiler emitted for its setter. Only the setter matching the declaration's
	/// receiver and value type qualifies, so an ordinary static method that merely shares the
	/// <c>set_Name</c> name is never mistaken for an accessor.
	/// </summary>
	static void CollectExtensionSetters(INamedTypeSymbol container, ITypeSymbol targetType, string propertyName, SourceGenContext context, List<ExtensionPropertyAccessors> candidates)
	{
		if (!container.IsStatic || container.IsGenericType)
			return;
		if (context.RootType != null && !context.Compilation.IsSymbolAccessibleWithin(container, context.RootType))
			return;

		var setterName = ExtensionPropertyConventions.SetterName(propertyName);
		foreach (var (property, receiver) in GetExtensionPropertyDeclarations(container, propertyName))
		{
			if (property.IsStatic || property.SetMethod == null || receiver == null)
				continue;
			if (!IsReceiverApplicable(receiver, targetType, context))
				continue;

			foreach (var method in container.GetMembers(setterName).OfType<IMethodSymbol>())
			{
				if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public || method.IsGenericMethod)
					continue;
				if (!method.ReturnsVoid || method.Parameters.Length != 2 || method.Parameters[0].RefKind != RefKind.None)
					continue;
				if (!SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, receiver)
					|| !SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, property.Type))
					continue;
				if (candidates.Any(c => SymbolEqualityComparer.Default.Equals(c.Setter, method)))
					break;

				candidates.Add(new ExtensionPropertyAccessors { Setter = method, Property = property, ReceiverType = receiver });
				break;
			}
		}
	}

	/// <summary>
	/// The compiler nests an unspeakable "extension declaration" type in the container for every extension
	/// block, and that type declares the extension properties. See <see cref="ExtensionPropertyConventions"/>.
	/// </summary>
	/// <remarks><paramref name="propertyName"/> is the extension property to look for, or <c>null</c> for any.</remarks>
	static bool DeclaresExtensionProperty(INamedTypeSymbol container, string? propertyName)
		=> GetExtensionPropertyDeclarations(container, propertyName).Any();

	/// <summary>
	/// The extension properties <paramref name="container"/> declares, with the receiver type of their
	/// extension block, or <c>null</c> when the receiver cannot be determined.
	/// </summary>
	static IEnumerable<(IPropertySymbol property, ITypeSymbol? receiver)> GetExtensionPropertyDeclarations(INamedTypeSymbol container, string? propertyName)
	{
		foreach (var nested in container.GetTypeMembers())
		{
			if (nested.CanBeReferencedByName && ExtensionPropertyConventions.IsSpeakable(nested.MetadataName))
				continue;

			var blockReceiver = GetExtensionBlockReceiver(nested);
			foreach (var property in (propertyName == null ? nested.GetMembers() : nested.GetMembers(propertyName)).OfType<IPropertySymbol>())
				yield return (property, blockReceiver ?? GetMarkerReceiver(property));

			foreach (var declaration in GetExtensionPropertyDeclarations(nested, propertyName))
				yield return declaration;
		}
	}

	//INamedTypeSymbol.ExtensionParameter is newer than the Microsoft.CodeAnalysis version this generator is built
	//against, but it is there when running in a compiler that understands extension blocks
	static readonly System.Reflection.PropertyInfo? s_extensionParameter = typeof(INamedTypeSymbol).GetProperty("ExtensionParameter");

	static ITypeSymbol? GetExtensionBlockReceiver(INamedTypeSymbol extensionBlock)
		=> (s_extensionParameter?.GetValue(extensionBlock) as IParameterSymbol)?.Type;

	//Older compilers see the metadata shape instead: the property carries [ExtensionMarker("<M>$...")] naming a
	//sibling marker type, whose `<Extension>$` method takes the receiver as its only parameter
	static ITypeSymbol? GetMarkerReceiver(IPropertySymbol property)
	{
		var markerName = property.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.Name == "ExtensionMarkerAttribute" && a.ConstructorArguments.Length == 1)
			?.ConstructorArguments[0].Value as string;
		if (markerName == null)
			return null;

		var marker = property.ContainingType.GetTypeMembers().FirstOrDefault(t => t.MetadataName == markerName);
		var markerMethod = marker?.GetMembers("<Extension>$").OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 1);
		return markerMethod?.Parameters[0].Type;
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
		if (receiverType.SpecialType == SpecialType.System_Object)
			return true;
		if (targetType.InheritsFrom(receiverType, context))
			return true;
		return receiverType.TypeKind == TypeKind.Interface && targetType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, receiverType));
	}

	/// <summary>
	/// The one candidate whose receiver is at least as specific as every other one wins, as it would in C#.
	/// Comparing every candidate with every other, rather than folding them pairwise, is what makes the
	/// outcome independent of the order candidates are enumerated in.
	/// </summary>
	static ExtensionPropertyAccessors? MostSpecific(List<ExtensionPropertyAccessors> candidates, SourceGenContext context)
	{
		if (candidates.Count == 0)
			return null;
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

	/// <summary>
	/// The extension containers a xmlns brings in scope. Each [XmlnsDefinition] only brings in the namespace
	/// of the assembly it names, so a referenced assembly reusing that namespace name contributes nothing.
	/// Enumerating a clr namespace is expensive enough to remember.
	/// </summary>
	static INamedTypeSymbol[] GetExtensionContainersInScope(string xmlNamespace, SourceGenContext context)
	{
		context.extensionContainersInScope ??= [];
		if (context.extensionContainersInScope.TryGetValue(xmlNamespace, out var cached))
			return cached;

		var mappings = context.XmlnsCache.XmlnsDefinitions
			.Where(d => d.XmlNamespace == xmlNamespace && d.Target != null && !d.Target.StartsWith("http", StringComparison.Ordinal))
			.Select(d => (clrNamespace: d.Target, assemblyName: d.AssemblyName))
			.ToList();
		if (mappings.Count == 0)
		{
			XmlnsHelper.ParseXmlns(xmlNamespace, out _, out var ns, out var asm, out _);
			if (ns != null && !ns.StartsWith("http", StringComparison.Ordinal))
				//a clr-namespace without an assembly refers to the assembly declaring the XAML
				mappings.Add((ns, string.IsNullOrEmpty(asm) ? context.Compilation.Assembly.Identity.Name : asm));
		}

		var containers = new List<INamedTypeSymbol>();
		foreach (var (clrNamespace, assemblyName) in mappings.Distinct())
		{
			foreach (var assembly in GetAssemblies(assemblyName, context))
			{
				var namespaceSymbol = ResolveNamespace(assembly.GlobalNamespace, clrNamespace);
				if (namespaceSymbol == null)
					continue;

				foreach (var type in namespaceSymbol.GetTypeMembers())
				{
					if (type.IsStatic && !type.IsGenericType && !containers.Contains(type, SymbolEqualityComparer.Default) && DeclaresExtensionProperty(type, propertyName: null))
						containers.Add(type);
				}
			}
		}

		return context.extensionContainersInScope[xmlNamespace] = [.. containers];
	}

	static IEnumerable<IAssemblySymbol> GetAssemblies(string? assemblyName, SourceGenContext context)
	{
		var simpleName = string.IsNullOrEmpty(assemblyName) ? null : new System.Reflection.AssemblyName(assemblyName).Name;
		if (simpleName == null || context.Compilation.Assembly.Identity.Name == simpleName)
			yield return context.Compilation.Assembly;
		foreach (var reference in context.Compilation.References)
		{
			if (context.Compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly
				&& (simpleName == null || assembly.Identity.Name == simpleName))
				yield return assembly;
		}
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
	/// An extension property never hides an instance member: C# binds a name the target declares to that
	/// member, even when it cannot be assigned, and only falls back to extension members otherwise.
	/// </summary>
	static bool HasAccessibleInstanceMember(ITypeSymbol targetType, string name, SourceGenContext context)
	{
		bool IsAccessible(ISymbol symbol) => context.RootType == null || context.Compilation.IsSymbolAccessibleWithin(symbol, context.RootType);

		//interface declarations are not members of a class: an explicitly implemented property hides nothing
		var isInterface = targetType.TypeKind == TypeKind.Interface;
		return targetType.GetAllMembers(name, context).Any(m =>
			m is IPropertySymbol or IFieldSymbol
			&& !m.IsStatic
			&& (isInterface || m.ContainingType.TypeKind != TypeKind.Interface)
			&& IsAccessible(m));
	}

	/// <summary>
	/// Resolves an unqualified name against the extension containers the element's default xmlns brings in
	/// scope, the way C# resolves an extension member against the namespaces a file imports. Every applicable
	/// setter of every container in scope competes, so that an ambiguity inside one container is not hidden by
	/// a less specific candidate from another.
	/// </summary>
	static ExtensionPropertyAccessors? ResolveScopedExtensionProperty(ITypeSymbol targetType, XmlName propertyName, INode node, SourceGenContext context, out string? xmlNamespace, out bool ambiguous)
	{
		xmlNamespace = null;
		ambiguous = false;

		//unprefixed attributes carry no namespace, the scope is the default xmlns declared for the element
		if (propertyName.NamespaceURI != string.Empty || node.NamespaceResolver == null)
			return null;
		if (HasAccessibleInstanceMember(targetType, propertyName.LocalName, context))
			return null;

		xmlNamespace = node.NamespaceResolver.LookupNamespace(string.Empty);
		if (string.IsNullOrEmpty(xmlNamespace))
			return null;

		var candidates = new List<ExtensionPropertyAccessors>();
		foreach (var container in GetExtensionContainersInScope(xmlNamespace!, context))
			CollectExtensionSetters(container, targetType, propertyName.LocalName, context, candidates);

		var accessors = MostSpecific(candidates, context);
		ambiguous = accessors == null && candidates.Count > 1;
		return accessors;
	}

	static bool CanSetScopedExtensionProperty(ILocalValue parentVar, XmlName propertyName, INode node, SourceGenContext context, out ExtensionPropertyAccessors? accessors, out bool reported)
	{
		reported = false;
		accessors = ResolveScopedExtensionProperty(parentVar.Type, propertyName, node, context, out var xmlNamespace, out var ambiguous);

		//C# picks the most specific receiver, and refuses to guess between unrelated ones
		if (ambiguous)
		{
			var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, propertyName.LocalName);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.ExtensionPropertyAmbiguous, location, propertyName.LocalName, xmlNamespace, parentVar.Type.ToFQDisplayString()));
			reported = true;
			return false;
		}

		return accessors != null && CanAssignExtensionProperty(accessors, node, context);
	}

	/// <summary>
	/// The extension property an attribute assigns, if any, without reporting anything. Lets markup
	/// extensions that are evaluated before the assignment learn the type they have to provide.
	/// </summary>
	internal static IPropertySymbol? FindExtensionProperty(ITypeSymbol targetType, XmlName propertyName, INode node, SourceGenContext context)
	{
		if (TryGetQualifiedOwner(propertyName, context, out var container, out var memberName))
			return ResolveExtensionProperty(container!, targetType, memberName, context, (IXmlLineInfo)node, out _, report: false)?.Property;

		return ResolveScopedExtensionProperty(targetType, propertyName, node, context, out _, out _)?.Property;
	}

	//`reported` tells the caller a diagnostic was already emitted, so it must not also report an unresolved member
	static bool CanSetExtensionProperty(ILocalValue parentVar, XmlName propertyName, INode node, SourceGenContext context, out ExtensionPropertyAccessors? accessors, out bool reported)
	{
		accessors = null;
		reported = false;
		if (!TryGetQualifiedOwner(propertyName, context, out var container, out var memberName))
			return false;

		accessors = ResolveExtensionProperty(container!, parentVar.Type, memberName, context, (IXmlLineInfo)node, out reported);
		return accessors != null && CanAssignExtensionProperty(accessors, node, context);
	}

	//mirrors CanSet: the same conversions must be accepted for an extension property
	static bool CanAssignExtensionProperty(ExtensionPropertyAccessors accessors, INode node, SourceGenContext context)
	{
		var propertyType = accessors.PropertyType;

		if (node is ValueNode vn)
			return vn.CanConvertTo(accessors.Property, context);

		if (node is not ElementNode elementNode || !context.Variables.TryGetValue(elementNode, out var localVar))
			return false;

		return localVar.Type.InheritsFrom(propertyType, context)
			|| (propertyType.IsInterface() && localVar.Type.Implements(propertyType))
			|| propertyType.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default)
			|| context.Compilation.HasImplicitConversion(localVar.Type, propertyType)
			|| HasDoubleImplicitConversion(localVar.Type, propertyType, context, out _)
			|| HasExplicitConversion(localVar.Type, propertyType, context)
			//a value typed as object is cast at runtime, exactly like an ordinary property assignment
			|| localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default);
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
				var valueString = valueNode.ConvertTo(accessors.Property, writer, context, parentVar);
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
