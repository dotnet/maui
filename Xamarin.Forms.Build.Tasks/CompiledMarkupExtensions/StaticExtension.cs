using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;
using System.Xml;

using static System.String;

namespace Xamarin.Forms.Build.Tasks
{
	class StaticExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
		{
			INode ntype;
			if (!node.Properties.TryGetValue(new XmlName("", "Member"), out ntype))
				ntype = node.CollectionItems[0];
			var member = ((ValueNode)ntype).Value as string;

			if (IsNullOrEmpty(member) || !member.Contains(".")) {
				var lineInfo = node as IXmlLineInfo;
				throw new XamlParseException("Syntax for x:Static is [Member=][prefix:]typeName.staticMemberName", lineInfo);
			}

			var dotIdx = member.LastIndexOf('.');
			var typename = member.Substring(0, dotIdx);
			var membername = member.Substring(dotIdx + 1);

			var typeRef = module.ImportReference(XmlTypeExtensions.GetTypeReference(typename, module, node as BaseNode));
			var fieldRef = GetFieldReference(typeRef, membername, module);
			var propertyDef = GetPropertyDefinition(typeRef, membername, module);

			if (fieldRef == null && propertyDef == null)
				throw new XamlParseException($"x:Static: unable to find a public static field, static property, const or enum value named {membername} in {typename}", node as IXmlLineInfo);

			var fieldDef = fieldRef?.Resolve();
			if (fieldRef != null) {
				memberRef = fieldRef.FieldType;
				if (!fieldDef.HasConstant)
					return new [] { Instruction.Create(OpCodes.Ldsfld, fieldRef) };

				//Constants can be numbers, Boolean values, strings, or a null reference. (https://msdn.microsoft.com/en-us/library/e6w8fe1b.aspx)
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Boolean))
					return new [] { Instruction.Create(((bool)fieldDef.Constant) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.String))
					return new [] { Instruction.Create(OpCodes.Ldstr, (string)fieldDef.Constant) };
				if (fieldDef.Constant == null)
					return new [] { Instruction.Create(OpCodes.Ldnull) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Char))
					return new [] { Instruction.Create(OpCodes.Ldc_I4, (char)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Single))
					return new [] { Instruction.Create(OpCodes.Ldc_R4, (float)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Double))
					return new [] { Instruction.Create(OpCodes.Ldc_R8, (double)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Byte) || TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Int16) || TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Int32))
					return new [] { Instruction.Create(OpCodes.Ldc_I4, (int)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.SByte) || TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.UInt16) || TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.UInt32))
					return new [] { Instruction.Create(OpCodes.Ldc_I4, (uint)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.Int64))
					return new [] { Instruction.Create(OpCodes.Ldc_I8, (long)fieldDef.Constant) };
				if (TypeRefComparer.Default.Equals(memberRef, module.TypeSystem.UInt64))
					return new [] { Instruction.Create(OpCodes.Ldc_I8, (ulong)fieldDef.Constant) };

				//enum values
				if (memberRef.ResolveCached().IsEnum) {
					if (fieldDef.Constant is long)
						return new [] { Instruction.Create(OpCodes.Ldc_I8, (long)fieldDef.Constant) };
					if (fieldDef.Constant is ulong)
						return new [] { Instruction.Create(OpCodes.Ldc_I8, (ulong)fieldDef.Constant) };
					if (fieldDef.Constant is uint)
						return new [] { Instruction.Create(OpCodes.Ldc_I4, (uint)fieldDef.Constant) };
					//everything else will cast just fine to an int
					return new [] { Instruction.Create(OpCodes.Ldc_I4, (int)fieldDef.Constant) };
				}
			}

			memberRef = propertyDef.PropertyType;
			var getterDef = module.ImportReference(propertyDef.GetMethod);
			return new [] { Instruction.Create(OpCodes.Call, getterDef) };
		}

		public static FieldReference GetFieldReference(TypeReference typeRef, string fieldName, ModuleDefinition module)
		{
			TypeReference declaringTypeReference;
			FieldReference fRef = typeRef.GetField(fd => fd.Name == fieldName &&
			                                       fd.IsStatic &&
			                                       fd.IsPublic, out declaringTypeReference);
			if (fRef != null) {
				fRef = module.ImportReference(fRef.ResolveGenericParameters(declaringTypeReference));
				fRef.FieldType = module.ImportReference(fRef.FieldType);
			}
			return fRef;
		}

		public static PropertyDefinition GetPropertyDefinition(TypeReference typeRef, string propertyName, ModuleDefinition module)
		{
			TypeReference declaringTypeReference;
			PropertyDefinition pDef = typeRef.GetProperty(pd => pd.Name == propertyName &&
			                                              pd.GetMethod.IsPublic &&
			                                              pd.GetMethod.IsStatic, out declaringTypeReference);
			return pDef;
		}
	}
}
