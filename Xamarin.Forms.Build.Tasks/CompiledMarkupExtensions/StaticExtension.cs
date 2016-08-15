using System;
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
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, out TypeReference memberRef)
		{
			INode ntype;
			if (!node.Properties.TryGetValue(new XmlName("", "Member"), out ntype))
				ntype = node.CollectionItems [0];
			var member = ((ValueNode)ntype).Value as string;

			if (IsNullOrEmpty(member) || !member.Contains(".")) {
				var lineInfo = node as IXmlLineInfo;
				throw new XamlParseException("Syntax for x:Static is [Member=][prefix:]typeName.staticMemberName", lineInfo);
			}

			var dotIdx = member.LastIndexOf('.');
			var typename = member.Substring(0, dotIdx);
			var membername = member.Substring(dotIdx + 1);

			var typeRef = GetTypeReference(typename, module, node);
			var fieldRef = GetFieldReference(typeRef, membername, module);
			var propertyDef = GetPropertyDefinition(typeRef, membername, module);

			if (fieldRef == null && propertyDef == null)
				throw new XamlParseException(Format("x:Static: unable to find a public static field or property named {0} in {1}", membername, typename), node as IXmlLineInfo);

			if (fieldRef != null) {
				memberRef = fieldRef.FieldType;
				return new [] { Instruction.Create(OpCodes.Ldsfld, fieldRef) };
			}

			memberRef = propertyDef.PropertyType;
			var getterDef = propertyDef.GetMethod;
			return new [] { Instruction.Create(OpCodes.Call, getterDef)};
		}


		public static TypeReference GetTypeReference(string xmlType, ModuleDefinition module, IElementNode node)
		{
			var split = xmlType.Split(':');
			if (split.Length > 2)
				throw new Xaml.XamlParseException(string.Format("Type \"{0}\" is invalid", xmlType), node as IXmlLineInfo);

			string prefix, name;
			if (split.Length == 2) {
				prefix = split [0];
				name = split [1];
			} else {
				prefix = "";
				name = split [0];
			}
			var namespaceuri = node.NamespaceResolver.LookupNamespace(prefix) ?? "";
			return XmlTypeExtensions.GetTypeReference(new XmlType(namespaceuri, name, null), module, node as IXmlLineInfo);
		}

		public static FieldReference GetFieldReference(TypeReference typeRef, string fieldName, ModuleDefinition module)
		{
			TypeReference declaringTypeReference;
			FieldReference fRef = typeRef.GetField(fd => fd.Name == fieldName &&
			                                       fd.IsStatic &&
			                                       fd.IsPublic, out declaringTypeReference);
			if (fRef != null) {
				fRef = module.Import(fRef.ResolveGenericParameters(declaringTypeReference));
				fRef.FieldType = module.Import(fRef.FieldType);
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