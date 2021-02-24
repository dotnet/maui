using System.Collections.Generic;
using System.Globalization;
using Mono.Cecil.Cil;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
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
			yield return Create(Call, module.ImportMethodReference(("Microsoft.Maui.Controls.Core", "Microsoft.Maui.Controls", "Constraint"),
																   methodName: "Constant",
																   parameterTypes: new[] { ("mscorlib", "System", "Double") },
																   isStatic: true));
		}
	}
}
