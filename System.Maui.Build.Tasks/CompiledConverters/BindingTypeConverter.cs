using System.Collections.Generic;
using Mono.Cecil.Cil;

using System.Maui.Xaml;

using static System.String;
using System.Maui.Build.Tasks;

namespace System.Maui.Core.XamlC
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
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("System.Maui.Core", "System.Maui", "Binding"), parameterTypes: new[] {
				("mscorlib", "System", "String"),
				("System.Maui.Core", "System.Maui", "BindingMode"),
				("System.Maui.Core", "System.Maui", "IValueConverter"),
				("mscorlib", "System", "Object"),
				("mscorlib", "System", "String"),
				("mscorlib", "System", "Object")}));
		}
	}
}