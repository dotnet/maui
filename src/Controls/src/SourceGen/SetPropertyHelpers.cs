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

		// EventTrigger.Event property - generate AOT-safe code
		if (!asCollectionItem && CanSetEventTriggerEvent(parentVar, localName, valueNode, context))
		{
			SetEventTriggerEvent(writer, parentVar, valueNode, context);
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

		//POCO, set the property
		if (!asCollectionItem && CanSet(parentVar, localName, valueNode, context))
		{
			Set(writer, parentVar, localName, valueNode, context, getNodeValue);
			return;
		}

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
			writer.WriteLine($"{parentVar.ValueAccessor}[\"{key}\"] = {(getNodeValue(node, context.Compilation.ObjectType)).ValueAccessor};");
			return;
		}
		writer.WriteLine($"{parentVar.ValueAccessor}.Add({getNodeValue(node, context.Compilation.ObjectType).ValueAccessor});");
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

	static bool CanSetEventTriggerEvent(ILocalValue parentVar, string localName, INode valueNode, SourceGenContext context)
	{
		// Check if property is "Event" and value is a string
		if (localName != "Event" || valueNode is not ValueNode)
			return false;

		// Check if parent is the non-generic EventTrigger class
		// (We want to replace non-generic EventTrigger with generic EventTrigger<TBindable> for AOT)
		var eventTriggerType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.EventTrigger");
		if (eventTriggerType == null)
			return false;

		return SymbolEqualityComparer.Default.Equals(parentVar.Type, eventTriggerType);
	}

	static void SetEventTriggerEvent(IndentedTextWriter writer, ILocalValue parentVar, INode valueNode, SourceGenContext context)
	{
		// Generate AOT-safe EventTrigger code using EventTrigger.Create<T>() factory.
		// 
		// This method is only called when processing the Event property, which means
		// EventTriggerValueProvider.CanProvideValue returned true and no "new EventTrigger()"
		// was emitted. We emit the variable declaration with the factory call here.
		
		var eventName = (string)((ValueNode)valueNode).Value;

		// Find the target type by walking up the XAML tree from EventTrigger to the owning element
		ITypeSymbol? targetType = null;
		
		var node = valueNode.Parent;
		while (node != null)
		{
			if (node is ElementNode elementNode)
			{
				if (context.Variables.TryGetValue(node, out var elementVar))
				{
					var elementType = elementVar.Type;
					// Skip if this is the EventTrigger we're processing
					if (SymbolEqualityComparer.Default.Equals(elementType, parentVar.Type))
					{
						node = node.Parent;
						continue;
					}
					
					targetType = elementType;
					break;
				}
			}
			node = node.Parent;
		}

		if (targetType == null)
		{
			// Fallback: couldn't find target type, use reflection-based EventTrigger
			writer.WriteLine($"var {parentVar.ValueAccessor} = new global::Microsoft.Maui.Controls.EventTrigger();");
			writer.WriteLine($"{parentVar.ValueAccessor}.Event = \"{eventName}\";");
			return;
		}

		// Look up event on target type
		var eventSymbols = targetType.GetAllEvents(eventName, context).ToList();
		if (eventSymbols.Count == 0)
		{
			writer.WriteLine($"var {parentVar.ValueAccessor} = new global::Microsoft.Maui.Controls.EventTrigger();");
			writer.WriteLine($"{parentVar.ValueAccessor}.Event = \"{eventName}\";");
			return;
		}

		var eventSymbol = eventSymbols.First();
		var eventType = eventSymbol.Type;

		// Determine EventHandler signature
		var invoke = eventType.GetAllMethods("Invoke", context).FirstOrDefault();
		if (invoke == null || invoke.Parameters.Length != 2)
		{
			writer.WriteLine($"var {parentVar.ValueAccessor} = new global::Microsoft.Maui.Controls.EventTrigger();");
			writer.WriteLine($"{parentVar.ValueAccessor}.Event = \"{eventName}\";");
			return;
		}

		var eventArgsType = invoke.Parameters[1].Type;
		var isGenericEventHandler = !eventArgsType.Equals(
			context.Compilation.GetTypeByMetadataName("System.EventArgs"), 
			SymbolEqualityComparer.Default);

		// Generate EventTrigger.Create<T>() factory call
		var targetTypeName = targetType.ToFQDisplayString();
		
		if (isGenericEventHandler)
		{
			var eventArgsTypeName = eventArgsType.ToFQDisplayString();
			writer.WriteLine($"var {parentVar.ValueAccessor} = global::Microsoft.Maui.Controls.EventTrigger.Create<{targetTypeName}, {eventArgsTypeName}>(");
			writer.WriteLine($"    static (target, handler) => target.{eventName} += handler,");
			writer.WriteLine($"    static (target, handler) => target.{eventName} -= handler);");
		}
		else
		{
			writer.WriteLine($"var {parentVar.ValueAccessor} = global::Microsoft.Maui.Controls.EventTrigger.Create<{targetTypeName}>(");
			writer.WriteLine($"    static (target, handler) => target.{eventName} += handler,");
			writer.WriteLine($"    static (target, handler) => target.{eventName} -= handler);");
		}
		
		// Set the Event property for backward compatibility
		writer.WriteLine($"{parentVar.ValueAccessor}.Event = \"{eventName}\";");
	}

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
			//FIXME better error message: "handler signature does not match event signature"
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, handler));
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

	static bool CanGet(ILocalValue parentVar, string localName, SourceGenContext context, out ITypeSymbol? propertyType, out IPropertySymbol? propertySymbol)
	{
		propertyType = null;
		if ((propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault()) == null)
			return false;
		if (propertySymbol!.GetMethod is not IMethodSymbol propertyGetter || !propertyGetter.IsPublic() || propertyGetter.IsStatic)
			return false;

		propertyType = propertySymbol.Type;
		return true;
	}

	static bool CanGetValue(ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, bool attached, SourceGenContext context, out ITypeSymbol? propertyType)
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
			_ = CanGetValue(parentVar, bpFieldSymbol, attached, context, out  propertyType) || CanGet(parentVar, localName, context, out propertyType, out propertySymbol);
		
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
	/// </summary>
	static void SetExpressionBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol bpFieldSymbol, string expression, ITypeSymbol dataTypeSymbol, SourceGenContext context, ValueNode valueNode)
	{
		var bpName = bpFieldSymbol.ToFQDisplayString();
		var bpTypeAndConverter = bpFieldSymbol.GetBPTypeAndConverter(context);
		var targetType = bpTypeAndConverter?.type ?? context.Compilation.ObjectType;
		
		var sourceTypeName = dataTypeSymbol.ToFQDisplayString();
		var targetTypeName = targetType.ToFQDisplayString();

		// Transform quotes with semantic context - char literals stay as char only if target expects char
		var transformedExpression = CSharpExpressionHelpers.TransformQuotesWithSemantics(
			expression, context.Compilation, dataTypeSymbol, context.RootType);

		// Analyze expression for mixed local+binding scenarios
		var analysis = ExpressionAnalyzer.Analyze(transformedExpression, "__source", dataTypeSymbol, context.RootType);
		var handlers = analysis.Handlers;

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
			writer.WriteLine($"new global::Microsoft.Maui.Controls.Internals.TypedBinding<{sourceTypeName}, {targetTypeName}>(");
			writer.Indent++;
			// TransformedExpression already has identifiers prefixed with __source. where needed
			// Add null-forgiving operator if expression contains ?. to suppress nullability warnings
			var getterExpression = analysis.TransformedExpression;
			if (getterExpression.Contains("?."))
				getterExpression += "!";
			writer.WriteLine($"__source => ({getterExpression}, true),");
			
			// Generate setter if expression is a simple property chain
			if (analysis.IsSettable)
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

}
