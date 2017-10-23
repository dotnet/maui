using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;

using static System.String;

namespace Xamarin.Forms.Core.XamlC
{
	class BindingTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ModuleDefinition module, BaseNode node)
		{
			if (IsNullOrEmpty(value))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Binding)}", node);

			var bindingCtor = module.ImportReference(typeof(Binding)).Resolve().Methods.FirstOrDefault(md => md.IsConstructor && md.Parameters.Count == 6);
			var bindingCtorRef = module.ImportReference(bindingCtor);

			yield return Instruction.Create(OpCodes.Ldstr, value);
			yield return Instruction.Create(OpCodes.Ldc_I4, (int)BindingMode.Default);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Newobj, bindingCtorRef);
		}
	}
}