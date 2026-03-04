using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
{
	class StyleSheetProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			((ElementNode)node).Properties.TryGetValue(new XmlName("", "Source"), out INode sourceNode);
			if (sourceNode == null)
				((ElementNode)node).Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Source"), out sourceNode);

			INode styleNode = null;
			if (!((ElementNode)node).Properties.TryGetValue(new XmlName("", "Style"), out styleNode) &&
				!((ElementNode)node).Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Style"), out styleNode) &&
				((ElementNode)node).CollectionItems.Count == 1)
				styleNode = ((ElementNode)node).CollectionItems[0];

			if (sourceNode != null && styleNode != null)
				throw new BuildException(BuildExceptionCode.StyleSheetSourceOrContent, node, null);

			if (sourceNode == null && styleNode == null)
				throw new BuildException(BuildExceptionCode.StyleSheetNoSourceOrContent, node, null);

			if (styleNode != null && styleNode is not ValueNode)
				throw new BuildException(BuildExceptionCode.StyleSheetStyleNotALiteral, node, null);

			if (sourceNode != null && sourceNode is not ValueNode)
				throw new BuildException(BuildExceptionCode.StyleSheetSourceNotALiteral, node, null);

			if (styleNode != null)
			{
				var style = (styleNode as ValueNode).Value as string;
				yield return Create(Ldstr, style);
				yield return Create(Call, module.ImportMethodReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.StyleSheets", "StyleSheet"),
																	   methodName: "FromString",
																	   parameterTypes: [("mscorlib", "System", "String")],
																	   isStatic: true));
			}
			else
			{
				var source = (sourceNode as ValueNode)?.Value as string;
				INode rootNode = node;
				while (rootNode is not ILRootNode)
					rootNode = rootNode.Parent;

				var rootTargetPath = RDSourceTypeConverter.GetPathForType(context, module, ((ILRootNode)rootNode).TypeReference);
				var uri = new Uri(source, UriKind.Relative);

				var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);
				//fail early
				if (XamlCTask.GetResourceIdForPath(context.Cache, module, resourcePath) == null)
					throw new XamlParseException($"Resource '{source}' not found.", node);

				yield return Create(Ldstr, resourcePath); //resourcePath

				yield return Create(Ldtoken, module.ImportReference(((ILRootNode)rootNode).TypeReference));
				yield return Create(Call, module.ImportMethodReference(context.Cache, ("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Callvirt, module.ImportPropertyGetterReference(context.Cache, ("mscorlib", "System", "Type"), propertyName: "Assembly", flatten: true)); //assembly

				foreach (var instruction in node.PushXmlLineInfo(context))
					yield return instruction; //lineinfo

				yield return Create(Call, module.ImportMethodReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.StyleSheets", "StyleSheet"),
																	   methodName: "FromResource",
																	   parameterTypes: [("mscorlib", "System", "String"), ("mscorlib", "System.Reflection", "Assembly"), ("System.Xml.ReaderWriter", "System.Xml", "IXmlLineInfo")],
																	   isStatic: true));
			}

			//the variable is of type `object`. fix that
			var vardef = new VariableDefinition(module.ImportReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.StyleSheets", "StyleSheet")));
			yield return Create(Stloc, vardef);
			vardefref.VariableDefinition = vardef;
		}
	}
}