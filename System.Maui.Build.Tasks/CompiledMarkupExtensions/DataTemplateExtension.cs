using System.Linq;
using System.Xml;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Maui.Xaml;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace System.Maui.Build.Tasks
{
	class DataTemplateExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef)
		{
			typeRef = module.ImportReference(("System.Maui.Core", "System.Maui", "DataTemplate"));
			var name = new XmlName("", "TypeName");

			if (!node.Properties.TryGetValue(name, out INode typeNameNode) && node.CollectionItems.Any())
				typeNameNode = node.CollectionItems[0];

			if (!(typeNameNode is ValueNode valueNode))
				throw new XamlParseException("TypeName isn't set.", node as XmlLineInfo);

			var contentTypeRef = module.ImportReference(XmlTypeExtensions.GetTypeReference(valueNode.Value as string, module, node as BaseNode))
				?? throw new XamlParseException($"Can't resolve type `{valueNode.Value}'.", node as IXmlLineInfo);

			var dataTemplateCtor = module.ImportCtorReference(typeRef, new[] { module.ImportReference(("mscorlib", "System", "Type")) });
			return new List<Instruction> {
				Create(Ldtoken, module.ImportReference(contentTypeRef)),
				Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true)),
				Create(Newobj, dataTemplateCtor),
			};
		}
	}
}
