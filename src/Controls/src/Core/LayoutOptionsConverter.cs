using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>Class that takes a string representation of a <see cref="Microsoft.Maui.Controls.LayoutOptions"/> and returns a corresponding <see cref="Microsoft.Maui.Controls.LayoutOptions"/>.</summary>
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.LayoutOptionsConverter")]
	public sealed class LayoutOptionsConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> true;

#pragma warning disable CS0618 // Type or member is obsolete (AndExpand options)
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				var parts = strValue.Split('.');
				if (parts.Length > 2 || (parts.Length == 2 && parts[0] != "LayoutOptions"))
					throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(LayoutOptions)}");
				strValue = parts[parts.Length - 1];
				switch (strValue)
				{
					case "Start":
						return LayoutOptions.Start;
					case "Center":
						return LayoutOptions.Center;
					case "End":
						return LayoutOptions.End;
					case "Fill":
						return LayoutOptions.Fill;
					case "StartAndExpand":
						return LayoutOptions.StartAndExpand;
					case "CenterAndExpand":
						return LayoutOptions.CenterAndExpand;
					case "EndAndExpand":
						return LayoutOptions.EndAndExpand;
					case "FillAndExpand":
						return LayoutOptions.FillAndExpand;
				}
				FieldInfo? field = typeof(LayoutOptions).GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == strValue);
				if (field is not null)
				{
					return field.GetValue(null);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(LayoutOptions)}");
		}
#pragma warning restore CS0618 // Type or member is obsolete

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not LayoutOptions options)
				throw new NotSupportedException();
			if (options.Alignment == LayoutAlignment.Start)
				return $"{nameof(LayoutAlignment.Start)}{(options.Expands ? "AndExpand" : "")}";
			if (options.Alignment == LayoutAlignment.Center)
				return $"{nameof(LayoutAlignment.Center)}{(options.Expands ? "AndExpand" : "")}";
			if (options.Alignment == LayoutAlignment.End)
				return $"{nameof(LayoutAlignment.End)}{(options.Expands ? "AndExpand" : "")}";
			if (options.Alignment == LayoutAlignment.Fill)
				return $"{nameof(LayoutAlignment.Fill)}{(options.Expands ? "AndExpand" : "")}";
			throw new NotSupportedException();
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
			=> true;

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
			=> false;

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
			=> new(new[] {
				"Start",
				"Center",
				"End",
				"Fill",
				"StartAndExpand",
				"CenterAndExpand",
				"EndAndExpand",
				"FillAndExpand"
			});
	}
}
