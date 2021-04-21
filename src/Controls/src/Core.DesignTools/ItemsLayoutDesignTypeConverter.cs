namespace Microsoft.Maui.Controls.Core.Design
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;

	public class ItemsLayoutDesignTypeConverter : TypeConverter
	{
		public ItemsLayoutDesignTypeConverter()
		{
		}

		protected StandardValuesCollection Values { get; set; }

		// This tells XAML this converter can be used to process strings
		// Without this the values won't show up as hints
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> Values ??= new StandardValuesCollection(new List<string> { "VerticalList", "HorizontalList", "VerticalGrid", "HorizontalGrid" });

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;
	}
}