using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class RectangleTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (string.IsNullOrEmpty(value))
				throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Rect));
			double x, y, w, h;
			var xywh = value.Split(',');
			if (xywh.Length != 4 ||
				!double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x) ||
				!double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y) ||
				!double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out w) ||
				!double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out h))
				throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Rect));

			return GenerateIL(context, x, y, w, h, module);
		}

		IEnumerable<Instruction> GenerateIL(ILContext context, double x, double y, double w, double h, ModuleDefinition module)
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
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics", "Rect"), parameterTypes: new[] {
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double"),
				("mscorlib", "System", "Double")}));
		}
	}
}