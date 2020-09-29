using System.Collections.Generic;
using System.Globalization;
using Mono.Cecil.Cil;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Xamarin.Forms.Core.XamlC
{
	class ConstraintTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			double size;

			if (string.IsNullOrEmpty(value) || !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Constraint));

			yield return Create(Ldc_R8, size);
			yield return Create(Call, module.ImportMethodReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Constraint"),
																   methodName: "Constant",
																   parameterTypes: new[] { ("mscorlib", "System", "Double") },
																   isStatic: true));
		}
	}
}