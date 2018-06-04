using System;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Rocks;

using Mono.Cecil.Cil;
using Microsoft.Build.Framework;
using System.IO;

namespace XFCorePostProcessor.Tasks
{
	public class FixXFCoreAssembly : Task
	{
		[Required]
		public string Assembly { get; set; }
		public string ReferencePath { get; set; }

		public override bool Execute()
		{
			Log.LogMessage("Generating backcompat code for #2835");

			var resolver =  new AssemblyResolver();

			if (!string.IsNullOrEmpty(ReferencePath)) {
				var paths = ReferencePath.Replace("//", "/").Split(';');
				foreach (var p in paths) {
					var searchpath = Path.GetDirectoryName(p);
					resolver.AddSearchDirectory(searchpath);
				}
			}

			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(Assembly, new ReaderParameters { AssemblyResolver = resolver, ReadWrite = true, ReadSymbols = true })) {
				var resourceLoader = assemblyDefinition.MainModule.GetType("Xamarin.Forms.Internals.ResourceLoader");
				var module = assemblyDefinition.MainModule;
				var methodDef = new MethodDefinition("get_ResourceProvider",
													 MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
													 module.ImportReference(module.ImportReference(typeof(Func<,>)).MakeGenericInstanceType(module.ImportReference(typeof(string)),
														module.ImportReference(typeof(string)))));

				var body = new MethodBody(methodDef);
				var il = body.GetILProcessor();
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
				methodDef.Body = body;
				resourceLoader.Methods.Add(methodDef);

				assemblyDefinition.Write(new WriterParameters {
					WriteSymbols = true,
				});
			}
			return true;
		}
	}

	class AssemblyResolver : DefaultAssemblyResolver
	{
		public void AddAssembly(string p)
		{
			RegisterAssembly(AssemblyDefinition.ReadAssembly(p, new ReaderParameters {
				AssemblyResolver = this
			}));
		}
	}
}
