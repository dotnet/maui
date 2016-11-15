using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public class XamlCTask : XamlTask
	{
		bool hasCompiledXamlResources;
		public bool KeepXamlResources { get; set; }
		public bool OptimizeIL { get; set; }
		public bool OutputGeneratedILAsCode { get; set; }

		internal string Type { get; set; }

		public override bool Execute(IList<Exception> thrownExceptions)
		{
			Logger = Logger ?? new Logger(null, Verbosity);
			Logger.LogLine(1, "Compiling Xaml");
			Logger.LogLine(1, "\nAssembly: {0}", Assembly);
			if (!string.IsNullOrEmpty(DependencyPaths))
				Logger.LogLine(1, "DependencyPaths: \t{0}", DependencyPaths);
			if (!string.IsNullOrEmpty(ReferencePath))
				Logger.LogLine(1, "ReferencePath: \t{0}", ReferencePath.Replace("//", "/"));
			Logger.LogLine(3, "DebugSymbols:\"{0}\"", DebugSymbols);
			var skipassembly = true; //change this to false to enable XamlC by default
			bool success = true;

			if (!File.Exists(Assembly))
			{
				Logger.LogLine(1, "Assembly file not found. Skipping XamlC.");
				return true;
			}

			var resolver = new XamlCAssemblyResolver();
			if (!string.IsNullOrEmpty(DependencyPaths))
			{
				foreach (var dep in DependencyPaths.Split(';'))
				{
					Logger.LogLine(3, "Adding searchpath {0}", dep);
					resolver.AddSearchDirectory(dep);
				}
			}

			if (!string.IsNullOrEmpty(ReferencePath))
			{
				var paths = ReferencePath.Replace("//", "/").Split(';');
				foreach (var p in paths)
				{
					var searchpath = Path.GetDirectoryName(p);
					Logger.LogLine(3, "Adding searchpath {0}", searchpath);
					resolver.AddSearchDirectory(searchpath);
				}
			}

			var assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.GetFullPath(Assembly), new ReaderParameters
			{
				AssemblyResolver = resolver,
				ReadSymbols = DebugSymbols
			});

			CustomAttribute xamlcAttr;
			if (assemblyDefinition.HasCustomAttributes &&
			    (xamlcAttr =
				    assemblyDefinition.CustomAttributes.FirstOrDefault(
					    ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
			{
				var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
				if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
					skipassembly = true;
				if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
					skipassembly = false;
			}

			foreach (var module in assemblyDefinition.Modules)
			{
				var skipmodule = skipassembly;
				if (module.HasCustomAttributes &&
				    (xamlcAttr =
					    module.CustomAttributes.FirstOrDefault(
						    ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
				{
					var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
					if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
						skipmodule = true;
					if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
						skipmodule = false;
				}

				Logger.LogLine(2, " Module: {0}", module.Name);
				var resourcesToPrune = new List<EmbeddedResource>();
				foreach (var resource in module.Resources.OfType<EmbeddedResource>())
				{
					Logger.LogString(2, "  Resource: {0}... ", resource.Name);
					string classname;
					if (!resource.IsXaml(out classname))
					{
						Logger.LogLine(2, "skipped.");
						continue;
					}
					TypeDefinition typeDef = module.GetType(classname);
					if (typeDef == null)
					{
						Logger.LogLine(2, "no type found... skipped.");
						continue;
					}
					var skiptype = skipmodule;
					if (typeDef.HasCustomAttributes &&
					    (xamlcAttr =
						    typeDef.CustomAttributes.FirstOrDefault(
							    ca => ca.AttributeType.FullName == "Xamarin.Forms.Xaml.XamlCompilationAttribute")) != null)
					{
						var options = (XamlCompilationOptions)xamlcAttr.ConstructorArguments[0].Value;
						if ((options & XamlCompilationOptions.Skip) == XamlCompilationOptions.Skip)
							skiptype = true;
						if ((options & XamlCompilationOptions.Compile) == XamlCompilationOptions.Compile)
							skiptype = false;
					}

					if (Type != null)
						skiptype = !(Type == classname);

					if (skiptype)
					{
						Logger.LogLine(2, "Has XamlCompilationAttribute set to Skip and not Compile... skipped");
						continue;
					}

					var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
					if (initComp == null)
					{
						Logger.LogLine(2, "no InitializeComponent found... skipped.");
						continue;
					}
					Logger.LogLine(2, "");

					var initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
					if (initCompRuntime != null)
						Logger.LogLine(2, "   __InitComponentRuntime already exists... not duplicating");
					else {
						Logger.LogString(2, "   Duplicating {0}.InitializeComponent () into {0}.__InitComponentRuntime ... ", typeDef.Name);
						initCompRuntime = DuplicateMethodDef(typeDef, initComp, "__InitComponentRuntime");
						Logger.LogLine(2, "done.");
					}

					Logger.LogString(2, "   Parsing Xaml... ");
					var rootnode = ParseXaml(resource.GetResourceStream(), typeDef);
					if (rootnode == null)
					{
						Logger.LogLine(2, "failed.");
						continue;
					}
					Logger.LogLine(2, "done.");

					hasCompiledXamlResources = true;

					Logger.LogString(2, "   Replacing {0}.InitializeComponent ()... ", typeDef.Name);
					Exception e;
					if (!TryCoreCompile(initComp, initCompRuntime, rootnode, out e)) {
						success = false;
						Logger.LogLine(2, "failed.");
						thrownExceptions?.Add(e);
						Logger.LogException(null, null, null, resource.Name, e);
						Logger.LogLine(4, e.StackTrace);
						continue;
					}
					Logger.LogLine(2, "done.");

					if (OptimizeIL)
					{
						Logger.LogString(2, "   Optimizing IL... ");
						initComp.Body.OptimizeMacros();
						Logger.LogLine(2, "done");
					}

					if (OutputGeneratedILAsCode)
					{
						var filepath = Path.Combine(Path.GetDirectoryName(Assembly), typeDef.FullName + ".decompiled.cs");
						Logger.LogString(2, "   Decompiling {0} into {1}...", typeDef.FullName, filepath);
						var decompilerContext = new DecompilerContext(module);
						using (var writer = new StreamWriter(filepath))
						{
							var output = new PlainTextOutput(writer);

							var codeDomBuilder = new AstBuilder(decompilerContext);
							codeDomBuilder.AddType(typeDef);
							codeDomBuilder.GenerateCode(output);
						}

						Logger.LogLine(2, "done");
					}
					resourcesToPrune.Add(resource);
				}
				if (!KeepXamlResources)
				{
					if (resourcesToPrune.Any())
						Logger.LogLine(2, "  Removing compiled xaml resources");
					foreach (var resource in resourcesToPrune)
					{
						Logger.LogString(2, "   Removing {0}... ", resource.Name);
						module.Resources.Remove(resource);
						Logger.LogLine(2, "done");
					}
				}

				Logger.LogLine(2, "");
			}

			if (!hasCompiledXamlResources)
			{
				Logger.LogLine(1, "No compiled resources. Skipping writing assembly.");
				return success;
			}

			Logger.LogString(1, "Writing the assembly... ");
			try
			{
				assemblyDefinition.Write(Assembly, new WriterParameters
				{
					WriteSymbols = DebugSymbols
				});
				Logger.LogLine(1, "done.");
			}
			catch (Exception e)
			{
				Logger.LogLine(1, "failed.");
				Logger.LogException(null, null, null, null, e);
				thrownExceptions?.Add(e);
				Logger.LogLine(4, e.StackTrace);
				success = false;
			}

			return success;
		}

		bool TryCoreCompile(MethodDefinition initComp, MethodDefinition initCompRuntime, ILRootNode rootnode, out Exception exception)
		{
			try {
				var body = new MethodBody(initComp);
				var il = body.GetILProcessor();
				il.Emit(OpCodes.Nop);

				if (initCompRuntime != null) {
					// Generating branching code for the Previewer
					//	IL_0007:  call class [mscorlib]System.Func`2<class [mscorlib]System.Type,string> class [Xamarin.Forms.Xaml.Internals]Xamarin.Forms.Xaml.XamlLoader::get_XamlFileProvider()
					//  IL_000c:  brfalse IL_0031
					//  IL_0011:  call class [mscorlib]System.Func`2<class [mscorlib]System.Type,string> class [Xamarin.Forms.Xaml.Internals]Xamarin.Forms.Xaml.XamlLoader::get_XamlFileProvider()
					//  IL_0016:  ldarg.0 
					//  IL_0017:  call instance class [mscorlib]System.Type object::GetType()
					//  IL_001c:  callvirt instance !1 class [mscorlib]System.Func`2<class [mscorlib]System.Type, string>::Invoke(!0)
					//  IL_0021:  brfalse IL_0031
					//  IL_0026:  ldarg.0 
					//  IL_0027:  call instance void class Xamarin.Forms.Xaml.UnitTests.XamlLoaderGetXamlForTypeTests::__InitComponentRuntime()
					//  IL_002c:  ret
					//  IL_0031:  nop

					var nop = Instruction.Create(OpCodes.Nop);
					var getXamlFileProvider = body.Method.Module.Import(body.Method.Module.Import(typeof(Xamarin.Forms.Xaml.Internals.XamlLoader))
							.Resolve()
							.Properties.FirstOrDefault(pd => pd.Name == "XamlFileProvider")
							.GetMethod);
					il.Emit(OpCodes.Call, getXamlFileProvider);
					il.Emit(OpCodes.Brfalse, nop);
					il.Emit(OpCodes.Call, getXamlFileProvider);
					il.Emit(OpCodes.Ldarg_0);
					var getType = body.Method.Module.Import(body.Method.Module.Import(typeof(object))
									  .Resolve()
									  .Methods.FirstOrDefault(md => md.Name == "GetType"));
					il.Emit(OpCodes.Call, getType);
					var func = body.Method.Module.Import(body.Method.Module.Import(typeof(Func<Type, string>))
							 .Resolve()
							 .Methods.FirstOrDefault(md => md.Name == "Invoke"));
					func = func.ResolveGenericParameters(body.Method.Module.Import(typeof(Func<Type, string>)), body.Method.Module);
					il.Emit(OpCodes.Callvirt, func);
					il.Emit(OpCodes.Brfalse, nop);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Call, initCompRuntime);
					il.Emit(OpCodes.Ret);
					il.Append(nop);
				}

				var visitorContext = new ILContext(il, body, body.Method.Module);

				rootnode.Accept(new XamlNodeVisitor((node, parent) => node.Parent = parent), null);
				rootnode.Accept(new ExpandMarkupsVisitor(visitorContext), null);
				rootnode.Accept(new PruneIgnoredNodesVisitor(), null);
				rootnode.Accept(new CreateObjectVisitor(visitorContext), null);
				rootnode.Accept(new SetNamescopesAndRegisterNamesVisitor(visitorContext), null);
				rootnode.Accept(new SetFieldVisitor(visitorContext), null);
				rootnode.Accept(new SetResourcesVisitor(visitorContext), null);
				rootnode.Accept(new SetPropertiesVisitor(visitorContext, true), null);

				il.Emit(OpCodes.Ret);
				initComp.Body = body;
				exception = null;
				return true;
			} catch (Exception e) {
				exception = e;
				return false;
			}
		}
	}
}