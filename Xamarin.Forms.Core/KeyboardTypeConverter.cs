using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(Keyboard))]
	public class KeyboardTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				string[] parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Keyboard"))
				{
					string keyboard = parts[parts.Length - 1];
					FieldInfo field = typeof(Keyboard).GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == keyboard);
					if (field != null)
						return (Keyboard)field.GetValue(null);
					PropertyInfo property = typeof(Keyboard).GetProperties().FirstOrDefault(pi => pi.Name == keyboard && pi.CanRead && pi.GetMethod.IsStatic);
					if (property != null)
						return (Keyboard)property.GetValue(null, null);
				}
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Keyboard)));
		}
	}
}