using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
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

			var rootTargetPath = XamlCTask.GetPathForType(context.Cache, currentModule, ((ILRootNode)rootNode).TypeReference);

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
			var resourceTypeRef = GetTypeForPath(context.Cache, module, resourcePath);
			if (resourceTypeRef == null)
				throw new BuildException(BuildExceptionCode.ResourceMissing, node, null, value);

			var resourceType = module.ImportReference(resourceTypeRef).Resolve();

			// validate that the resourceType has a default ctor
			var hasDefaultCtor = resourceType.Methods.Any(md => md.IsConstructor && !md.HasParameters);
			if (!hasDefaultCtor)
				throw new BuildException(BuildExceptionCode.ConstructorDefaultMissing, node, null, resourceType);

			var resourceDictionaryType = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ResourceDictionary");
			var resourceDictionaryTypeDefinition = module.GetTypeDefinition(context.Cache, resourceDictionaryType);

			//abuse the converter, produce some side effect, but leave the stack untouched
			//public void SetAndCreateSource<TResourceType>(Uri value)
			foreach (var instruction in context.Variables[rdNode].LoadAs(context.Cache, resourceDictionaryTypeDefinition, currentModule))
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
			var uriType = currentModule.ImportReference(context.Cache, ("System", "System", "Uri"));
			var uriVarDef = new VariableDefinition(uriType);
			body.Variables.Add(uriVarDef);
			yield return Create(Stloc, uriVarDef);

			var method = module.ImportMethodReference(
							context.Cache,
							resourceDictionaryTypeDefinition,
							methodName: "SetAndCreateSource",
							parameterTypes: new[] { uriType });

			var genericInstanceMethod = new GenericInstanceMethod(method);
			genericInstanceMethod.GenericArguments.Add(resourceType);

			yield return Create(Callvirt, currentModule.ImportReference(genericInstanceMethod));
			//ldloc the stored uri as return value
			yield return Create(Ldloc, uriVarDef);
		}

		internal static string GetPathForType(ILContext context, ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}

		private static TypeReference GetTypeForPath(XamlCache cache, ModuleDefinition module, string path)
		{
			foreach (var ca in module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (ca.ConstructorArguments[1].Value as string != path)
					continue;
				return ca.ConstructorArguments[2].Value as TypeReference;
			}
			return null;
		}
	}
}