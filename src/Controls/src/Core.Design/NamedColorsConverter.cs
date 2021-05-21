using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.Design
{
	public class NamedColorsConverter : System.ComponentModel.TypeConverter
	{
		class NameColorMapping
		{
			public NameColorMapping(string name, Graphics.Color color)
			{
				Name = name;
				Color = color;
			}

			public string Name { get; set; }
			public Graphics.Color Color { get; set; }
		}

		public NamedColorsConverter()
			: base()
		{
			NamedColors = new List<NameColorMapping>();

			var fields = typeof(Graphics.Colors)
					.GetFields(BindingFlags.Static | BindingFlags.Public);

			foreach (var f in fields)
			{
				if (f.GetValue(null) is Graphics.Color c)
					NamedColors.Add(new NameColorMapping(f.Name, c));
			}
		}

		List<NameColorMapping> NamedColors { get; }

		// String -> Color
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		// Color -> String
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		// String -> Color
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strVal = value?.ToString();

			if (string.IsNullOrEmpty(strVal))
				return Graphics.Colors.Black;

			var namedColor = NamedColors.FirstOrDefault(n => n.Name.Equals(strVal))?.Color;

			if (namedColor != null)
				return namedColor;

			try
			{
				return Graphics.Color.FromArgb(strVal.TrimStart('#'));
			}
			catch { }

			return Graphics.Colors.Black;
		}

		// Color -> String
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is Graphics.Color c)
			{
				var f = NamedColors.FirstOrDefault(n => n.Color.Equals(c));
				if (f != null)
					return f.Name;

				return $"#{c.ToHex()}";
			}

			return null;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			=> new(NamedColors.Select(c => c.Name).ToArray());
	}
}
