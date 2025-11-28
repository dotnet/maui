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

		//DynamicResource
		if (!asCollectionItem && CanSetDynamicResource(bpFieldSymbol, valueNode, context, getNodeValue))
		{
			SetDynamicResource(writer, parentVar, bpFieldSymbol!, valueNode, context, getNodeValue);
			return;
		}

		//If it's a BP and the value is BindingBase, SetBinding
		if (!asCollectionItem && CanSetBinding(bpFieldSymbol, valueNode, context))
		{
			SetBinding(writer, parentVar, bpFieldSymbol!, valueNode, context, getNodeValue);
			return;
		}

		//If it's a BP, SetValue
		if (!asCollectionItem && CanSetValue(bpFieldSymbol, valueNode, context, getNodeValue))
		{
			SetValue(writer, parentVar, bpFieldSymbol!, valueNode, context, getNodeValue);
			return;
		}

		//POCO, set the property
		if (!asCollectionItem && CanSet(parentVar, localName, valueNode, context))
		{
			Set(writer, parentVar, localName, valueNode, context, getNodeValue);
			return;
		}

		//C# 14 extension property
		if (!asCollectionItem && CanSetExtensionProperty(parentVar, localName, valueNode, context))
		{
			SetExtensionProperty(writer, parentVar, localName, valueNode, context, getNodeValue);
			return;
		}

		if (CanAdd(parentVar, localName, bpFieldSymbol, attached, valueNode, context, getNodeValue))
		{
			Add(writer, parentVar, propertyName, valueNode, context, getNodeValue);
			return;
		}

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

	static bool CanSetValue(IFieldSymbol? bpFieldSymbol, INode node,  SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		if (bpFieldSymbol == null)
			return false;
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

	static void SetValue(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		var pType = bpFieldSymbol.GetBPTypeAndConverter(context)?.type;
		if (node is ValueNode valueNode)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var valueString = valueNode.ConvertTo(bpFieldSymbol, writer,context, parentVar);
				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpFieldSymbol.ToFQDisplayString()}, {valueString});");
			}
		}
		else if (node is ElementNode elementNode)
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var localVar = getNodeValue(elementNode, context.Compilation.ObjectType);
				string cast = string.Empty;
				
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
				
				writer.WriteLine($"{parentVar.ValueAccessor}.SetValue({bpFieldSymbol.ToFQDisplayString()}, {cast}{localVar.ValueAccessor});");
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

	static bool CanSetBinding(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
	{

		if (bpFieldSymbol == null)
			return false;
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

	static void SetBinding(IndentedTextWriter writer, ILocalValue parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		var localVariable = getNodeValue((ElementNode)node, context.Compilation.ObjectType);
		writer.WriteLine($"{parentVar.ValueAccessor}.SetBinding({bpFieldSymbol.ToFQDisplayString()}, {localVariable.ValueAccessor});");
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

	/// <summary>
	/// Finds C# 14 extension property for a given target type and property name.
	/// In C# 14, extension properties appear directly on static classes with get_X/set_X accessor methods
	/// that take the target type as the first parameter.
	/// Results are cached per context to avoid repeated searches.
	/// </summary>
	static IPropertySymbol? FindExtensionProperty(
		ITypeSymbol targetType, string propertyName, SourceGenContext context)
	{
		// Use cache if available
		context.extensionPropertyCache ??= new Dictionary<(ITypeSymbol, string), IPropertySymbol?>(
			new ExtensionPropertyKeyComparer());
		
		var key = (targetType, propertyName);
		if (context.extensionPropertyCache.TryGetValue(key, out var cachedResult))
			return cachedResult;

		// Search through all static types
		var allStaticTypes = GetAllStaticTypes(context);
		
		foreach (var type in allStaticTypes)
		{
			// Look for the property directly on the static class
			foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
			{
				if (prop.Name != propertyName)
					continue;

				// Check if this is an extension property by looking at the accessor parameters
				var getter = prop.GetMethod;
				var setter = prop.SetMethod;
				
				ITypeSymbol? extendedType = null;
				if (getter != null && getter.Parameters.Length == 1)
					extendedType = getter.Parameters[0].Type;
				else if (setter != null && setter.Parameters.Length == 2)
					extendedType = setter.Parameters[0].Type;

				if (extendedType != null && IsAssignableFrom(extendedType, targetType, context))
				{
					context.extensionPropertyCache[key] = prop;
					return prop;
				}
			}
		}

		context.extensionPropertyCache[key] = null;
		return null;
	}

	/// <summary>
	/// Finds C# 14 extension property getter and setter methods for a given target type and property name.
	/// Extension properties are compiled as static get_X/set_X methods in static classes.
	/// The getter has 1 parameter (the target) and the setter has 2 parameters (target, value).
	/// Results are cached per context to avoid repeated searches.
	/// </summary>
	static (IMethodSymbol? Getter, IMethodSymbol? Setter) FindExtensionPropertyMethods(
		ITypeSymbol targetType, string propertyName, SourceGenContext context)
	{
		// Use cache if available
		context.extensionPropertyMethodsCache ??= new Dictionary<(ITypeSymbol, string), (IMethodSymbol?, IMethodSymbol?)>(
			new ExtensionPropertyKeyComparer());
		
		var key = (targetType, propertyName);
		if (context.extensionPropertyMethodsCache.TryGetValue(key, out var cachedResult))
			return cachedResult;

		var getterName = $"get_{propertyName}";
		var setterName = $"set_{propertyName}";

		// Search through all static types
		var allStaticTypes = GetAllStaticTypes(context);

		foreach (var type in allStaticTypes)
		{
			// Look for get_PropertyName and set_PropertyName static methods
			IMethodSymbol? getter = null;
			IMethodSymbol? setter = null;

			foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
			{
				if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
					continue;

				if (method.Name == getterName && method.Parameters.Length == 1)
				{
					var paramType = method.Parameters[0].Type;
					if (IsAssignableFrom(paramType, targetType, context))
					{
						getter = method;
					}
				}
				else if (method.Name == setterName && method.Parameters.Length == 2)
				{
					var paramType = method.Parameters[0].Type;
					if (IsAssignableFrom(paramType, targetType, context))
					{
						setter = method;
					}
				}
			}

			if (getter != null || setter != null)
			{
				var result = (getter, setter);
				context.extensionPropertyMethodsCache[key] = result;
				return result;
			}
		}

		context.extensionPropertyMethodsCache[key] = (null, null);
		return (null, null);
	}

	/// <summary>
	/// Comparer for extension property cache keys that uses symbol equality.
	/// </summary>
	sealed class ExtensionPropertyKeyComparer : IEqualityComparer<(ITypeSymbol, string)>
	{
		public bool Equals((ITypeSymbol, string) x, (ITypeSymbol, string) y)
			=> SymbolEqualityComparer.Default.Equals(x.Item1, y.Item1) && x.Item2 == y.Item2;

		public int GetHashCode((ITypeSymbol, string) obj)
			=> SymbolEqualityComparer.Default.GetHashCode(obj.Item1) ^ obj.Item2.GetHashCode();
	}

	/// <summary>
	/// Gets all static types from the compilation, with caching to avoid repeated enumeration.
	/// </summary>
	static INamedTypeSymbol[] GetAllStaticTypes(SourceGenContext context)
	{
		if (context.allStaticTypesCache != null)
			return context.allStaticTypesCache;

		var staticTypes = new List<INamedTypeSymbol>();
		foreach (var type in GetAllTypesFromCompilation(context.Compilation))
		{
			if (type.IsStatic)
				staticTypes.Add(type);
		}
		
		context.allStaticTypesCache = staticTypes.ToArray();
		return context.allStaticTypesCache;
	}

	static IEnumerable<INamedTypeSymbol> GetAllTypesFromCompilation(Compilation compilation)
	{
		// Get types from all referenced assemblies
		foreach (var reference in compilation.References)
		{
			if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
			{
				foreach (var type in GetAllTypesFromNamespace(assembly.GlobalNamespace))
				{
					yield return type;
				}
			}
		}

		// Get types from the current compilation
		foreach (var type in GetAllTypesFromNamespace(compilation.GlobalNamespace))
		{
			yield return type;
		}
	}

	static IEnumerable<INamedTypeSymbol> GetAllTypesFromNamespace(INamespaceSymbol namespaceSymbol)
	{
		foreach (var type in namespaceSymbol.GetTypeMembers())
		{
			yield return type;
			foreach (var nested in GetNestedTypes(type))
			{
				yield return nested;
			}
		}

		foreach (var nestedNs in namespaceSymbol.GetNamespaceMembers())
		{
			foreach (var type in GetAllTypesFromNamespace(nestedNs))
			{
				yield return type;
			}
		}
	}

	static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
	{
		foreach (var nested in type.GetTypeMembers())
		{
			yield return nested;
			foreach (var deepNested in GetNestedTypes(nested))
			{
				yield return deepNested;
			}
		}
	}

	static bool IsAssignableFrom(ITypeSymbol baseType, ITypeSymbol derivedType, SourceGenContext context)
	{
		if (SymbolEqualityComparer.Default.Equals(baseType, derivedType))
			return true;

		return derivedType.InheritsFrom(baseType, context) || 
		       (baseType.TypeKind == TypeKind.Interface && derivedType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, baseType)));
	}

	static bool CanSetExtensionProperty(ILocalValue parentVar, string localName, INode node, SourceGenContext context)
	{
		ITypeSymbol? propertyType = null;

		// First try to find extension property as IPropertySymbol (C# 14 semantic model)
		var extProp = FindExtensionProperty(parentVar.Type, localName, context);
		if (extProp != null && extProp.SetMethod != null)
		{
			propertyType = extProp.Type;
		}
		else
		{
			// Fall back to finding lowered accessor methods
			var (getter, setter) = FindExtensionPropertyMethods(parentVar.Type, localName, context);
			if (setter == null)
				return false;

			// Get the property type from the getter's return type or setter's second parameter
			propertyType = getter?.ReturnType ?? setter.Parameters[1].Type;
		}

		if (node is ValueNode vn && vn.CanConvertTo(propertyType, context))
			return true;

		if (node is not ElementNode elementNode)
			return false;

		if (!context.Variables.TryGetValue(elementNode, out var localVar))
			return false;

		if (localVar.Type.InheritsFrom(propertyType, context))
			return true;

		if (propertyType.TypeKind == TypeKind.Interface && localVar.Type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, propertyType)))
			return true;

		if (propertyType.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
			return true;

		if (context.Compilation.HasImplicitConversion(localVar.Type, propertyType))
			return true;

		return false;
	}

	static bool CanGetExtensionProperty(ILocalValue parentVar, string localName, SourceGenContext context, out ITypeSymbol? propertyType)
	{
		propertyType = null;
		
		// First try to find extension property as IPropertySymbol (C# 14 semantic model)
		var extProp = FindExtensionProperty(parentVar.Type, localName, context);
		if (extProp != null && extProp.GetMethod != null)
		{
			propertyType = extProp.Type;
			return true;
		}

		// Fall back to finding lowered accessor methods
		var (getter, _) = FindExtensionPropertyMethods(parentVar.Type, localName, context);
		if (getter == null)
			return false;

		propertyType = getter.ReturnType;
		return true;
	}

	static void SetExtensionProperty(IndentedTextWriter writer, ILocalValue parentVar, string localName, INode node, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate getNodeValue)
	{
		// First try to find extension property as IPropertySymbol (C# 14 semantic model)
		var extProp = FindExtensionProperty(parentVar.Type, localName, context);
		if (extProp != null)
		{
			// Generate code using property access syntax - the compiler will handle it
			var propertyType = extProp.Type;

			if (node is ValueNode valueNode)
			{
				using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
				{
					var valueString = valueNode.ConvertTo(propertyType, writer, context, parentVar);
					writer.WriteLine($"{parentVar.ValueAccessor}.{localName} = {valueString};");
				}
			}
			else if (node is ElementNode elementNode)
			{
				using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
				{
					var localVar = getNodeValue(elementNode, context.Compilation.ObjectType);
					string cast = string.Empty;
					if (!context.Compilation.HasImplicitConversion(localVar.Type, propertyType))
					{
						cast = $"({propertyType.ToFQDisplayString()})";
					}
					writer.WriteLine($"{parentVar.ValueAccessor}.{localName} = {cast}{localVar.ValueAccessor};");
				}
			}
			return;
		}

		// Fall back to lowered accessor methods
		var (getter, setter) = FindExtensionPropertyMethods(parentVar.Type, localName, context);
		
		// Get the property type from the getter's return type or setter's second parameter
		var propType = getter?.ReturnType ?? setter!.Parameters[1].Type;

		// Generate: ExtensionClass.set_PropertyName(target, value)
		var extensionClassName = setter!.ContainingType.ToFQDisplayString();
		var setterName = setter.Name;

		if (node is ValueNode vn)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var valueString = vn.ConvertTo(propType, writer, context, parentVar);
				writer.WriteLine($"{extensionClassName}.{setterName}({parentVar.ValueAccessor}, {valueString});");
			}
		}
		else if (node is ElementNode en)
		{
			using (context.ProjectItem.EnableLineInfo ? PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.ProjectItem) : PrePost.NoBlock())
			{
				var localVar = getNodeValue(en, context.Compilation.ObjectType);
				string cast = string.Empty;
				if (!context.Compilation.HasImplicitConversion(localVar.Type, propType))
				{
					cast = $"({propType.ToFQDisplayString()})";
				}
				writer.WriteLine($"{extensionClassName}.{setterName}({parentVar.ValueAccessor}, {cast}{localVar.ValueAccessor});");
			}
		}
	}

	static string GetExtensionProperty(ILocalValue parentVar, string localName, SourceGenContext context)
	{
		// First try to find extension property as IPropertySymbol (C# 14 semantic model)
		var extProp = FindExtensionProperty(parentVar.Type, localName, context);
		if (extProp != null)
		{
			// Generate code using property access syntax - the compiler will handle it
			return $"{parentVar.ValueAccessor}.{localName}";
		}

		// Fall back to lowered accessor methods
		var (getter, _) = FindExtensionPropertyMethods(parentVar.Type, localName, context);
		
		// Generate: ExtensionClass.get_PropertyName(target)
		var extensionClassName = getter!.ContainingType.ToFQDisplayString();
		var getterName = getter.Name;

		return $"{extensionClassName}.{getterName}({parentVar.ValueAccessor})";
	}
}
