using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Xamarin.Forms.Build.Tasks
{
	static class NodeILExtensions
	{
		public static bool CanConvertValue(this ValueNode node, ILContext context, TypeReference targetTypeRef, IEnumerable<ICustomAttributeProvider> attributeProviders)
		{
			TypeReference typeConverter = null;
			foreach (var attributeProvider in attributeProviders) {
				CustomAttribute typeConverterAttribute;
				if (
					(typeConverterAttribute =
						attributeProvider.CustomAttributes.FirstOrDefault(
							cad => TypeConverterAttribute.TypeConvertersType.Contains(cad.AttributeType.FullName))) != null) {
					typeConverter = typeConverterAttribute.ConstructorArguments[0].Value as TypeReference;
					break;
				}
			}

			return node.CanConvertValue(context, targetTypeRef, typeConverter);
		}

		public static bool CanConvertValue(this ValueNode node, ILContext context, FieldReference bpRef)
		{
			var module = context.Body.Method.Module;
			var targetTypeRef = bpRef.GetBindablePropertyType(node, module);
			var typeConverter = bpRef.GetBindablePropertyTypeConverter(module);
			return node.CanConvertValue(context, targetTypeRef, typeConverter);
		}

		public static bool CanConvertValue(this ValueNode node, ILContext context, TypeReference targetTypeRef, TypeReference typeConverter)
		{
			var str = (string)node.Value;
			var module = context.Body.Method.Module;

			//If there's a [TypeConverter], use it
			if (typeConverter != null && str != null) {
				var typeConvAttribute = typeConverter.GetCustomAttribute(module, ("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "TypeConversionAttribute"));
				if (typeConvAttribute == null) //trust the unattributed TypeConverter
					return true;
				var toType = typeConvAttribute.ConstructorArguments.First().Value as TypeReference;
				return toType.InheritsFromOrImplements(targetTypeRef);
			}

			///No reason to return false
			return true;
		}

		public static IEnumerable<Instruction> PushConvertedValue(this ValueNode node, ILContext context,
			TypeReference targetTypeRef, IEnumerable<ICustomAttributeProvider> attributeProviders,
			IEnumerable<Instruction> pushServiceProvider, bool boxValueTypes, bool unboxValueTypes)
		{
			TypeReference typeConverter = null;
			foreach (var attributeProvider in attributeProviders)
			{
				CustomAttribute typeConverterAttribute;
				if (
					(typeConverterAttribute =
						attributeProvider.CustomAttributes.FirstOrDefault(
							cad => TypeConverterAttribute.TypeConvertersType.Contains(cad.AttributeType.FullName))) != null)
				{
					typeConverter = typeConverterAttribute.ConstructorArguments[0].Value as TypeReference;
					break;
				}
			}
			return node.PushConvertedValue(context, targetTypeRef, typeConverter, pushServiceProvider, boxValueTypes,
				unboxValueTypes);
		}

		public static IEnumerable<Instruction> PushConvertedValue(this ValueNode node, ILContext context, FieldReference bpRef,
			IEnumerable<Instruction> pushServiceProvider, bool boxValueTypes, bool unboxValueTypes)
		{
			var module = context.Body.Method.Module;
			var targetTypeRef = bpRef.GetBindablePropertyType(node, module);
			var typeConverter = bpRef.GetBindablePropertyTypeConverter(module);

			return node.PushConvertedValue(context, targetTypeRef, typeConverter, pushServiceProvider, boxValueTypes,
				unboxValueTypes);
		}

		public static IEnumerable<Instruction> PushConvertedValue(this ValueNode node, ILContext context,
			TypeReference targetTypeRef, TypeReference typeConverter, IEnumerable<Instruction> pushServiceProvider,
			bool boxValueTypes, bool unboxValueTypes)
		{
			var module = context.Body.Method.Module;
			var str = (string)node.Value;

			//If the TypeConverter has a ProvideCompiledAttribute that can be resolved, shortcut this
			var compiledConverterName = typeConverter?.GetCustomAttribute(module, ("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "ProvideCompiledAttribute"))?.ConstructorArguments?.First().Value as string;
			Type compiledConverterType;
			if (compiledConverterName != null && (compiledConverterType = Type.GetType (compiledConverterName)) != null) {
				var compiledConverter = Activator.CreateInstance (compiledConverterType);
				var converter = typeof(ICompiledTypeConverter).GetMethods ().FirstOrDefault (md => md.Name == "ConvertFromString");
				var instructions = (IEnumerable<Instruction>)converter.Invoke (compiledConverter, new object[] {
					node.Value as string, context, node as BaseNode});
				foreach (var i in instructions)
					yield return i;
				if (targetTypeRef.IsValueType && boxValueTypes)
					yield return Instruction.Create (OpCodes.Box, module.ImportReference (targetTypeRef));
				yield break;
			}

			//If there's a [TypeConverter], use it
			if (typeConverter != null)
			{
				var isExtendedConverter = typeConverter.ImplementsInterface(module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "IExtendedTypeConverter")));
				var typeConverterCtorRef = module.ImportCtorReference(typeConverter, paramCount: 0);
				var convertFromInvariantStringDefinition = isExtendedConverter
					? module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms", "IExtendedTypeConverter"))
						.ResolveCached()
						.Methods.FirstOrDefault(md => md.Name == "ConvertFromInvariantString" && md.Parameters.Count == 2)
					: typeConverter.ResolveCached()
						.AllMethods()
						.FirstOrDefault(md => md.Name == "ConvertFromInvariantString" && md.Parameters.Count == 1);
				var convertFromInvariantStringReference = module.ImportReference(convertFromInvariantStringDefinition);

				yield return Instruction.Create(OpCodes.Newobj, typeConverterCtorRef);
				yield return Instruction.Create(OpCodes.Ldstr, node.Value as string);

				if (isExtendedConverter)
				{
					foreach (var instruction in pushServiceProvider)
						yield return instruction;
				}

				yield return Instruction.Create(OpCodes.Callvirt, convertFromInvariantStringReference);

				if (targetTypeRef.IsValueType && unboxValueTypes)
					yield return Instruction.Create(OpCodes.Unbox_Any, module.ImportReference(targetTypeRef));

				//ConvertFrom returns an object, no need to Box
				yield break;
			}
			var originalTypeRef = targetTypeRef;
			var isNullable = false;
			MethodReference nullableCtor = null;
			if (targetTypeRef.ResolveCached().FullName == "System.Nullable`1")
			{
				var nullableTypeRef = targetTypeRef;
				targetTypeRef = ((GenericInstanceType)targetTypeRef).GenericArguments[0];
				isNullable = true;
				nullableCtor = originalTypeRef.GetMethods(md => md.IsConstructor && md.Parameters.Count == 1, module).Single().Item1;
				nullableCtor = nullableCtor.ResolveGenericParameters(nullableTypeRef, module);
			}

			var implicitOperator = module.TypeSystem.String.GetImplicitOperatorTo(targetTypeRef, module);

			//Obvious Built-in conversions
			if (targetTypeRef.ResolveCached().BaseType != null && targetTypeRef.ResolveCached().BaseType.FullName == "System.Enum")
				yield return PushParsedEnum(targetTypeRef, str, node);
			else if (targetTypeRef.FullName == "System.Char")
				yield return Instruction.Create(OpCodes.Ldc_I4, Char.Parse(str));
			else if (targetTypeRef.FullName == "System.SByte")
				yield return Instruction.Create(OpCodes.Ldc_I4, SByte.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Int16")
				yield return Instruction.Create(OpCodes.Ldc_I4, Int16.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Int32")
				yield return Instruction.Create(OpCodes.Ldc_I4, Int32.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Int64")
				yield return Instruction.Create(OpCodes.Ldc_I8, Int64.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Byte")
				yield return Instruction.Create(OpCodes.Ldc_I4, Byte.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.UInt16")
				yield return Instruction.Create(OpCodes.Ldc_I4, unchecked((int)UInt16.Parse(str, CultureInfo.InvariantCulture)));
			else if (targetTypeRef.FullName == "System.UInt32")
				yield return Instruction.Create(OpCodes.Ldc_I4, unchecked((int)UInt32.Parse(str, CultureInfo.InvariantCulture)));
			else if (targetTypeRef.FullName == "System.UInt64")
				yield return Instruction.Create(OpCodes.Ldc_I8, unchecked((long)UInt64.Parse(str, CultureInfo.InvariantCulture)));
			else if (targetTypeRef.FullName == "System.Single")
				yield return Instruction.Create(OpCodes.Ldc_R4, Single.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Double")
				yield return Instruction.Create(OpCodes.Ldc_R8, Double.Parse(str, CultureInfo.InvariantCulture));
			else if (targetTypeRef.FullName == "System.Boolean") {
				if (Boolean.Parse(str))
					yield return Instruction.Create(OpCodes.Ldc_I4_1);
				else
					yield return Instruction.Create(OpCodes.Ldc_I4_0);
			} else if (targetTypeRef.FullName == "System.TimeSpan") {
				var ts = TimeSpan.Parse(str, CultureInfo.InvariantCulture);
				var ticks = ts.Ticks;
				yield return Instruction.Create(OpCodes.Ldc_I8, ticks);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("mscorlib", "System", "TimeSpan"), parameterTypes: new[] { ("mscorlib", "System", "Int64") }));
			} else if (targetTypeRef.FullName == "System.DateTime") {
				var dt = DateTime.Parse(str, CultureInfo.InvariantCulture);
				var ticks = dt.Ticks;
				yield return Instruction.Create(OpCodes.Ldc_I8, ticks);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("mscorlib", "System", "DateTime"), parameterTypes: new[] { ("mscorlib", "System", "Int64") }));
			} else if (targetTypeRef.FullName == "System.String" && str.StartsWith("{}", StringComparison.Ordinal))
				yield return Instruction.Create(OpCodes.Ldstr, str.Substring(2));
			else if (targetTypeRef.FullName == "System.String")
				yield return Instruction.Create(OpCodes.Ldstr, str);
			else if (targetTypeRef.FullName == "System.Object")
				yield return Instruction.Create(OpCodes.Ldstr, str);
			else if (targetTypeRef.FullName == "System.Decimal") {
				decimal outdecimal;
				if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out outdecimal)) {
					var vardef = new VariableDefinition(module.ImportReference(("mscorlib", "System", "Decimal")));
					context.Body.Variables.Add(vardef);
					//Use an extra temp var so we can push the value to the stack, just like other cases
//					IL_0003:  ldstr "adecimal"
//					IL_0008:  ldc.i4.s 0x6f
//					IL_000a:  call class [mscorlib]System.Globalization.CultureInfo class [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
//					IL_000f:  ldloca.s 0
//					IL_0011:  call bool valuetype [mscorlib]System.Decimal::TryParse(string, valuetype [mscorlib]System.Globalization.NumberStyles, class [mscorlib]System.IFormatProvider, [out] valuetype [mscorlib]System.Decimal&)
//					IL_0016:  pop
					yield return Create(Ldstr, str);
					yield return Create(Ldc_I4, 0x6f); //NumberStyles.Number
					yield return Create(Call, module.ImportPropertyGetterReference(("mscorlib", "System.Globalization", "CultureInfo"),
																				   propertyName: "InvariantCulture", isStatic: true));
					yield return Create(Ldloca, vardef);
					yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Decimal"),
																		   methodName: "TryParse",
																		   parameterTypes: new[] {
																			   ("mscorlib", "System", "String"),
																			   ("mscorlib", "System.Globalization", "NumberStyles"),
																			   ("mscorlib", "System", "IFormatProvider"),
																			   ("mscorlib", "System", "Decimal"),
																		   },
																		   isStatic: true));
					yield return Create(Pop);
					yield return Create(Ldloc, vardef);
				} else {
					yield return Create(Ldc_I4_0);
					yield return Create(Newobj, module.ImportCtorReference(("mscorlib", "System", "Decimal"), parameterTypes: new[] { ("mscorlib", "System", "Int32") }));
				}
			} else if (implicitOperator != null) {
				yield return Create(Ldstr, node.Value as string);
				yield return Create(Call, module.ImportReference(implicitOperator));
			} else
				yield return Create(Ldnull);

			if (isNullable)
				yield return Create(Newobj, module.ImportReference(nullableCtor));
			if (originalTypeRef.IsValueType && boxValueTypes)
				yield return Create(Box, module.ImportReference(originalTypeRef));
		}

		static Instruction PushParsedEnum(TypeReference enumRef, string value, IXmlLineInfo lineInfo)
		{
			var enumDef = enumRef.ResolveCached();
			if (!enumDef.IsEnum)
				throw new InvalidOperationException();

			// The approved types for an enum are byte, sbyte, short, ushort, int, uint, long, or ulong.
			// https://msdn.microsoft.com/en-us/library/sbbt4032.aspx
			byte b = 0; sbyte sb = 0; short s = 0; ushort us = 0;
			int i = 0; uint ui = 0; long l = 0; ulong ul = 0;
			bool found = false;
			TypeReference typeRef = null;

			foreach (var field in enumDef.Fields)
				if (field.Name == "value__")
					typeRef = field.FieldType;

			if (typeRef == null)
				throw new ArgumentException();

			foreach (var v in value.Split(',')) {
				foreach (var field in enumDef.Fields) {
					if (field.Name == "value__")
						continue;
					if (field.Name == v.Trim()) {
						switch (typeRef.FullName) {
						case "System.Byte":
							b |= (byte)field.Constant;
							break;
						case "System.SByte":
							if (found)
								throw new XamlParseException($"Multi-valued enums are not valid on sbyte enum types", lineInfo);
							sb = (sbyte)field.Constant;
							break;
						case "System.Int16":
							s |= (short)field.Constant;
							break;
						case "System.UInt16":
							us |= (ushort)field.Constant;
							break;
						case "System.Int32":
							i |= (int)field.Constant;
							break;
						case "System.UInt32":
							ui |= (uint)field.Constant;
							break;
						case "System.Int64":
							l |= (long)field.Constant;
							break;
						case "System.UInt64":
							ul |= (ulong)field.Constant;
							break;
						}
						found = true;
					}
				}
			}

			if (!found)
				throw new XamlParseException($"Enum value not found for {value}", lineInfo);
				
			switch (typeRef.FullName) {
			case "System.Byte":
				return Instruction.Create(OpCodes.Ldc_I4, (int)b);
			case "System.SByte":
				return Instruction.Create(OpCodes.Ldc_I4, (int)sb);
			case "System.Int16":
				return Instruction.Create(OpCodes.Ldc_I4, (int)s);
			case "System.UInt16":
				return Instruction.Create(OpCodes.Ldc_I4, (int)us);
			case "System.Int32":
				return Instruction.Create(OpCodes.Ldc_I4, (int)i);
			case "System.UInt32":
				return Instruction.Create(OpCodes.Ldc_I4, (uint)ui);
			case "System.Int64":
				return Instruction.Create(OpCodes.Ldc_I4, (long)l);
			case "System.UInt64":
				return Instruction.Create(OpCodes.Ldc_I4, (ulong)ul);
			default:
				throw new XamlParseException($"Enum value not found for {value}", lineInfo);
			}
		}

		public static IEnumerable<Instruction> PushXmlLineInfo(this INode node, ILContext context)
		{
			var module = context.Body.Method.Module;

			var xmlLineInfo = node as IXmlLineInfo;
			if (xmlLineInfo == null) {
				yield return Create(Ldnull);
				yield break;
			}
			MethodReference ctor;
			if (xmlLineInfo.HasLineInfo()) {
				yield return Create(Ldc_I4, xmlLineInfo.LineNumber);
				yield return Create(Ldc_I4, xmlLineInfo.LinePosition);
				ctor = module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XmlLineInfo"), parameterTypes: new[] {
					("mscorlib", "System", "Int32"),
					("mscorlib", "System", "Int32"),
				});
			}
			else
				ctor = module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XmlLineInfo"), parameterTypes: null);
			yield return Create(Newobj, ctor);
		}

		public static IEnumerable<Instruction> PushParentObjectsArray(this INode node, ILContext context)
		{
			var module = context.Body.Method.Module;

			var nodes = new List<IElementNode>();
			INode n = node.Parent;
			while (n != null)
			{
				var en = n as IElementNode;
				if (en != null && context.Variables.ContainsKey(en))
					nodes.Add(en);
				n = n.Parent;
			}

			if (nodes.Count == 0 && context.ParentContextValues == null)
			{
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}

			if (nodes.Count == 0)
			{
				yield return Instruction.Create(OpCodes.Ldarg_0);
				yield return Instruction.Create(OpCodes.Ldfld, context.ParentContextValues);
				yield break;
			}

			//Compute parent object length
			if (context.ParentContextValues != null)
			{
				yield return Instruction.Create(OpCodes.Ldarg_0);
				yield return Instruction.Create(OpCodes.Ldfld, context.ParentContextValues);
				yield return Instruction.Create(OpCodes.Ldlen);
				yield return Instruction.Create(OpCodes.Conv_I4);
			}
			else
				yield return Instruction.Create(OpCodes.Ldc_I4_0);
			var parentObjectLength = new VariableDefinition(module.TypeSystem.Int32);
			context.Body.Variables.Add(parentObjectLength);
			yield return Instruction.Create(OpCodes.Stloc, parentObjectLength);

			//Create the final array
			yield return Instruction.Create(OpCodes.Ldloc, parentObjectLength);
			yield return Instruction.Create(OpCodes.Ldc_I4, nodes.Count);
			yield return Instruction.Create(OpCodes.Add);
			yield return Instruction.Create(OpCodes.Newarr, module.TypeSystem.Object);
			var finalArray = new VariableDefinition(module.ImportArrayReference(("mscorlib", "System", "Object")));
			context.Body.Variables.Add(finalArray);
			yield return Instruction.Create(OpCodes.Stloc, finalArray);

			//Copy original array to final
			if (context.ParentContextValues != null)
			{
				yield return Create(Ldarg_0);
				yield return Create(Ldfld, context.ParentContextValues); //sourceArray
				yield return Create(Ldc_I4_0); //sourceIndex
				yield return Create(Ldloc, finalArray); //destinationArray
				yield return Create(Ldc_I4, nodes.Count); //destinationIndex
				yield return Create(Ldloc, parentObjectLength); //length
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Array"),
																	   methodName: "Copy",
																	   parameterTypes: new[] {
																		   ("mscorlib", "System", "Array"),
																		   ("mscorlib", "System", "Int32"),
																		   ("mscorlib", "System", "Array"),
																		   ("mscorlib", "System", "Int32"),
																		   ("mscorlib", "System", "Int32"),
																	   },
																	   isStatic: true));
			}

			//Add nodes to array
			yield return Instruction.Create(OpCodes.Ldloc, finalArray);
			if (nodes.Count > 0)
			{
				for (var i = 0; i < nodes.Count; i++)
				{
					var en = nodes[i];
					yield return Instruction.Create(OpCodes.Dup);
					yield return Instruction.Create(OpCodes.Ldc_I4, i);
					yield return Instruction.Create(OpCodes.Ldloc, context.Variables[en]);
					if (context.Variables[en].VariableType.IsValueType)
						yield return Instruction.Create(OpCodes.Box, module.ImportReference(context.Variables[en].VariableType));
					yield return Instruction.Create(OpCodes.Stelem_Ref);
				}
			}
		}

		static IEnumerable<Instruction> PushTargetProperty(FieldReference bpRef, PropertyReference propertyRef, TypeReference declaringTypeReference, ModuleDefinition module)
		{
			if (bpRef != null) {
				yield return Create(Ldsfld, bpRef);
				yield break;
			}
			if (propertyRef != null) {
				yield return Create(Ldtoken, module.ImportReference(declaringTypeReference ?? propertyRef.DeclaringType));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Ldstr, propertyRef.Name);
				yield return Create(Call, module.ImportMethodReference(("System.Reflection.Extensions", "System.Reflection", "RuntimeReflectionExtensions"),
																	   methodName: "GetRuntimeProperty",
																	   parameterTypes: new[]{
																		   ("mscorlib", "System", "Type"),
																		   ("mscorlib", "System", "String"),
																	   },
																	   isStatic: true));
				yield break;
			}
			yield return Create(Ldnull);
			yield break;
		}

		public static IEnumerable<Instruction> PushServiceProvider(this INode node, ILContext context, FieldReference bpRef = null, PropertyReference propertyRef = null, TypeReference declaringTypeReference = null)
		{
			var module = context.Body.Method.Module;

#if NOSERVICEPROVIDER
			yield return Instruction.Create (OpCodes.Ldnull);
			yield break;
#endif

			var addService = module.ImportMethodReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XamlServiceProvider"),
														  methodName: "Add",
														  parameterTypes: new[] {
															  ("mscorlib", "System", "Type"),
															  ("mscorlib", "System", "Object"),
														  });

			yield return Create(Newobj, module.ImportCtorReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XamlServiceProvider"), parameterTypes: null));

			//Add a SimpleValueTargetProvider and register it as IProvideValueTarget and IReferenceProvider
			var pushParentIl = node.PushParentObjectsArray(context).ToList();
			if (pushParentIl[pushParentIl.Count - 1].OpCode != Ldnull) {
				yield return Create(Dup); //Keep the serviceProvider on the stack
				yield return Create(Ldtoken, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "IProvideValueTarget")));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));

				foreach (var instruction in pushParentIl)
					yield return instruction;

				foreach (var instruction in PushTargetProperty(bpRef, propertyRef, declaringTypeReference, module))
					yield return instruction;

				yield return Create(Newobj, module.ImportCtorReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "SimpleValueTargetProvider"), paramCount: 2));
				//store the provider so we can register it again with a different key
				yield return Create(Dup);
				var refProvider = new VariableDefinition(module.ImportReference(("mscorlib", "System", "Object")));
				context.Body.Variables.Add(refProvider);
				yield return Create(Stloc, refProvider);
				yield return Create(Callvirt, addService);

				yield return Create(Dup); //Keep the serviceProvider on the stack
				yield return Create(Ldtoken, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "IReferenceProvider")));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Ldloc, refProvider);
				yield return Create(Callvirt, addService);
			}

			//Add a XamlTypeResolver
			if (node.NamespaceResolver != null) {
				yield return Create(Dup); //Duplicate the serviceProvider
				yield return Create(Ldtoken, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "IXamlTypeResolver")));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Newobj, module.ImportCtorReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XmlNamespaceResolver"), parameterTypes: null));
				foreach (var kvp in node.NamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml)) {
					yield return Create(Dup); //dup the resolver
					yield return Create(Ldstr, kvp.Key);
					yield return Create(Ldstr, kvp.Value);
					yield return Create(Callvirt, module.ImportMethodReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XmlNamespaceResolver"),
																			   methodName: "Add",
																			   parameterTypes: new[] {
																				   ("mscorlib", "System", "String"),
																				   ("mscorlib", "System", "String"),
																			   }));
				}
				yield return Create(Ldtoken, context.Body.Method.DeclaringType);
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System.Reflection", "IntrospectionExtensions"), methodName: "GetTypeInfo", parameterTypes: new[] { ("mscorlib", "System", "Type") }, isStatic: true));
				yield return Create(Callvirt, module.ImportPropertyGetterReference(("mscorlib", "System.Reflection", "TypeInfo"), propertyName: "Assembly", flatten: true));
				yield return Create(Newobj, module.ImportCtorReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XamlTypeResolver"), paramCount: 2));
				yield return Create(Callvirt, addService);
			}

			if (node is IXmlLineInfo) {
				yield return Create(Dup); //Duplicate the serviceProvider
				yield return Create(Ldtoken, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "IXmlLineInfoProvider")));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				foreach (var instruction in node.PushXmlLineInfo(context))
					yield return instruction;
				yield return Create(Newobj, module.ImportCtorReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XmlLineInfoProvider"), parameterTypes: new[] { ("System.Xml.ReaderWriter", "System.Xml", "IXmlLineInfo") }));
				yield return Create(Callvirt, addService);
			}
		}
	}
}