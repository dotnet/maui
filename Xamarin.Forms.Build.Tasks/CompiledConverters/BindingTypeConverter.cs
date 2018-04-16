using System.Collections.Generic;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;

using static System.String;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class BindingTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (IsNullOrEmpty(value))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Binding)}", node);

			yield return Instruction.Create(OpCodes.Ldstr, value);
			yield return Instruction.Create(OpCodes.Ldc_I4, (int)BindingMode.Default);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Binding"), parameterTypes: new[] {
				("mscorlib", "System", "String"),
				("Xamarin.Forms.Core", "Xamarin.Forms", "BindingMode"),
				("Xamarin.Forms.Core", "Xamarin.Forms", "IValueConverter"),
				("mscorlib", "System", "Object"),
				("mscorlib", "System", "String"),
				("mscorlib", "System", "Object")}));
		}
	}
}