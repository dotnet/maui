using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Core.XamlC
{
	class ConstraintTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ModuleDefinition module, BaseNode node)
		{
			double size;

			if (string.IsNullOrEmpty(value) || !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Constraint)}", node);

			yield return Instruction.Create(OpCodes.Ldc_R8, size);

			var constantDef = module.ImportReference(typeof(Constraint)).Resolve().Methods.FirstOrDefault(md => md.IsStatic && md.Name == "Constant");
			var constantRef = module.ImportReference(constantDef);
			yield return Instruction.Create(OpCodes.Call, constantRef);
		}
	}
}
