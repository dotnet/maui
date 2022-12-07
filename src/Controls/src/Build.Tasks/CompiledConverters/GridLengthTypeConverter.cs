using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
{
	class GridLengthTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;
			if (!string.IsNullOrWhiteSpace(value))
			{

				value = value.Trim();

				if (value.Equals("auto", StringComparison.OrdinalIgnoreCase))
				{
					yield return Create(Ldsfld, module.ImportFieldReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "GridLength"), nameof(GridLength.Auto), isStatic: true));
					yield break;
				}
				if (value.Equals("*", StringComparison.OrdinalIgnoreCase))
				{
					yield return Create(Ldsfld, module.ImportFieldReference(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "GridLength"), nameof(GridLength.Star), isStatic: true));
					yield break;
				}
				if (value.EndsWith("*", StringComparison.OrdinalIgnoreCase) && double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var length))
				{
					yield return Create(Ldc_R8, length);
					yield return Create(Ldc_I4, (int)GridUnitType.Star);
					yield return Create(Newobj, module.ImportCtorReference(context.Cache,
							type: module.GetTypeDefinition(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "GridLength")),
							parameterTypes: new[] { module.TypeSystem.Double, module.GetTypeDefinition(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "GridUnitType")) }));
					yield break;
				}
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out length))
				{
					yield return Create(Ldc_R8, length);
					yield return Create(Newobj, module.ImportCtorReference(context.Cache,
							type: module.GetTypeDefinition(context.Cache, ("Microsoft.Maui", "Microsoft.Maui", "GridLength")),
							parameterTypes: new[] { module.TypeSystem.Double }));
					yield break;
				}

			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(GridLength));
		}
	}
}
