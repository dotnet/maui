using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class UriTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (string.IsNullOrWhiteSpace(value)) {
				yield return Create(Ldnull);
				yield break;
			}

			var uriCtorRef = module.ImportCtorReference(("System", "System", "Uri"),
														paramCount: 2,
														predicate: md => md.Parameters[1].ParameterType.FullName == "System.UriKind");
			yield return Create(Ldstr, value);
			yield return Create(Ldc_I4_0); //UriKind.RelativeOrAbsolute
			yield return Create(Newobj, uriCtorRef);
		}
	}
}