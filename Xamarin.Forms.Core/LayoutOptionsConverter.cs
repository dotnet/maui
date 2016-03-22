using System;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	public sealed class LayoutOptionsConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				string[] parts = value.Split('.');
				if (parts.Length > 2 || (parts.Length == 2 && parts[0] != "LayoutOptions"))
					throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(LayoutOptions)));
				value = parts[parts.Length - 1];
				FieldInfo field = typeof(LayoutOptions).GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == value);
				if (field != null)
					return (LayoutOptions)field.GetValue(null);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(LayoutOptions)));
		}
	}
}