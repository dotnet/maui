using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;
using System.Xml;

namespace Xamarin.Forms.Build.Tasks
{
	class CreateObjectVisitor : IXamlNodeVisitor
	{
		public CreateObjectVisitor(ILContext context)
		{
			Context = context;
			Module = context.Body.Method.Module;
		}

		public ILContext Context { get; }

		ModuleDefinition Module { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;


		public void Visit(ValueNode node, INode parentNode)
		{
			Context.Values[node] = node.Value;
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
			//At this point, all MarkupNodes are expanded to ElementNodes
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			var typeref = Module.ImportReference(node.XmlType.GetTypeReference(Module, node));
			TypeDefinition typedef = typeref.Resolve();

			if (IsXaml2009LanguagePrimitive(node)) {
				var vardef = new VariableDefinition(typeref);
				Context.Variables [node] = vardef;
				Context.Body.Variables.Add(vardef);

				Context.IL.Append(PushValueFromLanguagePrimitive(typedef, node));
				Context.IL.Emit(OpCodes.Stloc, vardef);
				return;
			}

			//if this is a MarkupExtension that can be compiled directly, compile and returns the value
			var compiledMarkupExtensionName = typeref.GetCustomAttribute(Module.ImportReference(typeof(ProvideCompiledAttribute)))?.ConstructorArguments?[0].Value as string;
			Type compiledMarkupExtensionType;
			ICompiledMarkupExtension markupProvider;
			if (compiledMarkupExtensionName != null &&
				(compiledMarkupExtensionType = Type.GetType(compiledMarkupExtensionName)) != null &&
				(markupProvider = Activator.CreateInstance(compiledMarkupExtensionType) as ICompiledMarkupExtension) != null) {

				var il = markupProvider.ProvideValue(node, Module, Context, out typeref);
				typeref = Module.ImportReference(typeref);

				var vardef = new VariableDefinition(typeref);
				Context.Variables[node] = vardef;
				Context.Body.Variables.Add(vardef);

				Context.IL.Append(il);
				Context.IL.Emit(OpCodes.Stloc, vardef);

				//clean the node as it has been fully exhausted
				foreach (var prop in node.Properties)
					if (!node.SkipProperties.Contains(prop.Key))
						node.SkipProperties.Add(prop.Key);
				node.CollectionItems.Clear();
				return;
			}

			MethodDefinition factoryCtorInfo = null;
			MethodDefinition factoryMethodInfo = null;
			MethodDefinition parameterizedCtorInfo = null;
			MethodDefinition ctorInfo = null;

			if (node.Properties.ContainsKey(XmlName.xArguments) && !node.Properties.ContainsKey(XmlName.xFactoryMethod)) {
				factoryCtorInfo = typedef.AllMethods().FirstOrDefault(md => md.IsConstructor &&
																			!md.IsStatic &&
																			md.HasParameters &&
																			md.MatchXArguments(node, typeref, Module, Context));
				if (factoryCtorInfo == null) {
					throw new XamlParseException(
						string.Format("No constructors found for {0} with matching x:Arguments", typedef.FullName), node);
				}
				ctorInfo = factoryCtorInfo;
				if (!typedef.IsValueType) //for ctor'ing typedefs, we first have to ldloca before the params
					Context.IL.Append(PushCtorXArguments(factoryCtorInfo, node));
			} else if (node.Properties.ContainsKey(XmlName.xFactoryMethod)) {
				var factoryMethod = (string)(node.Properties [XmlName.xFactoryMethod] as ValueNode).Value;
				factoryMethodInfo = typedef.AllMethods().FirstOrDefault(md => !md.IsConstructor &&
																			  md.Name == factoryMethod &&
																			  md.IsStatic &&
																			  md.MatchXArguments(node, typeref, Module, Context));
				if (factoryMethodInfo == null) {
					throw new XamlParseException(
						String.Format("No static method found for {0}::{1} ({2})", typedef.FullName, factoryMethod, null), node);
				}
				Context.IL.Append(PushCtorXArguments(factoryMethodInfo, node));
			}
			if (ctorInfo == null && factoryMethodInfo == null) {
				parameterizedCtorInfo = typedef.Methods.FirstOrDefault(md => md.IsConstructor &&
																			 !md.IsStatic &&
																			 md.HasParameters &&
																			 md.Parameters.All(
																				 pd =>
																					 pd.CustomAttributes.Any(
																						 ca =>
																							 ca.AttributeType.FullName ==
																							 "Xamarin.Forms.ParameterAttribute")));
			}
			if (parameterizedCtorInfo != null && ValidateCtorArguments(parameterizedCtorInfo, node)) {
				ctorInfo = parameterizedCtorInfo;
//				IL_0000:  ldstr "foo"
				Context.IL.Append(PushCtorArguments(parameterizedCtorInfo, node));
			}
			ctorInfo = ctorInfo ?? typedef.Methods.FirstOrDefault(md => md.IsConstructor && !md.HasParameters && !md.IsStatic);

			var ctorinforef = ctorInfo?.ResolveGenericParameters(typeref, Module);
			var factorymethodinforef = factoryMethodInfo?.ResolveGenericParameters(typeref, Module);
			var implicitOperatorref = typedef.Methods.FirstOrDefault(md =>
				md.IsPublic &&
				md.IsStatic &&
				md.IsSpecialName &&
				md.Name == "op_Implicit" && md.Parameters [0].ParameterType.FullName == "System.String");

			if (ctorinforef != null || factorymethodinforef != null || typedef.IsValueType) {
				VariableDefinition vardef = new VariableDefinition(typeref);
				Context.Variables [node] = vardef;
				Context.Body.Variables.Add(vardef);

				ValueNode vnode = null;
				if (node.CollectionItems.Count == 1 && (vnode = node.CollectionItems.First() as ValueNode) != null &&
					vardef.VariableType.IsValueType) {
					//<Color>Purple</Color>
					Context.IL.Append(vnode.PushConvertedValue(Context, typeref, new ICustomAttributeProvider [] { typedef },
						node.PushServiceProvider(Context), false, true));
					Context.IL.Emit(OpCodes.Stloc, vardef);
				} else if (node.CollectionItems.Count == 1 && (vnode = node.CollectionItems.First() as ValueNode) != null &&
						   implicitOperatorref != null) {
					//<FileImageSource>path.png</FileImageSource>
					var implicitOperator = Module.ImportReference(implicitOperatorref);
					Context.IL.Emit(OpCodes.Ldstr, ((ValueNode)(node.CollectionItems.First())).Value as string);
					Context.IL.Emit(OpCodes.Call, implicitOperator);
					Context.IL.Emit(OpCodes.Stloc, vardef);
				} else if (factorymethodinforef != null) {
					Context.IL.Emit(OpCodes.Call, Module.ImportReference(factorymethodinforef));
					Context.IL.Emit(OpCodes.Stloc, vardef);
				} else if (!typedef.IsValueType) {
					var ctor = Module.ImportReference(ctorinforef);
//					IL_0001:  newobj instance void class [Xamarin.Forms.Core]Xamarin.Forms.Button::'.ctor'()
//					IL_0006:  stloc.0 
					Context.IL.Emit(OpCodes.Newobj, ctor);
					Context.IL.Emit(OpCodes.Stloc, vardef);
				} else if (ctorInfo != null && node.Properties.ContainsKey(XmlName.xArguments) &&
						   !node.Properties.ContainsKey(XmlName.xFactoryMethod) && ctorInfo.MatchXArguments(node, typeref, Module, Context)) {
//					IL_0008:  ldloca.s 1
//					IL_000a:  ldc.i4.1 
//					IL_000b:  call instance void valuetype Test/Foo::'.ctor'(bool)

					var ctor = Module.ImportReference(ctorinforef);
					Context.IL.Emit(OpCodes.Ldloca, vardef);
					Context.IL.Append(PushCtorXArguments(factoryCtorInfo, node));
					Context.IL.Emit(OpCodes.Call, ctor);
				} else {
//					IL_0000:  ldloca.s 0
//					IL_0002:  initobj Test/Foo
					Context.IL.Emit(OpCodes.Ldloca, vardef);
					Context.IL.Emit(OpCodes.Initobj, Module.ImportReference(typedef));
				}

				if (typeref.FullName == "Xamarin.Forms.Xaml.ArrayExtension") {
					var visitor = new SetPropertiesVisitor(Context);
					foreach (var cnode in node.Properties.Values.ToList())
						cnode.Accept(visitor, node);
					foreach (var cnode in node.CollectionItems)
						cnode.Accept(visitor, node);

					markupProvider = new ArrayExtension();

					var il = markupProvider.ProvideValue(node, Module, Context, out typeref);

					vardef = new VariableDefinition(typeref);
					Context.Variables[node] = vardef;
					Context.Body.Variables.Add(vardef);

					Context.IL.Append(il);
					Context.IL.Emit(OpCodes.Stloc, vardef);

					//clean the node as it has been fully exhausted
					foreach (var prop in node.Properties)
						if (!node.SkipProperties.Contains(prop.Key))
							node.SkipProperties.Add(prop.Key);
					node.CollectionItems.Clear();

					return;
				}
			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
//			IL_0013:  ldarg.0 
//			IL_0014:  stloc.3 

			var ilnode = (ILRootNode)node;
			var typeref = ilnode.TypeReference;
			var vardef = new VariableDefinition(typeref);
			Context.Variables[node] = vardef;
			Context.Root = vardef;
			Context.Body.Variables.Add(vardef);
			Context.IL.Emit(OpCodes.Ldarg_0);
			Context.IL.Emit(OpCodes.Stloc, vardef);
		}

		public void Visit(ListNode node, INode parentNode)
		{
			XmlName name;
			if (SetPropertiesVisitor.TryGetPropertyName(node, parentNode, out name))
				node.XmlName = name;
		}

		bool ValidateCtorArguments(MethodDefinition ctorinfo, ElementNode enode)
		{
			foreach (var parameter in ctorinfo.Parameters)
			{
				var propname =
					parameter.CustomAttributes.First(ca => ca.AttributeType.FullName == "Xamarin.Forms.ParameterAttribute")
						.ConstructorArguments.First()
						.Value as string;
				if (!enode.Properties.ContainsKey(new XmlName("", propname)))
					return false;
			}
			return true;
		}

		IEnumerable<Instruction> PushCtorArguments(MethodDefinition ctorinfo, ElementNode enode)
		{
			foreach (var parameter in ctorinfo.Parameters)
			{
				var propname =
					parameter.CustomAttributes.First(ca => ca.AttributeType.FullName == "Xamarin.Forms.ParameterAttribute")
						.ConstructorArguments.First()
						.Value as string;
				var node = enode.Properties[new XmlName("", propname)];
				if (!enode.SkipProperties.Contains(new XmlName("", propname)))
					enode.SkipProperties.Add(new XmlName("", propname));
				VariableDefinition vardef;
				ValueNode vnode = null;

				if (node is IElementNode && (vardef = Context.Variables[node as IElementNode]) != null)
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				else if ((vnode = node as ValueNode) != null)
				{
					foreach (var instruction in vnode.PushConvertedValue(Context,
						parameter.ParameterType,
						new ICustomAttributeProvider[] { parameter, parameter.ParameterType.Resolve() },
						enode.PushServiceProvider(Context), false, true))
						yield return instruction;
				}
			}
		}

		IEnumerable<Instruction> PushCtorXArguments(MethodDefinition factoryCtorInfo, ElementNode enode)
		{
			if (!enode.Properties.ContainsKey(XmlName.xArguments))
				yield break;

			var arguments = new List<INode>();
			var node = enode.Properties[XmlName.xArguments] as ElementNode;
			if (node != null)
				arguments.Add(node);
			var list = enode.Properties[XmlName.xArguments] as ListNode;
			if (list != null)
			{
				foreach (var n in list.CollectionItems)
					arguments.Add(n);
			}

			for (var i = 0; i < factoryCtorInfo.Parameters.Count; i++)
			{
				var parameter = factoryCtorInfo.Parameters[i];
				var arg = arguments[i];
				VariableDefinition vardef;
				ValueNode vnode = null;

				if (arg is IElementNode && (vardef = Context.Variables[arg as IElementNode]) != null)
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				else if ((vnode = arg as ValueNode) != null)
				{
					foreach (var instruction in vnode.PushConvertedValue(Context,
						parameter.ParameterType,
						new ICustomAttributeProvider[] { parameter, parameter.ParameterType.Resolve() },
						enode.PushServiceProvider(Context), false, true))
						yield return instruction;
				}
			}
		}

		static bool IsXaml2009LanguagePrimitive(IElementNode node)
		{
			if (node.NamespaceURI == XamlParser.X2009Uri) {
				var n = node.XmlType.Name.Split(':') [1];
				return n != "Array";
			}
			if (node.NamespaceURI != "clr-namespace:System;assembly=mscorlib")
				return false;
			var name = node.XmlType.Name.Split(':')[1];
			if (name == "SByte" ||
				name == "Int16" ||
				name == "Int32" ||
				name == "Int64" ||
				name == "Byte" ||
				name == "UInt16" ||
				name == "UInt32" ||
				name == "UInt64" ||
				name == "Single" ||
				name == "Double" ||
				name == "Boolean" ||
				name == "String" ||
				name == "Char" ||
				name == "Decimal" ||
				name == "TimeSpan" ||
				name == "Uri")
				return true;
			return false;
		}

		IEnumerable<Instruction> PushValueFromLanguagePrimitive(TypeDefinition typedef, ElementNode node)
		{
			var hasValue = node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode &&
			               ((ValueNode)node.CollectionItems[0]).Value is string;
			var valueString = hasValue ? ((ValueNode)node.CollectionItems[0]).Value as string : string.Empty;
			switch (typedef.FullName) {
			case "System.SByte":
				sbyte outsbyte;
				if (hasValue && sbyte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outsbyte))
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)outsbyte);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.Int16":
				short outshort;
				if (hasValue && short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outshort))
					yield return Instruction.Create(OpCodes.Ldc_I4, outshort);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.Int32":
				int outint;
				if (hasValue && int.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outint))
					yield return Instruction.Create(OpCodes.Ldc_I4, outint);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.Int64":
				long outlong;
				if (hasValue && long.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outlong))
					yield return Instruction.Create(OpCodes.Ldc_I8, outlong);
				else
					yield return Instruction.Create(OpCodes.Ldc_I8, 0L);
				break;
			case "System.Byte":
				byte outbyte;
				if (hasValue && byte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outbyte))
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)outbyte);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.UInt16":
				short outushort;
				if (hasValue && short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outushort))
					yield return Instruction.Create(OpCodes.Ldc_I4, outushort);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.UInt32":
				int outuint;
				if (hasValue && int.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outuint))
					yield return Instruction.Create(OpCodes.Ldc_I4, outuint);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.UInt64":
				long outulong;
				if (hasValue && long.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outulong))
					yield return Instruction.Create(OpCodes.Ldc_I8, outulong);
				else
					yield return Instruction.Create(OpCodes.Ldc_I8, 0L);
				break;
			case "System.Boolean":
				bool outbool;
				if (hasValue && bool.TryParse(valueString, out outbool))
					yield return Instruction.Create(outbool ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4_0);
				break;
			case "System.String":
				yield return Instruction.Create(OpCodes.Ldstr, valueString);
				break;
			case "System.Object":
				var ctorinfo =
					Context.Body.Method.Module.TypeSystem.Object.Resolve()
						.Methods.FirstOrDefault(md => md.IsConstructor && !md.HasParameters);
				var ctor = Context.Body.Method.Module.ImportReference(ctorinfo);
				yield return Instruction.Create(OpCodes.Newobj, ctor);
				break;
			case "System.Char":
				char outchar;
				if (hasValue && char.TryParse(valueString, out outchar))
					yield return Instruction.Create(OpCodes.Ldc_I4, outchar);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x00);
				break;
			case "System.Decimal":
				decimal outdecimal;
				if (hasValue && decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outdecimal)) {
					var vardef = new VariableDefinition(Context.Body.Method.Module.ImportReference(typeof(decimal)));
					Context.Body.Variables.Add(vardef);
					//Use an extra temp var so we can push the value to the stack, just like other cases
					//					IL_0003:  ldstr "adecimal"
					//					IL_0008:  ldc.i4.s 0x6f
					//					IL_000a:  call class [mscorlib]System.Globalization.CultureInfo class [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
					//					IL_000f:  ldloca.s 0
					//					IL_0011:  call bool valuetype [mscorlib]System.Decimal::TryParse(string, valuetype [mscorlib]System.Globalization.NumberStyles, class [mscorlib]System.IFormatProvider, [out] valuetype [mscorlib]System.Decimal&)
					//					IL_0016:  pop
					yield return Instruction.Create(OpCodes.Ldstr, valueString);
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x6f); //NumberStyles.Number
					var getInvariantInfo =
						Context.Body.Method.Module.ImportReference(typeof(CultureInfo))
							.Resolve()
							.Properties.FirstOrDefault(pd => pd.Name == "InvariantCulture")
							.GetMethod;
					var getInvariant = Context.Body.Method.Module.ImportReference(getInvariantInfo);
					yield return Instruction.Create(OpCodes.Call, getInvariant);
					yield return Instruction.Create(OpCodes.Ldloca, vardef);
					var tryParseInfo =
						Context.Body.Method.Module.ImportReference(typeof(decimal))
							.Resolve()
							.Methods.FirstOrDefault(md => md.Name == "TryParse" && md.Parameters.Count == 4);
					var tryParse = Context.Body.Method.Module.ImportReference(tryParseInfo);
					yield return Instruction.Create(OpCodes.Call, tryParse);
					yield return Instruction.Create(OpCodes.Pop);
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				} else {
					yield return Instruction.Create(OpCodes.Ldc_I4_0);
					var decimalctorinfo =
						Context.Body.Method.Module.ImportReference(typeof(decimal))
							.Resolve()
							.Methods.FirstOrDefault(
								md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters [0].ParameterType.FullName == "System.Int32");
					var decimalctor = Context.Body.Method.Module.ImportReference(decimalctorinfo);
					yield return Instruction.Create(OpCodes.Newobj, decimalctor);
				}
				break;
			case "System.Single":
				float outfloat;
				if (hasValue && float.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outfloat))
					yield return Instruction.Create(OpCodes.Ldc_R4, outfloat);
				else
					yield return Instruction.Create(OpCodes.Ldc_R4, 0f);
				break;
			case "System.Double":
				double outdouble;
				if (hasValue && double.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out outdouble))
					yield return Instruction.Create(OpCodes.Ldc_R8, outdouble);
				else
					yield return Instruction.Create(OpCodes.Ldc_R8, 0d);
				break;
			case "System.TimeSpan":
				TimeSpan outspan;
				if (hasValue && TimeSpan.TryParse(valueString, CultureInfo.InvariantCulture, out outspan)) {
					var vardef = new VariableDefinition(Context.Body.Method.Module.ImportReference(typeof(TimeSpan)));
					Context.Body.Variables.Add(vardef);
					//Use an extra temp var so we can push the value to the stack, just like other cases
					yield return Instruction.Create(OpCodes.Ldstr, valueString);
					var getInvariantInfo =
						Context.Body.Method.Module.ImportReference(typeof(CultureInfo))
							.Resolve()
							.Properties.FirstOrDefault(pd => pd.Name == "InvariantCulture")
							.GetMethod;
					var getInvariant = Context.Body.Method.Module.ImportReference(getInvariantInfo);
					yield return Instruction.Create(OpCodes.Call, getInvariant);
					yield return Instruction.Create(OpCodes.Ldloca, vardef);
					var tryParseInfo =
						Context.Body.Method.Module.ImportReference(typeof(TimeSpan))
							.Resolve()
							.Methods.FirstOrDefault(md => md.Name == "TryParse" && md.Parameters.Count == 3);
					var tryParse = Context.Body.Method.Module.ImportReference(tryParseInfo);
					yield return Instruction.Create(OpCodes.Call, tryParse);
					yield return Instruction.Create(OpCodes.Pop);
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				} else {
					yield return Instruction.Create(OpCodes.Ldc_I8, 0L);
					var timespanctorinfo =
						Context.Body.Method.Module.ImportReference(typeof(TimeSpan))
							.Resolve()
							.Methods.FirstOrDefault(
								md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters [0].ParameterType.FullName == "System.Int64");
					var timespanctor = Context.Body.Method.Module.ImportReference(timespanctorinfo);
					yield return Instruction.Create(OpCodes.Newobj, timespanctor);
				}
				break;
			case "System.Uri":
				Uri outuri;
				if (hasValue && Uri.TryCreate(valueString, UriKind.RelativeOrAbsolute, out outuri)) {
					var vardef = new VariableDefinition(Context.Body.Method.Module.ImportReference(typeof(Uri)));
					Context.Body.Variables.Add(vardef);
					//Use an extra temp var so we can push the value to the stack, just like other cases
					yield return Instruction.Create(OpCodes.Ldstr, valueString);
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)UriKind.RelativeOrAbsolute);
					yield return Instruction.Create(OpCodes.Ldloca, vardef);
					var tryCreateInfo =
						Context.Body.Method.Module.ImportReference(typeof(Uri))
							.Resolve()
							.Methods.FirstOrDefault(md => md.Name == "TryCreate" && md.Parameters.Count == 3);
					var tryCreate = Context.Body.Method.Module.ImportReference(tryCreateInfo);
					yield return Instruction.Create(OpCodes.Call, tryCreate);
					yield return Instruction.Create(OpCodes.Pop);
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				} else
					yield return Instruction.Create(OpCodes.Ldnull);
				break;
			default:
				var defaultctorinfo = typedef.Methods.FirstOrDefault(md => md.IsConstructor && !md.HasParameters);
				if (defaultctorinfo != null) {
					var defaultctor = Context.Body.Method.Module.ImportReference(defaultctorinfo);
					yield return Instruction.Create(OpCodes.Newobj, defaultctor);
				} else {
					//should never happen. but if it does, this prevents corrupting the IL stack
					yield return Instruction.Create(OpCodes.Ldnull);
				}
				break;
			}
		}
	}
}