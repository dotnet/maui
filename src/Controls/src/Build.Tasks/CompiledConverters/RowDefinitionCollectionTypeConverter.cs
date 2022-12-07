using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
{
	class RowDefinitionCollectionTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (!string.IsNullOrWhiteSpace(value))
			{
				value = value.Trim();
				var gridlengthconverter = new GridLengthTypeConverter();
				var parts = value.Split(',');

				yield return Create(Ldc_I4, parts.Length);
				yield return Create(Newarr, module.ImportReference(context.Cache, ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "RowDefinition")));
				for (var i = 0; i < parts.Length; i++)
				{
					yield return Create(Dup);
					yield return Create(Ldc_I4, i);
					foreach (var instruction in gridlengthconverter.ConvertFromString(parts[i], context, node))
						yield return instruction;
					yield return Create(Newobj, module.ImportCtorReference(
							context.Cache,
							type: ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "RowDefinition"),
							parameterTypes: new[] { ("Microsoft.Maui", "Microsoft.Maui", "GridLength") }));
					yield return Create(Stelem_Ref);
				}
				yield return Create(Newobj, module.ImportCtorReference(
						context.Cache,
						type: ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "RowDefinitionCollection"),
						paramCount: 1));
				yield break;

			}
			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(RowDefinitionCollection));
		}
	}
}