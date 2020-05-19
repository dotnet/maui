using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.ConstraintTypeConverter")]
	[Xaml.TypeConversion(typeof(Constraint))]
	public class ConstraintTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			double size;
			if (value != null && double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
				return Constraint.Constant(size);

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Constraint)));
		}
	}
}