using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using static Mono.Cecil.Cil.OpCodes;

using Xamarin.Forms.Xaml;
using Microsoft.Build.Framework;

namespace Xamarin.Forms.Build.Tasks
{
	public class XamlCTask : XamlTask
	{
		bool hasCompiledXamlResources;
		public bool KeepXamlResources { get; set; }
		public bool OptimizeIL { get; set; }

		[Obsolete("OutputGeneratedILAsCode is obsolete as of version 2.3.4. This option is no longer available.")]
		public bool OutputGeneratedILAsCode { get; set; }

		public bool CompileByDefault { get; set; }
		public bool ForceCompile { get; set; }

		public IAssemblyResolver DefaultAssemblyResolver { get; set; }

		internal string Type { get; set; }
		internal MethodDefinition InitCompForType { get; private set; }
		internal bool ReadOnly { get; set; }

		public override bool Execute(out IList<Exception> thrownExceptions)
		{
			thrownExceptions = null;
			Logger = Logger ?? new Logger(null);
			Logger.LogLine(MessageImportance.Normal, "Compiling Xaml, assembly: {0}", Assembly);
			var skipassembly = !CompileByDefault;
			bool success = true;

			if (!File.Exists(Assembly))
			{
				Logger.LogLine(MessageImportance.Normal, "Assembly file not found. Skipping XamlC.");
				return true;
			}

			var resolver = DefaultAssemblyResolver ?? new XamlCAssemblyResolver();
			var xamlCResolver = resolver as XamlCAssemblyResolver;

			if (xamlCResolver != null)
			{
				if (!string.IsNullOrEmpty(DependencyPaths))
				{
					foreach (var dep in DependencyPaths.Split(';'))
					{
						Logger.LogLine(MessageImportance.Low, "Adding searchpath {0}", dep);
						xamlCResolver.AddSearchDirectory(dep);
					}
				}

				if (!string.IsNullOrEmpty(ReferencePath))
				{
					var paths = ReferencePath.Replace("//", "/").Split(';');
					foreach (var p in paths)
					{
						var searchpath = Path.GetDirectoryName(p);
						Logger.LogLine(MessageImportance.Low, "Adding searchpath {0}", searchpath);
						xamlCResolver.AddSearchDirectory(searchpath);
					}
				}
			}
			else {
				Logger.LogLine(MessageImportance.Low, "Ignoring dependency and reference paths due to an unsupported resolver");
			}

			var debug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none");

			var readerParameters = new ReaderParameters {
				AssemblyResolver = resolver,
				ReadWrite = !ReadOnly,
				ReadSymbols = debug,
			};

			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.GetFullPath(Assembly),readerParameters)) {
				CustomAttribute xamlcAttr;
				if (assemblyDefinition.HasCustomAttributes &&
					(xamlcAttr =
						assemblyDefinition.CustomAttributes.FirstOrDefault(
							ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null) {
					var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
					if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
						skipassembly = true;
					if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
						skipassembly = false;
				}

				foreach (var module in assemblyDefinition.Modules) {
					var skipmodule = skipassembly;
					if (module.HasCustomAttributes &&
						(xamlcAttr =
							module.CustomAttributes.FirstOrDefault(
								ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null) {
						var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
						if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
							skipmodule = true;
						if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
							skipmodule = false;
					}

					Logger.LogLine(MessageImportance.Low, " Module: {0}", module.Name);
					var resourcesToPrune = new List<EmbeddedResource>();
					foreach (var resource in module.Resources.OfType<EmbeddedResource>()) {
						Logger.LogString(MessageImportance.Low, "  Resource: {0}... ", resource.Name);
						string classname;
						if (!resource.IsXaml(module, out classname)) {
							Logger.LogLine(MessageImportance.Low, "skipped.");
							continue;
						}
						TypeDefinition typeDef = module.GetType(classname);
						if (typeDef == null) {
							Logger.LogLine(MessageImportance.Low, "no type found... skipped.");
							continue;
						}
						var skiptype = skipmodule;
						if (typeDef.HasCustomAttributes &&
							(xamlcAttr =
								typeDef.CustomAttributes.FirstOrDefault(
									ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null) {
							var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
							if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
								skiptype = true;
							if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
								skiptype = false;
						}

						if (Type != null)
							skiptype = !(Type == classname);

						if (skiptype && !ForceCompile) {
							Logger.LogLine(MessageImportance.Low, "Has XamlCompilationAttribute set to Skip and not Compile... skipped");
							continue;
						}

						var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
						if (initComp == null) {
							Logger.LogLine(MessageImportance.Low, "no InitializeComponent found... skipped.");
							continue;
						}
						Logger.LogLine(MessageImportance.Low, "");

						CustomAttribute xamlFilePathAttr;
						var xamlFilePath = typeDef.HasCustomAttributes && (xamlFilePathAttr = typeDef.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlFilePathAttribute")) != null ?
												  (string)xamlFilePathAttr.ConstructorArguments[0].Value :
												  resource.Name;

						var initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
						if (initCompRuntime != null)
							Logger.LogLine(MessageImportance.Low, "   __InitComponentRuntime already exists... not creating");
						else {
							Logger.LogString(MessageImportance.Low, "   Creating empty {0}.__InitComponentRuntime ...", typeDef.Name);
							initCompRuntime = new MethodDefinition("__InitComponentRuntime", initComp.Attributes, initComp.ReturnType);
							initCompRuntime.Body.InitLocals = true;
							Logger.LogLine(MessageImportance.Low, "done.");
							Logger.LogString(MessageImportance.Low, "   Copying body of InitializeComponent to __InitComponentRuntime ...", typeDef.Name);
							initCompRuntime.Body = new MethodBody(initCompRuntime);
							var iCRIl = initCompRuntime.Body.GetILProcessor();
							foreach (var instr in initComp.Body.Instructions)
								iCRIl.Append(instr);
							initComp.Body.Instructions.Clear();
							initComp.Body.GetILProcessor().Emit(OpCodes.Ret);
							initComp.Body.InitLocals = true;
							typeDef.Methods.Add(initCompRuntime);
							Logger.LogLine(MessageImportance.Low, "done.");
						}

						Logger.LogString(MessageImportance.Low, "   Parsing Xaml... ");
						var rootnode = ParseXaml(resource.GetResourceStream(), typeDef);
						if (rootnode == null) {
							Logger.LogLine(MessageImportance.Low, "failed.");
							continue;
						}
						Logger.LogLine(MessageImportance.Low, "done.");

						hasCompiledXamlResources = true;

						Logger.LogString(MessageImportance.Low, "   Replacing {0}.InitializeComponent ()... ", typeDef.Name);
						Exception e;
						if (!TryCoreCompile(initComp, initCompRuntime, rootnode, out e)) {
							success = false;
							Logger.LogLine(MessageImportance.Low, "failed.");
							(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
							Logger.LogException(null, null, null, xamlFilePath, e);
							Logger.LogLine(MessageImportance.Low, e.StackTrace);
							continue;
						}
						if (Type != null)
						    InitCompForType = initComp;

						Logger.LogLine(MessageImportance.Low, "done.");

						if (OptimizeIL) {
							Logger.LogString(MessageImportance.Low, "   Optimizing IL... ");
							initComp.Body.Optimize();
							Logger.LogLine(MessageImportance.Low, "done");
						}

						Logger.LogLine(MessageImportance.Low, "");

#pragma warning disable 0618
						if (OutputGeneratedILAsCode)
							Logger.LogLine(MessageImportance.Low, "   Decompiling option has been removed. Use a 3rd party decompiler to admire the beauty of the IL generated");
#pragma warning restore 0618
						resourcesToPrune.Add(resource);
					}
					if (hasCompiledXamlResources) {
						Logger.LogString(MessageImportance.Low, "  Changing the module MVID...");
						module.Mvid = Guid.NewGuid();
						Logger.LogLine(MessageImportance.Low, "done.");
					}
					if (!KeepXamlResources) {
						if (resourcesToPrune.Any())
							Logger.LogLine(MessageImportance.Low, "  Removing compiled xaml resources");
						foreach (var resource in resourcesToPrune) {
							Logger.LogString(MessageImportance.Low, "   Removing {0}... ", resource.Name);
							module.Resources.Remove(resource);
							Logger.LogLine(MessageImportance.Low, "done");
						}
					}

					Logger.LogLine(MessageImportance.Low, "");
				}

				if (!hasCompiledXamlResources) {
					Logger.LogLine(MessageImportance.Low, "No compiled resources. Skipping writing assembly.");
					return success;
				}

				if (ReadOnly)
					return success;
				
				Logger.LogString(MessageImportance.Low, "Writing the assembly... ");
				try {
					assemblyDefinition.Write(new WriterParameters {
						WriteSymbols = debug,
					});
					Logger.LogLine(MessageImportance.Low, "done.");
				} catch (Exception e) {
					Logger.LogLine(MessageImportance.Low, "failed.");
					Logger.LogException(null, null, null, null, e);
					(thrownExceptions = thrownExceptions ?? new List<Exception>()).Add(e);
					Logger.LogLine(MessageImportance.Low, e.StackTrace);
					success = false;
				}
			}
			return success;
		}

		bool TryCoreCompile(MethodDefinition initComp, MethodDefinition initCompRuntime, ILRootNode rootnode, out Exception exception)
		{
			try {
				var body = new MethodBody(initComp);
				var module = body.Method.Module;
				body.InitLocals = true;
				var il = body.GetILProcessor();
				var resourcePath = GetPathForType(module, initComp.DeclaringType);

				il.Emit(Nop);

				if (initCompRuntime != null) {
					// Generating branching code for the Previewer

					//First using the ResourceLoader
					var nop = Instruction.Create(Nop);
					var getResourceProvider = module.ImportPropertyGetterReference(("Xamarin.Forms.Core", "Xamarin.Forms.Internals", "ResourceLoader"), "ResourceProvider", isStatic: true);
					il.Emit(Call, getResourceProvider);
					il.Emit(Brfalse, nop);
					il.Emit(Call, getResourceProvider);

					il.Emit(Ldtoken, module.ImportReference(initComp.DeclaringType));
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System", "Type"), methodName: "GetTypeFromHandle", parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") }, isStatic: true));
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System.Reflection", "IntrospectionExtensions"), methodName: "GetTypeInfo", parameterTypes: new[] { ("mscorlib", "System", "Type") }, isStatic: true));
					il.Emit(Callvirt, module.ImportPropertyGetterReference(("mscorlib", "System.Reflection", "TypeInfo"), propertyName: "Assembly", flatten: true));
					il.Emit(Callvirt, module.ImportMethodReference(("mscorlib", "System.Reflection", "Assembly"), methodName: "GetName", parameterTypes: null)); //assemblyName

					il.Emit(Ldstr, resourcePath);   //resourcePath
					il.Emit(Callvirt, module.ImportMethodReference(("mscorlib", "System", "Func`3"),
																   methodName: "Invoke",
																   paramCount: 2,
																   classArguments: new[] { ("mscorlib", "System.Reflection", "AssemblyName"), ("mscorlib", "System", "String"), ("mscorlib", "System", "String") }));
					il.Emit(Brfalse, nop);
					il.Emit(Ldarg_0);
					il.Emit(Call, initCompRuntime);
					il.Emit(Ret);
					il.Append(nop);

					//Or using the deprecated XamlLoader
					nop = Instruction.Create(Nop);

					var getXamlFileProvider = module.ImportPropertyGetterReference(("Xamarin.Forms.Xaml", "Xamarin.Forms.Xaml.Internals", "XamlLoader"), propertyName: "XamlFileProvider", isStatic: true);
					il.Emit(Call, getXamlFileProvider);
					il.Emit(Brfalse, nop);
					il.Emit(Call, getXamlFileProvider);
					il.Emit(Ldarg_0);
					il.Emit(Call, module.ImportMethodReference(("mscorlib", "System", "Object"), methodName: "GetType", parameterTypes: null));
					il.Emit(Callvirt, module.ImportMethodReference(("mscorlib", "System", "Func`2"),
																   methodName: "Invoke",
																   paramCount: 1,
																   classArguments: new[] { ("mscorlib", "System", "Type"), ("mscorlib", "System", "String")}));
					il.Emit(Brfalse, nop);
					il.Emit(Ldarg_0);
					il.Emit(Call, initCompRuntime);
					il.Emit(Ret);
					il.Append(nop);
				}

				var visitorContext = new ILContext(il, body, module);

				rootnode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);
				rootnode.Accept(new ExpandMarkupsVisitor(visitorContext), null);
				rootnode.Accept(new PruneIgnoredNodesVisitor(), null);
				rootnode.Accept(new CreateObjectVisitor(visitorContext), null);
				rootnode.Accept(new SetNamescopesAndRegisterNamesVisitor(visitorContext), null);
				rootnode.Accept(new SetFieldVisitor(visitorContext), null);
				rootnode.Accept(new SetResourcesVisitor(visitorContext), null);
				rootnode.Accept(new SetPropertiesVisitor(visitorContext, true), null);

				il.Emit(Ret);
				initComp.Body = body;
				exception = null;
				return true;
			} catch (Exception e) {
				exception = e;
				return false;
			}
		}

		internal static string GetPathForType(ModuleDefinition module, TypeReference type)
		{
			foreach (var ca in type.Module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (!TypeRefComparer.Default.Equals(ca.ConstructorArguments[2].Value as TypeReference, type))
					continue;
				return ca.ConstructorArguments[1].Value as string;
			}
			return null;
		}

		internal static string GetResourceIdForPath(ModuleDefinition module, string path)
		{
			foreach (var ca in module.GetCustomAttributes())
			{
				if (!TypeRefComparer.Default.Equals(ca.AttributeType, module.ImportReference(("Xamarin.Forms.Core", "Xamarin.Forms.Xaml", "XamlResourceIdAttribute"))))
					continue;
				if (ca.ConstructorArguments[1].Value as string != path)
					continue;
				return ca.ConstructorArguments[0].Value as string;
			}
			return null;
		}
	}
}
