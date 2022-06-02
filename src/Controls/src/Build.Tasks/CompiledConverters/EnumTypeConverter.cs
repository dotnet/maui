using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class EnumTypeConverter<TEnum> : ICompiledTypeConverter where TEnum : struct
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			if (!string.IsNullOrEmpty(value))
			{
				value = value.Trim();
				if (Enum.TryParse(value, out TEnum enumValue))
				{
					yield return Instruction.Create(OpCodes.Ldc_I4, (int)(object)enumValue);
					yield break;
				}
			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(TEnum));
		}
	}
}