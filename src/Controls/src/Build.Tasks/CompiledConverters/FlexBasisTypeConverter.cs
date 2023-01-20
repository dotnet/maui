using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Layouts;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class FlexBasisTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;
			if (!string.IsNullOrEmpty(value))
			{
				value = value.Trim();
				if (value == "Auto")
				{
					yield return Instruction.Create(OpCodes.Ldsfld,
						module.ImportFieldReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui.Layouts", "FlexBasis"),
							"Auto",
							isStatic: true));
					yield break;
				}
				if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase)
					&& float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
				{
					yield return Instruction.Create(OpCodes.Ldc_R4, (float)(relflex / 100));
					yield return Instruction.Create(OpCodes.Ldc_I4_1); //isRelative: true
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui.Layouts", "FlexBasis"), parameterTypes: new[] {
						("mscorlib", "System", "Single"),
						("mscorlib", "System", "Boolean")}));
					yield break;
				}
				if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
				{
					yield return Instruction.Create(OpCodes.Ldc_R4, flex);
					yield return Instruction.Create(OpCodes.Ldc_I4_0); //isRelative: false
					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui.Layouts", "FlexBasis"), parameterTypes: new[] {
						("mscorlib", "System", "Single"),
						("mscorlib", "System", "Boolean")}));
					yield break;
				}
			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(FlexBasis));
		}
	}
}
