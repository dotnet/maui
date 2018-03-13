using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;
using System.Xml;

namespace Xamarin.Forms.Build.Tasks
{
	class TypeExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
		{
			memberRef = module.ImportReference(("mscorlib", "System", "Type"));
			INode typeNameNode;

			var name = new XmlName("", "TypeName");
			if (!node.Properties.TryGetValue(name, out typeNameNode) && node.CollectionItems.Any())
				typeNameNode = node.CollectionItems[0];

			var valueNode = typeNameNode as ValueNode;
			if (valueNode == null)
				throw new XamlParseException("TypeName isn't set.", node as XmlLineInfo);

			if (!node.Properties.ContainsKey(name)) {
				node.Properties[name] = typeNameNode;
				node.CollectionItems.Clear();
			}

			var typeref = module.ImportReference(XmlTypeExtensions.GetTypeReference(valueNode.Value as string, module, node as BaseNode));
			if (typeref == null)
				throw new XamlParseException($"Can't resolve type `{valueNode.Value}'.", node as IXmlLineInfo);

			context.TypeExtensions[node] = typeref;

			return new List<Instruction> {
				Instruction.Create(OpCodes.Ldtoken, module.ImportReference(typeref)),
				Instruction.Create(OpCodes.Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", paramCount: 1, predicate: md => md.IsStatic)),

			};
		}
	}
}