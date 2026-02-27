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
	class DataTemplateExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(ElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef)
		{
			typeRef = module.ImportReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "DataTemplate"));
			var name = new XmlName("", "TypeName");

			if (!node.Properties.TryGetValue(name, out INode typeNameNode) && node.CollectionItems.Any())
				typeNameNode = node.CollectionItems[0];

			if (typeNameNode is not ValueNode valueNode)
				throw new BuildException(BuildExceptionCode.PropertyMissing, node as IXmlLineInfo, null, "TypeName", typeof(Microsoft.Maui.Controls.Xaml.DataTemplateExtension));

			var contentTypeRef = module.ImportReference(XmlTypeExtensions.GetTypeReference(context.Cache, valueNode.Value as string, module, node as BaseNode))
				?? throw new BuildException(BuildExceptionCode.TypeResolution, node as IXmlLineInfo, null, valueNode.Value);

			var dataTemplateCtor = module.ImportCtorReference(context.Cache, typeRef, new[] { module.ImportReference(context.Cache, ("mscorlib", "System", "Type")) });
			return [
				Create(Ldtoken, module.ImportReference(contentTypeRef)),
				Create(Call, module.ImportMethodReference(context.Cache, ("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true)),
				Create(Newobj, dataTemplateCtor),
			];
		}
	}
}
