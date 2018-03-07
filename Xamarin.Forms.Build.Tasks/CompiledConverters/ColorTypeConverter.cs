using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class ColorTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			do {
				if (string.IsNullOrEmpty(value))
					break;

				value = value.Trim();

				if (value.StartsWith("#", StringComparison.Ordinal)) {
					var color = Color.FromHex(value);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.R);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.G);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.B);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.A);

					var colorCtorRef = module.ImportCtorReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Color"),
																  paramCount: 4,
																  predicate: md => md.Parameters.All(p => p.ParameterType.FullName == "System.Double"));
					yield return Instruction.Create(OpCodes.Newobj, colorCtorRef);
					yield break;
				}
				var parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts [0] == "Color")) {
					var color = parts [parts.Length - 1];

					var fieldReference = module.ImportFieldReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Color"),
																	 color,
																	 fd => fd.IsStatic);
					if (fieldReference != null) {
						yield return Instruction.Create(OpCodes.Ldsfld, fieldReference);
						yield break;
					}
					var propertyGetterReference = module.ImportPropertyGetterReference(("Xamarin.Forms.Core", "Xamarin.Forms", "Color"),
																					   color,
																					   pd => pd.GetMethod.IsStatic);
					if (propertyGetterReference != null) {
						yield return Instruction.Create(OpCodes.Call, propertyGetterReference);
						yield break;
					}
				}
			} while (false);

			throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Color)}", node);
		}
	}
}