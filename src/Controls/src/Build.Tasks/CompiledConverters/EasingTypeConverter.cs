using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.XamlC
{
	class EasingTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				yield return Create(Ldnull);
				yield break;
			}

			value = value?.Trim() ?? "";
			var parts = value.Split('.');
			if (parts.Length == 2 && parts[0] == nameof(Easing))
				value = parts[parts.Length - 1];

			var assemblyTypeInfo = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", nameof(Easing));

			var module = context.Body.Method.Module;
			var fieldReference = module.ImportFieldReference(assemblyTypeInfo, value, isStatic: true, caseSensitive: false);

			if (fieldReference != null)
			{
				yield return Create(Ldsfld, fieldReference);
				yield break;
			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Easing));
		}
	}
}