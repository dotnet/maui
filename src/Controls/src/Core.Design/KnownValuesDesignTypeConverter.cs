namespace Microsoft.Maui.Controls.Design
{
	using System;
	using System.ComponentModel;

	public abstract class KnownValuesDesignTypeConverter : TypeConverter
	{
		protected abstract string[] KnownValues { get; }

		// This tells XAML this converter can be used to process strings
		// Without this the values won't show up as hints
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> new(KnownValues);

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;
	}
}
