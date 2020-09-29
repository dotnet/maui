using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Xamarin.Forms.Core.XamlC
{
	class UriTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (string.IsNullOrWhiteSpace(value))
			{
				yield return Create(Ldnull);
				yield break;
			}

			yield return Create(Ldstr, value);
			yield return Create(Ldc_I4_0); //UriKind.RelativeOrAbsolute
			yield return Create(Newobj, module.ImportCtorReference(("System", "System", "Uri"), parameterTypes: new[] {
				("mscorlib", "System", "String"),
				("System", "System", "UriKind")}));
		}
	}
}