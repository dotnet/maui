using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Xamarin.Forms.Build.Tasks
{
	public class DebugXamlCTask : XamlTask
	{
		public override bool Execute(IList<Exception> thrownExceptions)
		{
			Logger = Logger ?? new Logger(null, Verbosity);
			Logger.LogLine(1, "Preparing debug code for xamlc");
			Logger.LogLine(1, "\nAssembly: {0}", Assembly);

			var resolver = new DefaultAssemblyResolver();
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
					//					LogLine (3, "Referencing {0}", p);
					//					resolver.AddAssembly (p);
				}
			}
			var assemblyDefinition = AssemblyDefinition.ReadAssembly(Assembly, new ReaderParameters
			{
				//ReadSymbols = DebugSymbols,
				AssemblyResolver = resolver
			});

			foreach (var module in assemblyDefinition.Modules)
			{
				Logger.LogLine(2, " Module: {0}", module.Name);
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
					var initComp = typeDef.Methods.FirstOrDefault(md => md.Name == "InitializeComponent");
					if (initComp == null)
					{
						Logger.LogLine(2, "no InitializeComponent found... skipped.");
						continue;
					}
					var initCompRuntime = typeDef.Methods.FirstOrDefault(md => md.Name == "__InitComponentRuntime");
					if (initCompRuntime == null) {
						Logger.LogLine(2, "no __InitComponentRuntime found... duplicating.");
						initCompRuntime = DuplicateMethodDef(typeDef, initComp, "__InitComponentRuntime");
					}

//					IL_0000:  ldarg.0 
//					IL_0001:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.ContentPage::'.ctor'()
//
//					IL_0006:  nop 
//					IL_0007:  ldarg.1 
//					IL_0008:  brfalse IL_0018
//
//					IL_000d:  ldarg.0 
//					IL_000e:  callvirt instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::InitializeComponent()
//					IL_0013:  br IL_001e
//
//					IL_0018:  ldarg.0 
//					IL_0019:  callvirt instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::__InitComponentRuntime()
//					IL_001e:  ret 

					var altCtor =
						typeDef.Methods.Where(
							md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters[0].ParameterType == module.TypeSystem.Boolean)
							.FirstOrDefault();
					if (altCtor != null)
						Logger.LogString(2, "   Replacing body of {0}.{0} (bool {1}) ... ", typeDef.Name, altCtor.Parameters[0].Name);
					else
					{
						Logger.LogString(2, "   Adding {0}.{0} (bool useCompiledXaml) ... ", typeDef.Name);
						altCtor = new MethodDefinition(".ctor",
							MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
							MethodAttributes.RTSpecialName, module.TypeSystem.Void);
						altCtor.Parameters.Add(new ParameterDefinition("useCompiledXaml", ParameterAttributes.None,
							module.TypeSystem.Boolean));
					}

					var body = new MethodBody(altCtor);
					var il = body.GetILProcessor();
					var br2 = Instruction.Create(OpCodes.Ldarg_0);
					var ret = Instruction.Create(OpCodes.Ret);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Callvirt,
						module.Import(typeDef.BaseType.Resolve().GetConstructors().First(c => c.HasParameters == false)));

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
					Logger.LogLine(2, "done.");
				}

				Logger.LogLine(2, "");
			}
			Logger.LogString(1, "Writing the assembly... ");
			assemblyDefinition.Write(Assembly, new WriterParameters
			{
				WriteSymbols = DebugSymbols
			});
			Logger.LogLine(1, "done.");

			return true;
		}
	}
}