using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using static Microsoft.Build.Framework.MessageImportance;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.Build.Tasks
{
	public class DebugXamlCTask : XamlTask
	{
		public override bool Execute(out IList<Exception> thrownExceptions)
		{
			thrownExceptions = null;
			LoggingHelper.LogMessage(Normal, $"{new string(' ', 0)}Preparing debug code for xamlc, assembly: {Assembly}");

			var resolver = new DefaultAssemblyResolver();
			if (ReferencePath != null)
			{
				var paths = ReferencePath.Select(p => IOPath.GetDirectoryName(p.Replace("//", "/"))).Distinct();
				foreach (var searchpath in paths)
				{
					LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Adding searchpath {searchpath}");
					resolver.AddSearchDirectory(searchpath);
				}
			}

			var debug = DebugSymbols || (!string.IsNullOrEmpty(DebugType) && DebugType.ToLowerInvariant() != "none");

			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(Assembly, new ReaderParameters
			{
				ReadWrite = true,
				ReadSymbols = debug,
				AssemblyResolver = resolver
			}))
			{
				foreach (var module in assemblyDefinition.Modules)
				{
					LoggingHelper.LogMessage(Low, $"{new string(' ', 2)}Module: {module.Name}");
					foreach (var resource in module.Resources.OfType<EmbeddedResource>())
					{
						LoggingHelper.LogMessage(Low, $"{new string(' ', 4)}Resource: {resource.Name}");
						if (!resource.IsXaml(module, out var classname))
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}skipped.");
							continue;
						}
						TypeDefinition typeDef = module.GetType(classname);
						if (typeDef == null)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no type found... skipped.");
							continue;
						}

						var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
						if (initComp == null)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}no InitializeComponent found... skipped.");
							continue;
						}
						var initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
						if (initCompRuntime == null)
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Creating empty {typeDef.Name}.__InitComponentRuntime");
							initCompRuntime = new MethodDefinition("__InitComponentRuntime", initComp.Attributes, initComp.ReturnType);
							initCompRuntime.Body.InitLocals = true;
							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Copying body of InitializeComponent to __InitComponentRuntime");
							initCompRuntime.Body = new MethodBody(initCompRuntime);
							var iCRIl = initCompRuntime.Body.GetILProcessor();
							foreach (var instr in initComp.Body.Instructions)
								iCRIl.Append(instr);
							initComp.Body.Instructions.Clear();
							initComp.Body.GetILProcessor().Emit(OpCodes.Ret);
							typeDef.Methods.Add(initCompRuntime);
							LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
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

						var altCtor = typeDef.Methods.FirstOrDefault(md => md.IsConstructor
																		&& md.Parameters.Count == 1
																		&& md.Parameters[0].ParameterType == module.TypeSystem.Boolean);
						if (altCtor != null)
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Replacing body of {typeDef.Name}.{typeDef.Name} (bool {altCtor.Parameters[0].Name})");
						else
						{
							LoggingHelper.LogMessage(Low, $"{new string(' ', 6)}Adding {typeDef.Name}.{typeDef.Name} (bool useCompiledXaml)");
							altCtor = new MethodDefinition(".ctor",
								MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
								MethodAttributes.RTSpecialName, module.TypeSystem.Void);
							altCtor.Parameters.Add(new ParameterDefinition("useCompiledXaml", ParameterAttributes.None,
								module.TypeSystem.Boolean));
						}

						var body = new MethodBody(altCtor)
						{
							InitLocals = true
						};
						var il = body.GetILProcessor();
						var br2 = Instruction.Create(OpCodes.Ldarg_0);
						var ret = Instruction.Create(OpCodes.Ret);
						il.Emit(OpCodes.Ldarg_0);
						MethodReference baseCtor;
						if (typeDef.BaseType.Resolve().GetConstructors().FirstOrDefault(c => c.HasParameters && c.Parameters.Count == 1 && c.Parameters[0].Name == "useCompiledXaml") is MethodDefinition baseCtorDef)
						{
							baseCtor = module.ImportReference(baseCtorDef);
							baseCtor = module.ImportReference(baseCtor.ResolveGenericParameters(typeDef.BaseType, module));
							il.Emit(OpCodes.Ldarg_1);
						}
						else
						{
							baseCtor = module.ImportReference(typeDef.BaseType.Resolve().GetConstructors().First(c => c.HasParameters == false));
							baseCtor = module.ImportReference(baseCtor.ResolveGenericParameters(typeDef.BaseType, module));
						}
						il.Emit(OpCodes.Callvirt, baseCtor);

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
						LoggingHelper.LogMessage(Low, $"{new string(' ', 8)}done.");
					}

				}
				LoggingHelper.LogMessage(Normal, $"{new string(' ', 0)}Writing the assembly.");
				assemblyDefinition.Write(new WriterParameters
				{
					WriteSymbols = debug
				});
			}
			LoggingHelper.LogMessage(Normal, $"{new string(' ', 2)}done.");

			return true;
		}
	}
}