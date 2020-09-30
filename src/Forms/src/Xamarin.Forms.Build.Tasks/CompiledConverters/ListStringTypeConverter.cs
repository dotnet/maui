using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Xamarin.Forms.Core.XamlC
{
	class ListStringTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (value == null)
			{
				yield return Create(Ldnull);
				yield break;
			}
			var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

			yield return Create(Ldc_I4, parts.Count);
			yield return Create(Newobj, module.ImportCtorReference(("System.Collections", "System.Collections.Generic", "List`1"),
																   parameterTypes: new[] { ("mscorlib", "System", "Int32") },
																   classArguments: new[] { ("mscorlib", "System", "String") }));
			foreach (var part in parts)
			{
				yield return Create(Dup);
				yield return Create(Ldstr, part);
				yield return Create(Callvirt, module.ImportMethodReference(("mscorlib", "System.Collections.Generic", "ICollection`1"),
																		   methodName: "Add",
																		   paramCount: 1,
																		   classArguments: new[] { ("mscorlib", "System", "String") }));
			}
		}
	}
}