using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Core.XamlC
{
	class RDSourceTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;
			var body = context.Body;

			INode rootNode = node;
			while (!(rootNode is ILRootNode))
				rootNode = rootNode.Parent;

			var rdNode = node.Parent as IElementNode;

			var rootNs = ((ILRootNode)rootNode).TypeReference.GetCustomAttribute(module.ImportReference(typeof(XamlResourceIdAttribute))).ConstructorArguments[0].Value as string;
			var rootResourceId = ((ILRootNode)rootNode).TypeReference.GetCustomAttribute(module.ImportReference(typeof(XamlResourceIdAttribute))).ConstructorArguments[1].Value as string;
			var uri = new Uri(value, UriKind.RelativeOrAbsolute);
			var resourceId = ResourceDictionary.RDSourceTypeConverter.ComputeResourceId(uri, rootResourceId, rootNs);

			//abuse the converter, produce some side effect, but leave the stack untouched
			//public void SetAndLoadSource(Uri value, string resourceID, Assembly assembly, System.Xml.IXmlLineInfo lineInfo)
			yield return Instruction.Create(OpCodes.Ldloc, context.Variables[rdNode]); //the resourcedictionary
			foreach (var instruction in (new UriTypeConverter()).ConvertFromString(value, context, node))
				yield return instruction; //the Uri

			//keep the Uri for later
			yield return Instruction.Create(OpCodes.Dup);
			var uriVarDef = new VariableDefinition(module.ImportReference(typeof(Uri)));
			body.Variables.Add(uriVarDef);
			yield return Instruction.Create(OpCodes.Stloc, uriVarDef);

			yield return Instruction.Create(OpCodes.Ldstr, resourceId); //resourceId

			var getTypeFromHandle = module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
			var getAssembly = module.ImportReference(typeof(Type).GetProperty("Assembly").GetGetMethod());
			yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(((ILRootNode)rootNode).TypeReference));
			yield return Instruction.Create(OpCodes.Call, module.ImportReference(getTypeFromHandle));
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(getAssembly)); //assembly

			foreach (var instruction in node.PushXmlLineInfo(context))
				yield return instruction; //lineinfo

			var setAndLoadSource = module.ImportReference(typeof(ResourceDictionary).GetMethod("SetAndLoadSource"));
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(setAndLoadSource));

			//ldloc the stored uri as return value
			yield return Instruction.Create(OpCodes.Ldloc, uriVarDef);
		}
	}
}