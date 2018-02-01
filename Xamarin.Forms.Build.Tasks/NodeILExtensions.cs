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
				var typeConvAttribute = typeConverter.GetCustomAttribute(module.ImportReference(typeof(TypeConversionAttribute)));
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
			var compiledConverterName = typeConverter?.GetCustomAttribute (module.ImportReference(typeof(ProvideCompiledAttribute)))?.ConstructorArguments?.First().Value as string;
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
				var isExtendedConverter = typeConverter.ImplementsInterface(module.ImportReference(typeof (IExtendedTypeConverter)));
				var typeConverterCtor = typeConverter.Resolve().Methods.Single(md => md.IsConstructor && md.Parameters.Count == 0 && !md.IsStatic);
				var typeConverterCtorRef = module.ImportReference(typeConverterCtor);
				var convertFromInvariantStringDefinition = isExtendedConverter
					? module.ImportReference(typeof (IExtendedTypeConverter))
						.Resolve()
						.Methods.FirstOrDefault(md => md.Name == "ConvertFromInvariantString" && md.Parameters.Count == 2)
					: typeConverter.Resolve()
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
			if (targetTypeRef.Resolve().FullName == "System.Nullable`1")
			{
				var nullableTypeRef = targetTypeRef;
				targetTypeRef = ((GenericInstanceType)targetTypeRef).GenericArguments[0];
				isNullable = true;
				nullableCtor = originalTypeRef.GetMethods(md => md.IsConstructor && md.Parameters.Count == 1, module).Single().Item1;
				nullableCtor = nullableCtor.ResolveGenericParameters(nullableTypeRef, module);
			}

			var implicitOperator = module.TypeSystem.String.GetImplicitOperatorTo(targetTypeRef, module);

			//Obvious Built-in conversions
			if (targetTypeRef.Resolve().BaseType != null && targetTypeRef.Resolve().BaseType.FullName == "System.Enum")
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
				yield return Instruction.Create(OpCodes.Ldc_I4, UInt32.Parse(str, CultureInfo.InvariantCulture));
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
				var timeSpanCtor =
					module.ImportReference(typeof(TimeSpan))
						.Resolve()
						.Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 1);
				var timeSpanCtorRef = module.ImportReference(timeSpanCtor);

				yield return Instruction.Create(OpCodes.Ldc_I8, ticks);
				yield return Instruction.Create(OpCodes.Newobj, timeSpanCtorRef);
			} else if (targetTypeRef.FullName == "System.DateTime") {
				var dt = DateTime.Parse(str, CultureInfo.InvariantCulture);
				var ticks = dt.Ticks;
				var dateTimeCtor =
					module.ImportReference(typeof(DateTime))
						.Resolve()
						.Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 1);
				var dateTimeCtorRef = module.ImportReference(dateTimeCtor);

				yield return Instruction.Create(OpCodes.Ldc_I8, ticks);
				yield return Instruction.Create(OpCodes.Newobj, dateTimeCtorRef);
			} else if (targetTypeRef.FullName == "System.String" && str.StartsWith("{}", StringComparison.Ordinal))
				yield return Instruction.Create(OpCodes.Ldstr, str.Substring(2));
			else if (targetTypeRef.FullName == "System.String")
				yield return Instruction.Create(OpCodes.Ldstr, str);
			else if (targetTypeRef.FullName == "System.Object")
				yield return Instruction.Create(OpCodes.Ldstr, str);
			else if (targetTypeRef.FullName == "System.Decimal") {
				decimal outdecimal;
				if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out outdecimal)) {
					var vardef = new VariableDefinition(context.Body.Method.Module.ImportReference(typeof(decimal)));
					context.Body.Variables.Add(vardef);
					//Use an extra temp var so we can push the value to the stack, just like other cases
					//					IL_0003:  ldstr "adecimal"
					//					IL_0008:  ldc.i4.s 0x6f
					//					IL_000a:  call class [mscorlib]System.Globalization.CultureInfo class [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
					//					IL_000f:  ldloca.s 0
					//					IL_0011:  call bool valuetype [mscorlib]System.Decimal::TryParse(string, valuetype [mscorlib]System.Globalization.NumberStyles, class [mscorlib]System.IFormatProvider, [out] valuetype [mscorlib]System.Decimal&)
					//					IL_0016:  pop
					yield return Instruction.Create(OpCodes.Ldstr, str);
					yield return Instruction.Create(OpCodes.Ldc_I4, 0x6f); //NumberStyles.Number
					var getInvariantInfo =
						context.Body.Method.Module.ImportReference(typeof(CultureInfo))
							.Resolve()
							.Properties.FirstOrDefault(pd => pd.Name == "InvariantCulture")
							.GetMethod;
					var getInvariant = context.Body.Method.Module.ImportReference(getInvariantInfo);
					yield return Instruction.Create(OpCodes.Call, getInvariant);
					yield return Instruction.Create(OpCodes.Ldloca, vardef);
					var tryParseInfo =
						context.Body.Method.Module.ImportReference(typeof(decimal))
							.Resolve()
							.Methods.FirstOrDefault(md => md.Name == "TryParse" && md.Parameters.Count == 4);
					var tryParse = context.Body.Method.Module.ImportReference(tryParseInfo);
					yield return Instruction.Create(OpCodes.Call, tryParse);
					yield return Instruction.Create(OpCodes.Pop);
					yield return Instruction.Create(OpCodes.Ldloc, vardef);
				} else {
					yield return Instruction.Create(OpCodes.Ldc_I4_0);
					var decimalctorinfo =
						context.Body.Method.Module.ImportReference(typeof(decimal))
							.Resolve()
							.Methods.FirstOrDefault(
								md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters[0].ParameterType.FullName == "System.Int32");
					var decimalctor = context.Body.Method.Module.ImportReference(decimalctorinfo);
					yield return Instruction.Create(OpCodes.Newobj, decimalctor);
				}
			} else if (implicitOperator != null) {
				yield return Instruction.Create(OpCodes.Ldstr, node.Value as string);
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(implicitOperator));
			} else
				yield return Instruction.Create(OpCodes.Ldnull);

			if (isNullable)
				yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(nullableCtor));
			if (originalTypeRef.IsValueType && boxValueTypes)
				yield return Instruction.Create(OpCodes.Box, module.ImportReference(originalTypeRef));
		}

		static Instruction PushParsedEnum(TypeReference enumRef, string value, IXmlLineInfo lineInfo)
		{
			var enumDef = enumRef.Resolve();
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
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}
			MethodReference ctor;
			if (xmlLineInfo.HasLineInfo()) {
				yield return Instruction.Create(OpCodes.Ldc_I4, xmlLineInfo.LineNumber);
				yield return Instruction.Create(OpCodes.Ldc_I4, xmlLineInfo.LinePosition);
				ctor = module.ImportReference(typeof(XmlLineInfo).GetConstructor(new[] { typeof(int), typeof(int) }));
			}
			else
				ctor = module.ImportReference(typeof(XmlLineInfo).GetConstructor(new Type[] { }));
			yield return Instruction.Create(OpCodes.Newobj, ctor);
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
			var finalArray = new VariableDefinition(module.ImportReference(typeof (object[])));
			context.Body.Variables.Add(finalArray);
			yield return Instruction.Create(OpCodes.Stloc, finalArray);

			//Copy original array to final
			if (context.ParentContextValues != null)
			{
				yield return Instruction.Create(OpCodes.Ldarg_0);
				yield return Instruction.Create(OpCodes.Ldfld, context.ParentContextValues); //sourceArray
				yield return Instruction.Create(OpCodes.Ldc_I4_0); //sourceIndex
				yield return Instruction.Create(OpCodes.Ldloc, finalArray); //destinationArray
				yield return Instruction.Create(OpCodes.Ldc_I4, nodes.Count); //destinationIndex
				yield return Instruction.Create(OpCodes.Ldloc, parentObjectLength); //length
				var arrayCopy =
					module.ImportReference(typeof (Array))
						.Resolve()
						.Methods.First(
							md =>
								md.Name == "Copy" && md.Parameters.Count == 5 &&
								md.Parameters[1].ParameterType.FullName == module.TypeSystem.Int32.FullName);
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(arrayCopy));
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
				yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
				yield break;
			}
			if (propertyRef != null) {
//				IL_0000:  ldtoken [mscorlib]System.String
//				IL_0005:  call class [mscorlib]System.Type class [mscorlib] System.Type::GetTypeFromHandle(valuetype [mscorlib] System.RuntimeTypeHandle)
//				IL_000a:  ldstr "Foo"
//				IL_000f:  callvirt instance class [mscorlib] System.Reflection.PropertyInfo class [mscorlib] System.Type::GetProperty(string)
				var getTypeFromHandle = module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle", new [] { typeof(RuntimeTypeHandle) }));
				var getPropertyInfo = module.ImportReference(typeof(Type).GetMethod("GetProperty", new [] { typeof(string) }));
				yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(declaringTypeReference ?? propertyRef.DeclaringType));
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(getTypeFromHandle));
				yield return Instruction.Create(OpCodes.Ldstr, propertyRef.Name);
				yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(getPropertyInfo));
				yield break;
			}
			yield return Instruction.Create(OpCodes.Ldnull);
			yield break;
		}

		public static IEnumerable<Instruction> PushServiceProvider(this INode node, ILContext context, FieldReference bpRef = null, PropertyReference propertyRef = null, TypeReference declaringTypeReference = null)
		{
			var module = context.Body.Method.Module;

#if NOSERVICEPROVIDER
			yield return Instruction.Create (OpCodes.Ldnull);
			yield break;
#endif

			var ctorinfo = typeof (XamlServiceProvider).GetConstructor(new Type[] { });
			var ctor = module.ImportReference(ctorinfo);

			var addServiceInfo = typeof (XamlServiceProvider).GetMethod("Add", new[] { typeof (Type), typeof (object) });
			var addService = module.ImportReference(addServiceInfo);

			var getTypeFromHandle =
				module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
			var getTypeInfo = module.ImportReference(typeof(System.Reflection.IntrospectionExtensions).GetMethod("GetTypeInfo", new Type[] { typeof(Type)}));
			var getAssembly = module.ImportReference(typeof(System.Reflection.TypeInfo).GetProperty("Assembly").GetMethod);

			yield return Instruction.Create(OpCodes.Newobj, ctor);

			//Add a SimpleValueTargetProvider
			var pushParentIl = node.PushParentObjectsArray(context).ToList();
			if (pushParentIl[pushParentIl.Count - 1].OpCode != OpCodes.Ldnull)
			{
				yield return Instruction.Create(OpCodes.Dup); //Keep the serviceProvider on the stack
				yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(typeof (IProvideValueTarget)));
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(getTypeFromHandle));

				foreach (var instruction in pushParentIl)
					yield return instruction;

				foreach (var instruction in PushTargetProperty(bpRef, propertyRef, declaringTypeReference, module))
					yield return instruction;

				var targetProviderCtor =
					module.ImportReference(typeof (SimpleValueTargetProvider).GetConstructor(new[] { typeof (object[]), typeof(object) }));
				yield return Instruction.Create(OpCodes.Newobj, targetProviderCtor);
				yield return Instruction.Create(OpCodes.Callvirt, addService);
			}

			//Add a NamescopeProvider
			if (context.Scopes.ContainsKey(node))
			{
				yield return Instruction.Create(OpCodes.Dup); //Dupicate the serviceProvider
				yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(typeof (INameScopeProvider)));
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(getTypeFromHandle));
				var namescopeProviderCtor = module.ImportReference(typeof (NameScopeProvider).GetConstructor(new Type[] { }));
				yield return Instruction.Create(OpCodes.Newobj, namescopeProviderCtor);
				yield return Instruction.Create(OpCodes.Dup); //Duplicate the namescopeProvider
				var setNamescope = module.ImportReference(typeof (NameScopeProvider).GetProperty("NameScope").GetSetMethod());

				yield return Instruction.Create(OpCodes.Ldloc, context.Scopes[node].Item1);
				yield return Instruction.Create(OpCodes.Callvirt, setNamescope);
				yield return Instruction.Create(OpCodes.Callvirt, addService);
			}

			//Add a XamlTypeResolver
			if (node.NamespaceResolver != null)
			{
				yield return Create(Dup); //Dupicate the serviceProvider
				yield return Create(Ldtoken, module.ImportReference(typeof (IXamlTypeResolver)));
				yield return Create(Call, module.ImportReference(getTypeFromHandle));
				var xmlNamespaceResolverCtor = module.ImportReference(typeof (XmlNamespaceResolver).GetConstructor(new Type[] { }));
				var addNamespace = module.ImportReference(typeof (XmlNamespaceResolver).GetMethod("Add"));
				yield return Create(Newobj, xmlNamespaceResolverCtor);
				foreach (var kvp in node.NamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml))
				{
					yield return Create(Dup); //dup the resolver
					yield return Create(Ldstr, kvp.Key);
					yield return Create(Ldstr, kvp.Value);
					yield return Create(Callvirt, addNamespace);
				}
				yield return Create(Ldtoken, context.Body.Method.DeclaringType);
				yield return Create(Call, module.ImportReference(getTypeFromHandle));
				yield return Create(Call, module.ImportReference(getTypeInfo));
				yield return Create(Callvirt, getAssembly);
				var xtr = module.ImportReference(typeof (XamlTypeResolver)).Resolve();
				var xamlTypeResolverCtor = module.ImportReference(xtr.Methods.First(md => md.IsConstructor && md.Parameters.Count == 2));
				yield return Create(Newobj, xamlTypeResolverCtor);
				yield return Create(Callvirt, addService);
			}

			if (node is IXmlLineInfo)
			{
				yield return Instruction.Create(OpCodes.Dup); //Dupicate the serviceProvider
				yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(typeof (IXmlLineInfoProvider)));
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(getTypeFromHandle));

				foreach (var instruction in node.PushXmlLineInfo(context))
					yield return instruction;

				var lip = module.ImportReference(typeof (XmlLineInfoProvider)).Resolve();
				var lineInfoProviderCtor = module.ImportReference(lip.Methods.First(md => md.IsConstructor && md.Parameters.Count == 1));
				yield return Instruction.Create(OpCodes.Newobj, lineInfoProviderCtor);
				yield return Instruction.Create(OpCodes.Callvirt, addService);
			}
		}
	}
}
