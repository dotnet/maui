using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class TypeExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(ElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
		{
			memberRef = module.ImportReference(context.Cache, ("mscorlib", "System", "Type"));
			var name = new XmlName("", "TypeName");

			if (!node.Properties.TryGetValue(name, out INode typeNameNode) && node.CollectionItems.Any())
				typeNameNode = node.CollectionItems[0];

			if (!(typeNameNode is ValueNode valueNode))
				throw new BuildException(BuildExceptionCode.PropertyMissing, node as IXmlLineInfo, null, "TypeName", typeof(Microsoft.Maui.Controls.Xaml.TypeExtension));

			if (!node.Properties.ContainsKey(name))
			{
				node.Properties[name] = typeNameNode;
				node.CollectionItems.Clear();
			}

			var typeref = module.ImportReference(XmlTypeExtensions.GetTypeReference(context.Cache, valueNode.Value as string, module, node as BaseNode));

			context.TypeExtensions[node] = typeref ?? throw new BuildException(BuildExceptionCode.TypeResolution, node as IXmlLineInfo, null, valueNode.Value);

			return new List<Instruction> {
				Create(Ldtoken, module.ImportReference(typeref)),
				Create(Call, module.ImportMethodReference(context.Cache, ("mscorlib", "System", "Type"),
														  methodName: "GetTypeFromHandle",
														  parameterTypes: [("mscorlib", "System", "RuntimeTypeHandle")],
														  isStatic: true)),
			};
		}
	}
}