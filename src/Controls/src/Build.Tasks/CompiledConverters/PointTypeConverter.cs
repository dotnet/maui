using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC;

class PointTypeConverter : ICompiledTypeConverter
{
	public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
	{
		var module = context.Body.Method.Module;
		if (!string.IsNullOrEmpty(value) && Point.TryParse(value.Trim(), out var point))
		{
			foreach (var instruction in CreatePoint(context, module, point))
			{
				yield return instruction;
			}
			yield break;
		}
		throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Point));
	}

	public IEnumerable<Instruction> CreatePoint(ILContext context, ModuleDefinition module, Point point)
	{
		yield return Instruction.Create(OpCodes.Ldc_R8, point.X);
		yield return Instruction.Create(OpCodes.Ldc_R8, point.Y);
		yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics", "Point"), parameterTypes: new[] {
					("mscorlib", "System", "Double"),
					("mscorlib", "System", "Double")}));
	}
}
