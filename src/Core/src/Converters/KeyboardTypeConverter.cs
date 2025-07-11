using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.Converters
{
	/// <inheritdoc/>
	public class KeyboardTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				string[] parts = strValue.Split('.');

				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Keyboard"))
				{
					var kbType = typeof(Keyboard);

					string keyboard = parts[parts.Length - 1];
					FieldInfo? field = kbType.GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == keyboard);
					if (field?.GetValue(null) is Keyboard kb)
						return kb;

					PropertyInfo? property = kbType.GetProperties().FirstOrDefault(pi => pi.Name == keyboard && pi.CanRead && (pi.GetMethod?.IsStatic ?? false));
					if (property?.GetValue(null, null) is Keyboard propKb)
						return propKb;
				}
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(Keyboard)));
		}

		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (!(value is Keyboard keyboard))
				throw new NotSupportedException();

			if (keyboard == Keyboard.Plain)
				return nameof(Keyboard.Plain);
			if (keyboard == Keyboard.Chat)
				return nameof(Keyboard.Chat);
			if (keyboard == Keyboard.Default)
				return nameof(Keyboard.Default);
			if (keyboard == Keyboard.Email)
				return nameof(Keyboard.Email);
			if (keyboard == Keyboard.Numeric)
				return nameof(Keyboard.Numeric);
			if (keyboard == Keyboard.Telephone)
				return nameof(Keyboard.Telephone);
			if (keyboard == Keyboard.Text)
				return nameof(Keyboard.Text);
			if (keyboard == Keyboard.Url)
				return nameof(Keyboard.Url);
			if (keyboard == Keyboard.Date)
				return nameof(Keyboard.Date);
			if (keyboard == Keyboard.Time)
				return nameof(Keyboard.Time);
			if (keyboard == Keyboard.Password)
				return nameof(Keyboard.Password);

			throw new NotSupportedException();
		}
	}
}