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

			var add = module.ImportMethodReference(("mscorlib", "System.Collections.Generic", "ICollection`1"),
												   methodName: "Add",
												   paramCount: 1,
												   classArguments: new[] { ("mscorlib", "System", "String") });

			yield return Instruction.Create(OpCodes.Ldc_I4, parts.Count);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("System.Collections", "System.Collections.Generic", "List`1"),
																					   paramCount: 1,
																					   predicate: md => md.Parameters[0].ParameterType.FullName == "System.Int32",
																					   classArguments: new[] { ("mscorlib", "System", "String") }));
			foreach (var part in parts) {
				yield return Instruction.Create(OpCodes.Dup);
				yield return Instruction.Create(OpCodes.Ldstr, part);
				yield return Instruction.Create(OpCodes.Callvirt, add);
			}
		}
	}
}