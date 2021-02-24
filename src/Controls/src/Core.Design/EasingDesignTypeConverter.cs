namespace Microsoft.Maui.Controls.Core.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;

	public class EasingDesignTypeConverter : TypeConverter
	{
		readonly Lazy<StandardValuesCollection> _lazyValues = new Lazy<StandardValuesCollection>(() =>
		{
			var props = typeof(Easing)
				.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(p => p.Name)
				.ToArray();
			return new StandardValuesCollection(props);
		});

		protected StandardValuesCollection Values
			=> _lazyValues.Value;

		// This tells XAML this converter can be used to process strings
		// Without this the values won't show up as hints
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> Values;

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;
	}
}
