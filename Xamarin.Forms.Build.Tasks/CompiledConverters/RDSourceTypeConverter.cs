using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

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

			var rootTargetPath = XamlCTask.GetPathForType(module, ((ILRootNode)rootNode).TypeReference);
			var uri = new Uri(value, UriKind.Relative);

			var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

			//fail early
			var resourceId = XamlCTask.GetResourceIdForPath(module, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{value}' not found.", node);


			//abuse the converter, produce some side effect, but leave the stack untouched
			//public void SetAndLoadSource(Uri value, string resourceID, Assembly assembly, System.Xml.IXmlLineInfo lineInfo)
			yield return Create(Ldloc, context.Variables[rdNode]); //the resourcedictionary
			foreach (var instruction in (new UriTypeConverter()).ConvertFromString(value, context, node))
				yield return instruction; //the Uri

			//keep the Uri for later
			yield return Create(Dup);
			var uriVarDef = new VariableDefinition(module.ImportReference(typeof(Uri)));
			body.Variables.Add(uriVarDef);
			yield return Create(Stloc, uriVarDef);

			yield return Create(Ldstr, resourcePath); //resourcePath

			var getTypeFromHandle = module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
			var getTypeInfo = module.ImportReference(typeof(System.Reflection.IntrospectionExtensions).GetMethod("GetTypeInfo", new Type[] { typeof(Type) }));
			var getAssembly = module.ImportReference(typeof(System.Reflection.TypeInfo).GetProperty("Assembly").GetMethod);
			yield return Create(Ldtoken, module.ImportReference(((ILRootNode)rootNode).TypeReference));
			yield return Create(Call, module.ImportReference(getTypeFromHandle));
			yield return Create(Call, module.ImportReference(getTypeInfo));
			yield return Create(Callvirt, module.ImportReference(getAssembly)); //assembly

			foreach (var instruction in node.PushXmlLineInfo(context))
				yield return instruction; //lineinfo

			var setAndLoadSource = module.ImportReference(typeof(ResourceDictionary).GetMethod("SetAndLoadSource"));
			yield return Create(Callvirt, module.ImportReference(setAndLoadSource));

			//ldloc the stored uri as return value
			yield return Create(Ldloc, uriVarDef);
		}

		internal static string GetPathForType(ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes()) {
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(typeof(XamlResourceIdAttribute))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}
	}
}
