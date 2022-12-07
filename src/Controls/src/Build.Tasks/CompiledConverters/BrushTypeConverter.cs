using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC;

class BrushTypeConverter : ICompiledTypeConverter
{
	public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
	{
		var module = context.Body.Method.Module;

		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.StartsWith("#", StringComparison.Ordinal))
			{
				var colorConverter = new ColorTypeConverter();
				foreach (var instruction in colorConverter.ConvertFromString(value, context, node))
					yield return instruction;

				yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SolidColorBrush"), parameterTypes: new[] {
						("Microsoft.Maui.Graphics", "Microsoft.Maui.Graphics", "Color")}));

				yield break;
			}

			var propertyGetterReference = module.ImportPropertyGetterReference(context.Cache,
																				("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Brush"),
																				value,
																				isStatic: true,
																				caseSensitive: false);

			if (propertyGetterReference != null)
			{
				yield return Instruction.Create(OpCodes.Call, propertyGetterReference);
				yield break;
			}
		}
		throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Brush));
	}
}
