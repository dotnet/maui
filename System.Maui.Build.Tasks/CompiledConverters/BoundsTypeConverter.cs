using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class BoundsTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (string.IsNullOrEmpty(value))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Rectangle)}", node);

			double x = -1, y = -1, w = -1, h = -1;
			bool hasX, hasY, hasW, hasH;
			var xywh = value.Split(',');

			if (xywh.Length != 2 && xywh.Length != 4)
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Rectangle)}", node);

			hasX = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh [0], NumberStyles.Number, CultureInfo.InvariantCulture, out x);
			hasY = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh [1], NumberStyles.Number, CultureInfo.InvariantCulture, out y);
			hasW = xywh.Length == 4 && double.TryParse(xywh [2], NumberStyles.Number, CultureInfo.InvariantCulture, out w);
			hasH = xywh.Length == 4 && double.TryParse(xywh [3], NumberStyles.Number, CultureInfo.InvariantCulture, out h);

			if (!hasW && xywh.Length == 4 && string.Compare("AutoSize", xywh [2].Trim(), StringComparison.OrdinalIgnoreCase) == 0) {
				hasW = true;
				w = AbsoluteLayout.AutoSize;
			}

			if (!hasH && xywh.Length == 4 && string.Compare("AutoSize", xywh [3].Trim(), StringComparison.OrdinalIgnoreCase) == 0) {
				hasH = true;
				h = AbsoluteLayout.AutoSize;
			}

			if (hasX && hasY && xywh.Length == 2) {
				hasW = true;
				w = AbsoluteLayout.AutoSize;
				hasH = true;
				h = AbsoluteLayout.AutoSize;
			}

			if (!hasX || !hasY || !hasW || !hasH)
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Rectangle)}", node);

			return GenerateIL(x, y, w, h, module);
		}

		IEnumerable<Instruction> GenerateIL(double x, double y, double w, double h, ModuleDefinition module)
		{
//			IL_0000:  ldc.r8 3.1000000000000001
//			IL_0009:  ldc.r8 4.2000000000000002
//			IL_0012:  ldc.r8 5.2999999999999998
//			IL_001b:  ldc.r8 6.4000000000000004
//			IL_0024:  newobj instance void valuetype Test.Rectangle::'.ctor'(float64, float64, float64, float64)

			yield return Instruction.Create(OpCodes.Ldc_R8, x);
			yield return Instruction.Create(OpCodes.Ldc_R8, y);
			yield return Instruction.Create(OpCodes.Ldc_R8, w);
			yield return Instruction.Create(OpCodes.Ldc_R8, h);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Rectangle"), parameterTypes: new[] {
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double")}));
		}
	}
}
