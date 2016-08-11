using System;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	public class ColorTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (value.Trim().StartsWith("#", StringComparison.Ordinal))
					return Color.FromHex(value);
				string[] parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Color"))
				{
					string color = parts[parts.Length - 1];
					switch (color)
					{
						case "Default":
							return Color.Default;
						case "Transparent":
							return Color.Transparent;
						case "Aqua":
							return Color.Aqua;
						case "Black":
							return Color.Black;
						case "Blue":
							return Color.Blue;
						case "Fuchsia":
							return Color.Fuchsia;
						case "Gray":
							return Color.Gray;
						case "Green":
							return Color.Green;
						case "Lime":
							return Color.Lime;
						case "Maroon":
							return Color.Maroon;
						case "Navy":
							return Color.Navy;
						case "Olive":
							return Color.Olive;
						case "Orange":
							return Color.Orange;
						case "Purple":
							return Color.Purple;
						case "Pink":
							return Color.Pink;
						case "Red":
							return Color.Red;
						case "Silver":
							return Color.Silver;
						case "Teal":
							return Color.Teal;
						case "White":
							return Color.White;
						case "Yellow":
							return Color.Yellow;
					}
					FieldInfo field = typeof(Color).GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == color);
					if (field != null)
						return (Color)field.GetValue(null);
					PropertyInfo property = typeof(Color).GetProperties().FirstOrDefault(pi => pi.Name == color && pi.CanRead && pi.GetMethod.IsStatic);
					if (property != null)
						return (Color)property.GetValue(null, null);
				}
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Color)));
		}
	}
}