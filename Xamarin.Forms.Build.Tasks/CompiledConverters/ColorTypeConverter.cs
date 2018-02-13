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
					var colorCtor = module.ImportReferenceCached(typeof(Color)).ResolveCached().Methods.FirstOrDefault(
						md => md.IsConstructor && md.Parameters.Count == 4 &&
						md.Parameters.All(p => p.ParameterType.FullName == "System.Double"));
					var colorCtorRef = module.ImportReference(colorCtor);
					yield return Instruction.Create(OpCodes.Newobj, colorCtorRef);
					yield break;
				}
				var parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts [0] == "Color")) {
					var color = parts [parts.Length - 1];

					var field = module.ImportReferenceCached(typeof(Color)).ResolveCached().Fields.SingleOrDefault(fd => fd.Name == color && fd.IsStatic);
					if (field != null) {
						yield return Instruction.Create(OpCodes.Ldsfld, module.ImportReference(field));
						yield break;
					}
					var propertyGetter = module.ImportReferenceCached(typeof(Color)).ResolveCached().Properties.SingleOrDefault(pd => pd.Name == color && pd.GetMethod.IsStatic)?.GetMethod;
					if (propertyGetter != null) {
						yield return Instruction.Create(OpCodes.Call, module.ImportReference(propertyGetter));
						yield break;
					}
				}
			} while (false);

			throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Color)}", node);
		}
	}
}