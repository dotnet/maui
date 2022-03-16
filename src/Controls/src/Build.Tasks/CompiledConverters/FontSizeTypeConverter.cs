using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class FontSizeTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (!string.IsNullOrEmpty(value))
			{
				value = value.Trim();
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
				{
					yield return Instruction.Create(OpCodes.Ldc_R8, size);
					yield break;
				}	
			
				if (Enum.TryParse<NamedSize>(value, out NamedSize namedSize))
				{
					//Device.GetNamedSize(namedSize, targetObject)
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)namedSize);
					var parent = node.Parent as IElementNode;
					if (parent != null && context.Variables.ContainsKey(parent))
					    yield return Instruction.Create(OpCodes.Ldloc, context.Variables[parent]);
					else
					    yield return Instruction.Create(OpCodes.Ldnull);

					yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(
						    ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Device"),
						    methodName: "GetNamedSize",
						    parameterTypes: new[] {("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "NamedSize"),("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Element")},
						    isStatic: true));

					yield break;
				}
			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(double));
		}
	}
}
