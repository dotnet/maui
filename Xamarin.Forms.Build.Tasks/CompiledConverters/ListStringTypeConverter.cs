using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class ListStringTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ModuleDefinition module, BaseNode node)
		{
			if (value == null) {
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}
			var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

			var listCtor = module.ImportReference(typeof(List<>)).Resolve().Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters[0].ParameterType.FullName == "System.Int32");
			var listCtorRef = module.ImportReference(listCtor);
			listCtorRef = module.ImportReference(listCtorRef.ResolveGenericParameters(module.ImportReference(typeof(List<string>)), module));

			var adder = module.ImportReference(typeof(ICollection<>)).Resolve().Methods.FirstOrDefault(md => md.Name == "Add" && md.Parameters.Count == 1);
			var adderRef = module.ImportReference(adder);
			adderRef = module.ImportReference(adderRef.ResolveGenericParameters(module.ImportReference(typeof(ICollection<string>)), module));

			yield return Instruction.Create(OpCodes.Ldc_I4, parts.Count);
			yield return Instruction.Create(OpCodes.Newobj, listCtorRef);

			foreach (var part in parts) {
				yield return Instruction.Create(OpCodes.Dup);
				yield return Instruction.Create(OpCodes.Ldstr, part);
				yield return Instruction.Create(OpCodes.Callvirt, adderRef);
			}
		}
	}
}