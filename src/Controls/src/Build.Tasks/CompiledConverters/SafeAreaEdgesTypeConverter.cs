using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class SafeAreaEdgesTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (!string.IsNullOrEmpty(value))
			{
				var parts = value.Split(',');
				var result = new List<Instruction>();
				var regions = new List<Microsoft.Maui.Controls.SafeAreaRegions>();
				
				// Parse all parts
				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();
					
					if (string.Equals(part, "All", System.StringComparison.OrdinalIgnoreCase))
					{
						regions.Add(Microsoft.Maui.Controls.SafeAreaRegions.All);
					}
					else if (string.Equals(part, "None", System.StringComparison.OrdinalIgnoreCase))
					{
						regions.Add(Microsoft.Maui.Controls.SafeAreaRegions.None);
					}
					else
					{
						throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Microsoft.Maui.Controls.SafeAreaEdges));
					}
				}
				
				// Create SafeAreaEdges based on number of parts
				if (parts.Length == 1)
				{
					// Constructor(SafeAreaRegions uniformValue)
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[0]));
					result.Add(Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, 
						("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaEdges"), 
						parameterTypes: new[] { ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions") })));
				}
				else if (parts.Length == 2)
				{
					// Constructor(SafeAreaRegions horizontal, SafeAreaRegions vertical)
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[0])); // horizontal
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[1])); // vertical
					result.Add(Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, 
						("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaEdges"), 
						parameterTypes: new[] { ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions"), 
											   ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions") })));
				}
				else if (parts.Length == 4)
				{
					// Constructor(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[0])); // left
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[1])); // top
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[2])); // right
					result.Add(Instruction.Create(OpCodes.Ldc_I4, (int)regions[3])); // bottom
					result.Add(Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, 
						("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaEdges"), 
						parameterTypes: new[] { ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions"), 
											   ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions"), 
											   ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions"), 
											   ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "SafeAreaRegions") })));
				}
				else
				{
					throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Microsoft.Maui.Controls.SafeAreaEdges));
				}
				
				return result;
			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Microsoft.Maui.Controls.SafeAreaEdges));
		}
	}
}