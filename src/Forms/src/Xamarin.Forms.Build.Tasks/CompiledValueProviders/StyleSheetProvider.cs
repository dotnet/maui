using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Xaml;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Xamarin.Forms.Core.XamlC
{
	class StyleSheetProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			INode sourceNode = null;
			((IElementNode)node).Properties.TryGetValue(new XmlName("", "Source"), out sourceNode);
			if (sourceNode == null)
				((IElementNode)node).Properties.TryGetValue(new XmlName(XamlParser.XFUri, "Source"), out sourceNode);

			INode styleNode = null;
			if (!((IElementNode)node).Properties.TryGetValue(new XmlName("", "Style"), out styleNode) &&
				!((IElementNode)node).Properties.TryGetValue(new XmlName(XamlParser.XFUri, "Style"), out styleNode) &&
				((IElementNode)node).CollectionItems.Count == 1)
				styleNode = ((IElementNode)node).CollectionItems[0];

			if (sourceNode != null && styleNode != null)
				throw new BuildException(BuildExceptionCode.StyleSheetSourceOrContent, node, null);

			if (sourceNode == null && styleNode == null)
				throw new BuildException(BuildExceptionCode.StyleSheetNoSourceOrContent, node, null);

			if (styleNode != null && !(styleNode is ValueNode))
				throw new BuildException(BuildExceptionCode.StyleSheetStyleNotALiteral, node, null);

			if (sourceNode != null && !(sourceNode is ValueNode))
				throw new BuildException(BuildExceptionCode.StyleSheetSourceNotALiteral, node, null);

			if (styleNode != null)
			{
				var style = (styleNode as ValueNode).Value as string;
				yield return Create(Ldstr, style);
				yield return Create(Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.StyleSheets", "StyleSheet"),
																	   methodName: "FromString",
																	   parameterTypes: new[] { ("mscorlib", "System", "String") },
																	   isStatic: true));
			}
			else
			{
				var source = (sourceNode as ValueNode)?.Value as string;
				INode rootNode = node;
				while (!(rootNode is ILRootNode))
					rootNode = rootNode.Parent;

				var rootTargetPath = RDSourceTypeConverter.GetPathForType(module, ((ILRootNode)rootNode).TypeReference);
				var uri = new Uri(source, UriKind.Relative);

				var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);
				//fail early
				if (XamlCTask.GetResourceIdForPath(module, resourcePath) == null)
					throw new XamlParseException($"Resource '{source}' not found.", node);

				yield return Create(Ldstr, resourcePath); //resourcePath

				yield return Create(Ldtoken, module.ImportReference(((ILRootNode)rootNode).TypeReference));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Call, module.ImportMethodReference(("mscorlib", "System.Reflection", "IntrospectionExtensions"), methodName: "GetTypeInfo", parameterTypes: new[] { ("mscorlib", "System", "Type") }, isStatic: true));
				yield return Create(Callvirt, module.ImportPropertyGetterReference(("mscorlib", "System.Reflection", "TypeInfo"), propertyName: "Assembly", flatten: true)); //assembly

				foreach (var instruction in node.PushXmlLineInfo(context))
					yield return instruction; //lineinfo

				yield return Create(Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms.StyleSheets", "StyleSheet"),
																	   methodName: "FromResource",
																	   parameterTypes: new[] { ("mscorlib", "System", "String"), ("mscorlib", "System.Reflection", "Assembly"), ("System.Xml.ReaderWriter", "System.Xml", "IXmlLineInfo") },
																	   isStatic: true));
			}

			//the variable is of type `object`. fix that
			var vardef = new VariableDefinition(module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.StyleSheets", "StyleSheet")));
			yield return Create(Stloc, vardef);
			vardefref.VariableDefinition = vardef;
		}
	}
}