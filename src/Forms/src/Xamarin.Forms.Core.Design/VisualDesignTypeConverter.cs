namespace Xamarin.Forms.Core.Design
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

	public class VisualDesignTypeConverter : TypeConverter
	{
		public VisualDesignTypeConverter()
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
				var derivedNames = new List<string>();
				var baseType = typeof(IVisual);

				var typeElements = typeof(View).Assembly.ExportedTypes.Where(t => baseType.IsAssignableFrom(t) && t.Name != baseType.Name);

				foreach (var typeElement in typeElements)
				{
					string name = typeElement.Name;
					if (name.EndsWith("Visual", StringComparison.OrdinalIgnoreCase))
						name = name.Substring(0, name.Length - 6);

					derivedNames.Add(name);
				}

				Values = new StandardValuesCollection(derivedNames);
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