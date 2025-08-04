using System;
using System.Collections.Generic;
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
				value = value.Trim();

				// Split by comma - if no comma, we get array with single element
				var parts = value.Split(',');
				var regions = new int[parts.Length];

				for (int i = 0; i < parts.Length; i++)
				{
					var part = parts[i].Trim();

					// Performance optimization: use string comparison instead of Enum.TryParse
					// since SafeAreaRegions has specific values
					if (string.Equals(part, "All", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = 1 << 15; // SafeAreaRegions.All
					}
					else if (string.Equals(part, "None", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = 0; // SafeAreaRegions.None
					}
					else if (string.Equals(part, "Container", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = 1 << 1; // SafeAreaRegions.Container
					}
					else if (string.Equals(part, "SoftInput", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = 1 << 0; // SafeAreaRegions.SoftInput
					}
					else if (string.Equals(part, "Default", StringComparison.OrdinalIgnoreCase))
					{
						regions[i] = -1; // SafeAreaRegions.Default
					}
					else
					{
						throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(string));
					}
				}

				// Generate IL based on number of regions, same logic as runtime converter
				return regions.Length switch
				{
					1 => GenerateILUniform(module, context, regions[0]),
					2 => GenerateILHorizontalVertical(module, context, regions[0], regions[1]),
					4 => GenerateILAllEdges(module, context, regions[0], regions[1], regions[2], regions[3]),
					_ => throw new BuildException(BuildExceptionCode.Conversion, node, null, $"SafeAreaEdges must have 1, 2, or 4 values, but got {regions.Length}", typeof(string))
				};
			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(string));
		}

		// Generate IL for uniform constructor: new SafeAreaEdges(SafeAreaRegions uniformValue)
		IEnumerable<Instruction> GenerateILUniform(ModuleDefinition module, ILContext context, int uniformValue)
		{
			yield return Instruction.Create(OpCodes.Ldc_I4, uniformValue);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaEdges"), parameterTypes: new[] { ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions") }));
		}

		// Generate IL for horizontal/vertical constructor: new SafeAreaEdges(SafeAreaRegions horizontal, SafeAreaRegions vertical)
		IEnumerable<Instruction> GenerateILHorizontalVertical(ModuleDefinition module, ILContext context, int horizontal, int vertical)
		{
			yield return Instruction.Create(OpCodes.Ldc_I4, horizontal);
			yield return Instruction.Create(OpCodes.Ldc_I4, vertical);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaEdges"), parameterTypes: new[] { ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions"), ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions") }));
		}

		// Generate IL for all edges constructor: new SafeAreaEdges(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
		IEnumerable<Instruction> GenerateILAllEdges(ModuleDefinition module, ILContext context, int left, int top, int right, int bottom)
		{
			yield return Instruction.Create(OpCodes.Ldc_I4, left);
			yield return Instruction.Create(OpCodes.Ldc_I4, top);
			yield return Instruction.Create(OpCodes.Ldc_I4, right);
			yield return Instruction.Create(OpCodes.Ldc_I4, bottom);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaEdges"), parameterTypes: new[] { ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions"), ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions"), ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions"), ("Microsoft.Maui", "Microsoft.Maui", "SafeAreaRegions") }));
		}
	}
}