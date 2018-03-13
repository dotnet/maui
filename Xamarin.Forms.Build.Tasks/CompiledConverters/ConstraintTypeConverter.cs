using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class ConstraintTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			double size;

			if (string.IsNullOrEmpty(value) || !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Constraint)}", node);

			yield return Instruction.Create(OpCodes.Ldc_R8, size);
			var constantReference = module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Constraint"),
																 methodName: "Constant",
																 paramCount: 1,
																 predicate: md => md.IsStatic);
			yield return Instruction.Create(OpCodes.Call, constantReference);
		}
	}
}
