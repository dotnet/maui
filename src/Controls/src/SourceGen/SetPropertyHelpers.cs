using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;
using static LocationHelpers;

static class SetPropertyHelpers
{
	public static void SetPropertyValue(IndentedTextWriter writer, ILocalValue parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate? tryGetNodeValue = null, bool treeOrder = false, IndentedTextWriter? icWriter = null, ILocalValue? inflatorVar = null, bool asCollectionItem = false, ImmutableArray<Scope>? scopes = null)
	{
		tryGetNodeValue ??= (node, toType, out localVariable) => context.Variables.TryGetValue(node, out localVariable);

		if (propertyName.Equals(XmlName._CreateContent))
            propertyName = new XmlName("", "LoadTemplate"); //this way it'll be handled by CanSet()/Set() below			
			
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

		//DynamicResource
		if (!asCollectionItem && CanSetDynamicResource(bpFieldSymbol, valueNode, context, tryGetNodeValue))
		{
			SetDynamicResource(writer, parentVar, bpFieldSymbol!, valueNode, context, tryGetNodeValue);
			return;
		}

		//If it's a BP and the value is BindingBase, SetBinding
		if (!asCollectionItem && CanSetBinding(bpFieldSymbol, valueNode, parentVar.Type, localName, context, out var explicitPropertyNameForBinding))
		{

			SetBinding(writer, parentVar, bpFieldSymbol!, localName, explicitPropertyNameForBinding, valueNode, context, tryGetNodeValue);
			return;
		}

		//If it's a BP, SetValue

		if (!asCollectionItem && CanSetValue(bpFieldSymbol, valueNode, parentVar.Type, localName, context, tryGetNodeValue, out var explicitPropertyNameForValue))
		{
			SetValue(writer, parentVar, bpFieldSymbol!, localName, explicitPropertyNameForValue,valueNode, context, tryGetNodeValue, scopes);
			return;
		}

		//POCO, set the property
		if (!asCollectionItem && CanSet(parentVar, localName, valueNode, context))
		{
			Set(writer, parentVar, localName, valueNode, context, tryGetNodeValue);
			return;
		}

		if (CanAdd(parentVar, localName, bpFieldSymbol, attached, valueNode, context, tryGetNodeValue))
		{
			Add(writer, parentVar, propertyName, valueNode, context, tryGetNodeValue);
			return;
		}

		var location = LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, localName);
		context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
	}

	public static bool CanAddToResourceDictionary(ILocalValue parentVar, ITypeSymbol collectionType, ElementNode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
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
		
		tryGetNodeValue(node, context.Compilation.ObjectType, out var localVariable);
		var nodeType = localVariable!.Type;
		if (collectionType.GetAllMethods("Add", context).FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(nodeType, SymbolEqualityComparer.Default)) != null)
			return true;
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

	public static void AddToResourceDictionary(IndentedTextWriter writer, ILocalValue parentVar, ElementNode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
	{
		if (node.Properties.TryGetValue(XmlName.xKey, out var keyNode))
		{
			if (!context.KeysInRD.ContainsKey(parentVar))
				context.KeysInRD[parentVar] = [];
			context.KeysInRD[parentVar].Add((((ValueNode)keyNode).Value as string)!);
			var key = ((ValueNode)keyNode).Value as string;
			tryGetNodeValue(node, context.Compilation.ObjectType, out var lVar);
			writer.WriteLine($"{parentVar.ValueAccessor}[\"{key}\"] = {lVar?.ValueAccessor};");
			return;
		}
		tryGetNodeValue(node, context.Compilation.ObjectType, out var localVar);
		writer.WriteLine($"{parentVar.ValueAccessor}.Add({localVar?.ValueAccessor});");
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

	static bool CanSetDynamicResource(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
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

	static void SetDynamicResource(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol fieldSymbol, INode valueNode, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
	{	
		tryGetNodeValue(valueNode, context.Compilation.ObjectType, out var localVar);
		writer.WriteLine($"((global::Microsoft.Maui.Controls.Internals.IDynamicResourceHandler){parentVar.ValueAccessor}).SetDynamicResource({fieldSymbol.ToFQDisplayString()}, {localVar!.ValueAccessor}.Key);");
	}

	static bool CanConnectEvent(ILocalValue parentVar, string localName, INode valueNode, bool attached, SourceGenContext context)
		//FIXME check event signature
		=> !attached && valueNode is ValueNode && parentVar.Type.GetAllEvents(localName, context).Any();

	static void ConnectEvent(IndentedTextWriter writer, ILocalValue parentVar, string localName, INode valueNode, SourceGenContext context, bool treeOrder, IndentedTextWriter? icWriter, ILocalValue? inflatorVar)
	{
		var eventSymbol = parentVar.Type.GetAllEvents(localName, context).First();
		var eventType = eventSymbol.Type;
		var handler = (string)((ValueNode)valueNode).Value;
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

	static bool CanSetValue(IFieldSymbol? bpFieldSymbol, INode node, ITypeSymbol parentType, string localName, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue, out string? explicitPropertyName)
	{
		explicitPropertyName = null;
		
		if (bpFieldSymbol != null)
		{
			// Normal BP case - apply existing logic
			if (node is ValueNode vn && vn.CanConvertTo(bpFieldSymbol, context))
				return true;
			if (node is not ElementNode en)
				return false;

			tryGetNodeValue(en, context.Compilation.ObjectType, out var localVar);

			// If it's an attached BP, there's no second chance to handle IMarkupExtensions, so we try here.
			// Worst case scenario ? InvalidCastException at runtime
			if (localVar!.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
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


	static void SetValue(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol bpFieldSymbol, string localName, string? explicitPropertyName, INode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue, ImmutableArray<Scope>? scopes = null)
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
					: (property != null ? valueNode.ConvertTo(property, writer, context, parentVar) : tryGetNodeValue(node, context.Compilation.ObjectType, out var lvalue) ? lvalue!.ValueAccessor : "default");
				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpName}, {valueString});");
			}
		}
		else if (node is ElementNode elementNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				tryGetNodeValue(elementNode, context.Compilation.ObjectType, out var localVar);
				var cast = string.Empty;
				
				if (bpFieldSymbol != null)
				{
					// BP case: check for double implicit conversion first
					if (HasDoubleImplicitConversion(localVar!.Type, pType, context, out var conv))
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
				else if (property != null && !context.Compilation.HasImplicitConversion(localVar!.Type, property.Type))
				{
					cast = $"({property.Type.ToFQDisplayString()})";
				}
				
				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpName}, {cast}{localVar!.ValueAccessor});");
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


	static void Set(IndentedTextWriter writer, ILocalValue parentVar, string localName, INode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
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
				tryGetNodeValue(elementNode, context.Compilation.ObjectType, out var localVar);
				string intermediateCast = HasDoubleImplicitConversion(localVar!.Type, property.Type, context, out var conv) ? "(" + conv!.ReturnType.ToFQDisplayString() + ")" : string.Empty;
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

	static void SetBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol? bpFieldSymbol, string localName, string? explicitPropertyName, INode node, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
	{
		tryGetNodeValue((ElementNode)node, context.Compilation.ObjectType, out var localVariable);
		
		if (bpFieldSymbol != null)
		{
			// Normal case: we have the BP field symbol
			writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({bpFieldSymbol.ToFQDisplayString()}, {localVariable!.ValueAccessor});");
		}
		else
		{
			// Heuristic case: generate SetBinding call using the expected BindableProperty name
			// Use explicit property name if provided by attribute, otherwise use the default {localName}Property format
			var bpName = explicitPropertyName ?? $"{localName}Property";
			writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({parentVar.Type.ToFQDisplayString()}.{bpName}, {localVariable!.ValueAccessor});");
		}
	}

	static bool CanAdd(ILocalValue parentVar, string localName, IFieldSymbol? bpFieldSymbol, bool attached, INode valueNode, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
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

		if (CanAddToResourceDictionary(parentVar, localName != null ? propertyType! : parentVar.Type, en, context, tryGetNodeValue))
			return true;

		//FIXME should be better and test for value type
		if ((localName != null ? propertyType! : parentVar.Type).CanAdd(context))
			return true;

		return false;

	}

	static void Add(IndentedTextWriter writer, ILocalValue parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, NodeSGExtensions.TryGetNodeValueDelegate tryGetNodeValue)
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

		if (CanAddToResourceDictionary(parentVar, propertyType!, (ElementNode)valueNode, context, tryGetNodeValue))
		{
			ILocalValue rdAccessor;
			if (localName != null)
				rdAccessor = new DirectValue(propertyType!, GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, valueNode, context));
			else
				rdAccessor = parentVar;
				
			AddToResourceDictionary(writer, rdAccessor, (ElementNode)valueNode, context, tryGetNodeValue);
			return;
		}

		ITypeSymbol itemType;
		if (propertyType!.ImplementsGeneric(context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")!, out var typeArguments))
			itemType = typeArguments[0];
		else
			itemType = context.Compilation.ObjectType;

		var adder = propertyType!.GetAllMethods("Add", context).First(m => m.Parameters.Length == 1);
		var receiverType = adder.ReceiverType;

		var parentObj = localName != null ? $"{parentVar.ValueAccessor}.{localName}" : parentVar.ValueAccessor;
		if (bpFieldSymbol != null)
		{
			var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
			parentObj = $"(({typeandconverter?.type.ToFQDisplayString()}){parentVar.ValueAccessor}.GetValue({bpFieldSymbol.ToFQDisplayString()}))";
		}

		if (receiverType is not null && !propertyType!.Equals(receiverType, SymbolEqualityComparer.Default))
			parentObj = $"(({receiverType.ToFQDisplayString()}){parentObj})";

		//look for intermediate implicit casts
		string cast = string.Empty;
		tryGetNodeValue(valueNode, context.Compilation.ObjectType, out var localVar);
		if (HasDoubleImplicitConversion(localVar!.Type, itemType, context, out var conv))
			cast = "(" + conv!.ReturnType.ToFQDisplayString() + ")";

		using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.ProjectItem) : PrePost.NoBlock())
		{
			tryGetNodeValue(valueNode, context.Compilation.ObjectType, out localVar);
			writer.WriteLine($"{parentObj}.Add(({itemType.ToFQDisplayString()}){cast}{localVar!.ValueAccessor});");
		}
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

}
