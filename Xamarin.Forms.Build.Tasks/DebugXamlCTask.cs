using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;

using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	public class DebugXamlCTask : XamlTask
	{
		public override bool Execute(out IList<Exception> thrownExceptions)
		{
			thrownExceptions = null;
			Logger = Logger ?? new Logger(null);
			Logger.LogLine(MessageImportance.Normal, "Preparing debug code for xamlc, assembly: {0}", Assembly);

			var resolver = new DefaultAssemblyResolver();
			if (!string.IsNullOrEmpty(DependencyPaths))
			{
				foreach (var dep in DependencyPaths.Split(';'))
				{
					Logger.LogLine(MessageImportance.Low, "Adding searchpath {0}", dep);
					resolver.AddSearchDirectory(dep);
				}
			}
			if (!string.IsNullOrEmpty(ReferencePath))
			{
				var paths = ReferencePath.Replace("//", "/").Split(';');
				foreach (var p in paths)
				{
					var searchpath = Path.GetDirectoryName(p);
					Logger.LogLine(MessageImportance.Low, "Adding searchpath {0}", searchpath);
					resolver.AddSearchDirectory(searchpath);
					//					LogLine (3, "Referencing {0}", p);
					//					resolver.AddAssembly (p);
				}
			}

			var debug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none");

			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(Assembly, new ReaderParameters {
				ReadWrite = true,
				ReadSymbols = debug,
				AssemblyResolver = resolver
			})) {
				foreach (var module in assemblyDefinition.Modules) {
					Logger.LogLine(MessageImportance.Low, " Module: {0}", module.Name);
					foreach (var resource in module.Resources.OfType<EmbeddedResource>()) {
						Logger.LogString(MessageImportance.Low, "  Resource: {0}... ", resource.Name);
						string classname;
						if (!resource.IsXaml(module, out classname)) {
							Logger.LogLine(MessageImportance.Low, "skipped.");
							continue;
						} else
							Logger.LogLine(MessageImportance.Low, "");
						TypeDefinition typeDef = module.GetType(classname);
						if (typeDef == null) {
							Logger.LogLine(MessageImportance.Low, "no type found... skipped.");
							continue;
						}

						var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
						if (initComp == null) {
							Logger.LogLine(MessageImportance.Low, "no InitializeComponent found... skipped.");
							continue;
						}
						var initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
						if (initCompRuntime == null) {
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
							typeDef.Methods.Add(initCompRuntime);
							Logger.LogLine(MessageImportance.Low, "done.");
						}

//						IL_0000:  ldarg.0 
//						IL_0001:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.ContentPage::'.ctor'()
//
//						IL_0006:  nop 
//						IL_0007:  ldarg.1 
//						IL_0008:  brfalse IL_0018
//
//						IL_000d:  ldarg.0 
//						IL_000e:  callvirt instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::InitializeComponent()
//						IL_0013:  br IL_001e
//
//						IL_0018:  ldarg.0 
//						IL_0019:  callvirt instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::__InitComponentRuntime()
//						IL_001e:  ret 

						var altCtor =
							typeDef.Methods.Where(
								md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters[0].ParameterType == module.TypeSystem.Boolean)
								.FirstOrDefault();
						if (altCtor != null)
							Logger.LogString(MessageImportance.Low, "   Replacing body of {0}.{0} (bool {1}) ... ", typeDef.Name, altCtor.Parameters[0].Name);
						else {
							Logger.LogString(MessageImportance.Low, "   Adding {0}.{0} (bool useCompiledXaml) ... ", typeDef.Name);
							altCtor = new MethodDefinition(".ctor",
								MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
								MethodAttributes.RTSpecialName, module.TypeSystem.Void);
							altCtor.Parameters.Add(new ParameterDefinition("useCompiledXaml", ParameterAttributes.None,
								module.TypeSystem.Boolean));
						}

						var body = new MethodBody(altCtor);
						body.InitLocals = true;
						var il = body.GetILProcessor();
						var br2 = Instruction.Create(OpCodes.Ldarg_0);
						var ret = Instruction.Create(OpCodes.Ret);
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Callvirt,
							module.ImportReference(typeDef.BaseType.Resolve().GetConstructors().First(c => c.HasParameters == false)));

						il.Emit(OpCodes.Nop);
						il.Emit(OpCodes.Ldarg_1);
						il.Emit(OpCodes.Brfalse, br2);

						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Callvirt, initComp);
						il.Emit(OpCodes.Br, ret);

						il.Append(br2);
						il.Emit(OpCodes.Callvirt, initCompRuntime);
						il.Append(ret);

						altCtor.Body = body;
						if (!typeDef.Methods.Contains(altCtor))
							typeDef.Methods.Add(altCtor);
						Logger.LogLine(MessageImportance.Low, "done.");
					}

					Logger.LogLine(MessageImportance.Low, "");
				}
				Logger.LogString(MessageImportance.Normal, "Writing the assembly... ");
				assemblyDefinition.Write(new WriterParameters {
					WriteSymbols = debug
				});
			}
			Logger.LogLine(MessageImportance.Normal, "done.");

			return true;
		}
	}
}
