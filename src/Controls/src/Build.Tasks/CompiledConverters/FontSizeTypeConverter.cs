using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
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

#pragma warning disable CS0612 // Type or member is obsolete
				if (Enum.TryParse(value, out NamedSize namedSize))
				{
					//Device.GetNamedSize(namedSize, targetObject.GetType())
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)namedSize);
					var parent = node.Parent as IElementNode;
					if (parent != null && context.Variables.TryGetValue(parent, out VariableDefinition variable))
					{
						yield return Instruction.Create(OpCodes.Ldloc, variable);
						yield return Instruction.Create(OpCodes.Callvirt, module.ImportMethodReference(
							context.Cache,
							module.TypeSystem.Object,
							methodName: "GetType"));
					}
					else
					{
						yield return Instruction.Create(OpCodes.Ldtoken, module.ImportReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Label")));
						yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(
							context.Cache,
							("mscorlib", "System", "Type"),
							methodName: "GetTypeFromHandle",
							parameterTypes: new[] { ("mscorlib", "System", "RuntimeTypeHandle") },
							isStatic: true));
					}
					yield return Instruction.Create(OpCodes.Call, module.ImportMethodReference(
							context.Cache,
							("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Device"),
							methodName: "GetNamedSize",
							parameterTypes: new[] { ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "NamedSize"), ("System.Runtime", "System", "Type") },
							isStatic: true));

					yield break;
				}
#pragma warning restore CS0612 // Type or member is obsolete
			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(double));
		}
	}
}
