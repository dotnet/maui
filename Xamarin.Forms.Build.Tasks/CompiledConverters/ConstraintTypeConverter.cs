using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Xamarin.Forms.Xaml;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Core.XamlC
{
	class ConstraintTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			double size;

			if (string.IsNullOrEmpty(value) || !double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Constraint)}", node);

			yield return Instruction.Create(OpCodes.Ldc_R8, size);

			var constantDef = module.ImportReferenceCached(typeof(Constraint)).ResolveCached().Methods.FirstOrDefault(md => md.IsStatic && md.Name == "Constant");
			var constantRef = module.ImportReference(constantDef);
			yield return Instruction.Create(OpCodes.Call, constantRef);
		}
	}
}
