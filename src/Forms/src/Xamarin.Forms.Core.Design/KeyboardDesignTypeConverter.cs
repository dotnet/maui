namespace Xamarin.Forms.Core.Design
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;

	public class KeyboardDesignTypeConverter : TypeConverter
	{
		public KeyboardDesignTypeConverter()
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
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (Values == null)
			{
				var props = typeof(Keyboard)
					.GetProperties(BindingFlags.Static | BindingFlags.Public)
					.Where(p => p.CanRead)
					.Select(p => p.Name)
					.ToArray();
				Values = new StandardValuesCollection(props);
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