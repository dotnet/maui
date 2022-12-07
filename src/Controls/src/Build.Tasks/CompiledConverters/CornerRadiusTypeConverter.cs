using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class CornerRadiusTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (!string.IsNullOrEmpty(value))
			{
				double l, tl, tr, bl, br;
				var cornerradius = value.Split(',');
				switch (cornerradius.Length)
				{
					case 1:
						if (double.TryParse(cornerradius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out l))
							return GenerateIL(context, module, l);
						break;
					case 4:
						if (double.TryParse(cornerradius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tl)
							&& double.TryParse(cornerradius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out tr)
							&& double.TryParse(cornerradius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out bl)
							&& double.TryParse(cornerradius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out br))
							return GenerateIL(context, module, tl, tr, bl, br);
						break;
				}
			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(CornerRadius));
		}

		IEnumerable<Instruction> GenerateIL(ILContext context, ModuleDefinition module, params double[] args)
		{
			foreach (var d in args)
				yield return Instruction.Create(OpCodes.Ldc_R8, d);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "CornerRadius"), parameterTypes: args.Select(a => ("mscorlib", "System", "Double")).ToArray()));
		}
	}

}
