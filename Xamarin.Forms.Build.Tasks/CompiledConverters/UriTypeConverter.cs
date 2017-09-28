using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
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
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}

			var uriCtor = module.ImportReference(typeof(Uri)).Resolve().Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 2 && md.Parameters[1].ParameterType.FullName == "System.UriKind");
			var uriCtorRef = module.ImportReference(uriCtor);

			yield return Instruction.Create(OpCodes.Ldstr, value);
			yield return Instruction.Create(OpCodes.Ldc_I4_0); //UriKind.RelativeOrAbsolute
			yield return Instruction.Create(OpCodes.Newobj, uriCtorRef);
		}
	}
}