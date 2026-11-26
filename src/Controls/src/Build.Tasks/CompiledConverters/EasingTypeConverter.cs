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

			// The curve functions and CSS keywords share the runtime grammar (EasingSyntax.cs is linked).
			var parsed = EasingSyntax.Parse(value);
			if (parsed.Kind == EasingSyntax.Kind.Invalid)
				throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Easing));

			if (parsed.Kind != EasingSyntax.Kind.Named)
			{
				var easingType = ("Microsoft.Maui", "Microsoft.Maui", nameof(Easing));
				var easingModule = context.Body.Method.Module;
				switch (parsed.Kind)
				{
					case EasingSyntax.Kind.CubicBezier:
						yield return Create(Ldc_R8, parsed.A);
						yield return Create(Ldc_R8, parsed.B);
						yield return Create(Ldc_R8, parsed.C);
						yield return Create(Ldc_R8, parsed.D);
						yield return Create(Call, easingModule.ImportMethodReference(context.Cache, easingType, "CubicBezier", paramCount: 4, isStatic: true));
						break;
					case EasingSyntax.Kind.Spring:
						yield return Create(Ldc_R8, parsed.A);
						yield return Create(Call, easingModule.ImportMethodReference(context.Cache, easingType, "Spring", paramCount: 1, isStatic: true));
						break;
					default:
						yield return Create(Ldc_I4, (int)parsed.A);
						yield return Create(parsed.Flag ? Ldc_I4_1 : Ldc_I4_0);
						yield return Create(Call, easingModule.ImportMethodReference(context.Cache, easingType, "Steps", paramCount: 2, isStatic: true));
						break;
				}
				yield break;
			}

			value = parsed.Name;
			var parts = value.Split('.');
			if (parts.Length == 2 && parts[0] == nameof(Easing))
				value = parts[parts.Length - 1];

			// Easing lives in Microsoft.Maui (Core), not in Controls.
			var assemblyTypeInfo = ("Microsoft.Maui", "Microsoft.Maui", nameof(Easing));

			var module = context.Body.Method.Module;
			var fieldReference = module.ImportFieldReference(context.Cache, assemblyTypeInfo, value, isStatic: true, caseSensitive: false);

			if (fieldReference != null)
			{
				yield return Create(Ldsfld, fieldReference);
				yield break;
			}

			throw new BuildException(BuildExceptionCode.Conversion, node, null, value, typeof(Easing));
		}
	}
}