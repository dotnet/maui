using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class SafeAreaGroupArrayTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (!string.IsNullOrEmpty(value))
			{
				var parts = value.Split(',');
				var result = new List<Instruction>();
				
				// Create array
				result.Add(Instruction.Create(OpCodes.Ldc_I4, parts.Length));
				result.Add(Instruction.Create(OpCodes.Newarr, module.ImportReference(("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaGroup"))));
				
				// Populate array elements
				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					result.Add(Instruction.Create(OpCodes.Dup)); // Duplicate array reference
					result.Add(Instruction.Create(OpCodes.Ldc_I4, i)); // Array index
					
					if (string.Equals(part, "All", System.StringComparison.OrdinalIgnoreCase))
					{
						result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)Microsoft.Maui.Controls.SafeAreaGroup.All));
					}
					else if (string.Equals(part, "None", System.StringComparison.OrdinalIgnoreCase))
					{
						result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)Microsoft.Maui.Controls.SafeAreaGroup.None));
					}
					else
					{
						throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Microsoft.Maui.Controls.SafeAreaGroup[]));
					}
					
					result.Add(Instruction.Create(OpCodes.Stelem_I4)); // Store element
				}
				
				return result;
			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Microsoft.Maui.Controls.SafeAreaGroup[]));
		}
	}
}