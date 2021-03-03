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

		protected StandardValuesCollection Values
		{
			get;
			set;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			// This tells XAML this converter can be used to process strings
			// Without this the values won't show up as hints
			if (sourceType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (Values == null)
			{
				var names = new List<string>() { "VerticalList", "HorizontalList", "VerticalGrid", "HorizontalGrid" };
				Values = new StandardValuesCollection(names);
			}

			return Values;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}