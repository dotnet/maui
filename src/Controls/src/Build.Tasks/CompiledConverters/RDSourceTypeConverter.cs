using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
{
	class RDSourceTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var currentModule = context.Body.Method.Module;
			var body = context.Body;

			INode rootNode = node;
			while (!(rootNode is ILRootNode))
				rootNode = rootNode.Parent;

			var rdNode = node.Parent as IElementNode;

			var rootTargetPath = XamlCTask.GetPathForType(currentModule, ((ILRootNode)rootNode).TypeReference);

			var module = currentModule;
			string asmName = null;
			if (value.Contains(";assembly="))
			{
				var parts = value.Split(new[] { ";assembly=" }, StringSplitOptions.RemoveEmptyEntries);
				value = parts[0];
				asmName = parts[1];
				if (currentModule.Assembly.Name.Name != asmName)
				{
					var ar = currentModule.AssemblyReferences.FirstOrDefault(ar => ar.Name == asmName);
					if (ar == null)
						throw new BuildException(BuildExceptionCode.ResourceMissing, node, null, value);
					module = currentModule.AssemblyResolver.Resolve(ar).MainModule;
				}
			}
			var uri = new Uri(value, UriKind.Relative);

			var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

			//fail early
			var resourceId = XamlCTask.GetResourceIdForPath(module, resourcePath);
			if (resourceId == null)
				throw new BuildException(BuildExceptionCode.ResourceMissing, node, null, value);

			var resourceDictionaryType = ("Microsoft.Maui.Controls.Core", "Microsoft.Maui.Controls", "ResourceDictionary");

			//abuse the converter, produce some side effect, but leave the stack untouched
			//public void SetAndLoadSource(Uri value, string resourceID, Assembly assembly, System.Xml.IXmlLineInfo lineInfo)
			foreach (var instruction in context.Variables[rdNode].LoadAs(currentModule.GetTypeDefinition(resourceDictionaryType), currentModule))
				yield return instruction;
			//reappend assembly= in all cases, see other RD converter
			if (!string.IsNullOrEmpty(asmName))
				value = $"{value};assembly={asmName}";
			else
				value = $"{value};assembly={((ILRootNode)rootNode).TypeReference.Module.Assembly.Name.Name}";
			foreach (var instruction in (new UriTypeConverter()).ConvertFromString(value, context, node))
				yield return instruction; //the Uri

			//keep the Uri for later
			yield return Create(Dup);
			var uriVarDef = new VariableDefinition(currentModule.ImportReference(("System", "System", "Uri")));
			body.Variables.Add(uriVarDef);
			yield return Create(Stloc, uriVarDef);
			yield return Create(Ldstr, resourcePath); //resourcePath

			if (!string.IsNullOrEmpty(asmName))
			{
				yield return Create(Ldstr, asmName);
				yield return Create(Call, currentModule.ImportMethodReference(("mscorlib", "System.Reflection", "Assembly"), methodName: "Load", parameterTypes: new[] { ("mscorlib", "System", "String") }, isStatic: true));
			}
			else //we could use assembly.Load in the 'else' part too, but I don't want to change working code right now
			{
				yield return Create(Ldtoken, currentModule.ImportReference(((ILRootNode)rootNode).TypeReference));
				yield return Create(Call, currentModule.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Call, currentModule.ImportMethodReference(("mscorlib", "System.Reflection", "IntrospectionExtensions"), methodName: "GetTypeInfo", parameterTypes: new[] { ("mscorlib", "System", "Type") }, isStatic: true));
				yield return Create(Callvirt, currentModule.ImportPropertyGetterReference(("mscorlib", "System.Reflection", "TypeInfo"), propertyName: "Assembly", flatten: true));
			}
			foreach (var instruction in node.PushXmlLineInfo(context))
				yield return instruction; //lineinfo
			yield return Create(Callvirt, currentModule.ImportMethodReference(resourceDictionaryType,
																	   methodName: "SetAndLoadSource",
																	   parameterTypes: new[] { ("System", "System", "Uri"), ("mscorlib", "System", "String"), ("mscorlib", "System.Reflection", "Assembly"), ("System.Xml.ReaderWriter", "System.Xml", "IXmlLineInfo") }));
			//ldloc the stored uri as return value
			yield return Create(Ldloc, uriVarDef);
		}

		internal static string GetPathForType(ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(("Microsoft.Maui.Controls.Core", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}
	}
}