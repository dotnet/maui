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

			var rdNode = node.Parent as ElementNode;

			var rootTargetPath = XamlCTask.GetPathForType(context.Cache, currentModule, ((ILRootNode)rootNode).TypeReference);

			var module = currentModule;
			string asmName = null;
			if (value.Contains(";assembly="))
			{
				var parts = value.Split([";assembly="], StringSplitOptions.RemoveEmptyEntries);
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

			foreach (var instruction in CreateUri(context, (ILRootNode)rootNode, value, node, asmName))
				yield return instruction;

			var uriVarDef = new VariableDefinition(currentModule.ImportReference(context.Cache, ("System", "System", "Uri")));
			body.Variables.Add(uriVarDef);
			yield return Create(Stloc, uriVarDef);

			var resourceTypeRef = GetTypeForPath(context.Cache, module, resourcePath);
			if (resourceTypeRef is not null)
			{
				foreach (var instruction in CreateResourceDictionaryType(context, currentModule, module, node, rdNode, resourceTypeRef, uriVarDef))
					yield return instruction;
			}
			else
			{
				// It is possible that this resource exists but it is not compiled and it doesn't have a resource type associated with it (e.g., using <?xaml-comp compile="false"?>)
				// we can still generate code that will load the XAML from the resource file at runtime. This code won't be trimming safe and the generated code
				// will produce trimming warnings when compiled.
				var resourceId = XamlCTask.GetResourceIdForPath(context.Cache, module, resourcePath);
				if (resourceId is null)
				{
					throw new BuildException(BuildExceptionCode.ResourceMissing, node, null, value);
				}

				foreach (var instruction in LoadResourceDictionaryFromSource(context, currentModule, (ILRootNode)rootNode, node, rdNode, uriVarDef, resourcePath, asmName))
					yield return instruction;
			}

			yield return Create(Ldloc, uriVarDef);
		}

		private static IEnumerable<Instruction> CreateUri(ILContext context, ILRootNode rootNode, string value, BaseNode node, string asmName)
		{
			//reappend assembly= in all cases, see other RD converter
			if (!string.IsNullOrEmpty(asmName))
				value = $"{value};assembly={asmName}";
			else
				value = $"{value};assembly={rootNode.TypeReference.Module.Assembly.Name.Name}";
			foreach (var instruction in (new UriTypeConverter()).ConvertFromString(value, context, node))
				yield return instruction; //the Uri
		}

		private static IEnumerable<Instruction> CreateResourceDictionaryType(
			ILContext context,
			ModuleDefinition currentModule,
			ModuleDefinition module,
			BaseNode node,
			ElementNode rdNode,
			TypeReference resourceTypeRef,
			VariableDefinition uriVarDef)
		{
			var resourceType = module.ImportReference(resourceTypeRef).Resolve();

			// validate that the resourceType has a default ctor
			var hasDefaultCtor = resourceType.Methods.Any(md => md.IsConstructor && !md.HasParameters);
			if (!hasDefaultCtor)
				throw new BuildException(BuildExceptionCode.ConstructorDefaultMissing, node, null, resourceType);

			var method = module.ImportMethodReference(
				context.Cache,
				("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ResourceDictionary"),
				methodName: "SetAndCreateSource",
				parameterTypes: new[] { ("System", "System", "Uri") });

			var genericInstanceMethod = new GenericInstanceMethod(method);
			genericInstanceMethod.GenericArguments.Add(resourceType);

			// public void rd.SetAndCreateSource<TResourceType>(Uri value)
			foreach (var instruction in context.Variables[rdNode].LoadAs(context.Cache, currentModule.GetTypeDefinition(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ResourceDictionary")), currentModule))
				yield return instruction;
			yield return Create(Ldloc, uriVarDef);
			yield return Create(Callvirt, currentModule.ImportReference(genericInstanceMethod));
		}

		private static IEnumerable<Instruction> LoadResourceDictionaryFromSource(
			ILContext context,
			ModuleDefinition currentModule,
			ILRootNode rootNode,
			BaseNode node,
			ElementNode rdNode,
			VariableDefinition uriVarDef,
			string resourcePath,
			string asmName)
		{
			// public void static ResourceDictionaryHelpers.LoadFromSource(ResourceDictionary rd, Uri source, string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
			foreach (var instruction in context.Variables[rdNode].LoadAs(context.Cache, currentModule.GetTypeDefinition(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ResourceDictionary")), currentModule))
				yield return instruction;
			yield return Create(Ldloc, uriVarDef);
			yield return Create(Ldstr, resourcePath);

			if (!string.IsNullOrEmpty(asmName))
			{
				yield return Create(Ldstr, asmName);
				yield return Create(Call, currentModule.ImportMethodReference(context.Cache, ("mscorlib", "System.Reflection", "Assembly"), methodName: "Load", parameterTypes: new[] { ("mscorlib", "System", "String") }, isStatic: true));
			}
			else //we could use assembly.Load in the 'else' part too, but I don't want to change working code right now
			{
				yield return Create(Ldtoken, currentModule.ImportReference(rootNode.TypeReference));
				yield return Create(Call, currentModule.ImportMethodReference(context.Cache, ("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
				yield return Create(Callvirt, currentModule.ImportPropertyGetterReference(context.Cache, ("mscorlib", "System", "Type"), propertyName: "Assembly", flatten: true));
			}

			foreach (var instruction in node.PushXmlLineInfo(context))
				yield return instruction;

			yield return Create(Call, currentModule.ImportMethodReference(
				context.Cache,
				("Microsoft.Maui.Controls.Xaml", "Microsoft.Maui.Controls.Xaml", "ResourceDictionaryHelpers"),
				methodName: "LoadFromSource",
				parameterTypes: new[] {
					("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "ResourceDictionary"),
					("System", "System", "Uri"),
					("mscorlib", "System", "String"),
					("mscorlib", "System.Reflection", "Assembly"),
					("System.Xml.ReaderWriter", "System.Xml", "IXmlLineInfo") },
				isStatic: true));
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