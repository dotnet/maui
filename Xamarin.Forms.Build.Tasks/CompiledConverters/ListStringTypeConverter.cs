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
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (value == null) {
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}
			var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

			var listCtor = module.ImportReferenceCached(typeof(List<>)).ResolveCached().Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 1 && md.Parameters[0].ParameterType.FullName == "System.Int32");
			var listCtorRef = module.ImportReference(listCtor);
			listCtorRef = module.ImportReference(listCtorRef.ResolveGenericParameters(module.ImportReferenceCached(typeof(List<string>)), module));

			var adder = module.ImportReferenceCached(typeof(ICollection<>)).ResolveCached().Methods.FirstOrDefault(md => md.Name == "Add" && md.Parameters.Count == 1);
			var adderRef = module.ImportReference(adder);
			adderRef = module.ImportReference(adderRef.ResolveGenericParameters(module.ImportReferenceCached(typeof(ICollection<string>)), module));

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