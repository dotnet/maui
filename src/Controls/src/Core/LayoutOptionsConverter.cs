using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.LayoutOptionsConverter")]
	[Xaml.TypeConversion(typeof(LayoutOptions))]
	public sealed class LayoutOptionsConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				var parts = value.Split('.');
				if (parts.Length > 2 || (parts.Length == 2 && parts[0] != "LayoutOptions"))
					throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(LayoutOptions)}");
				value = parts[parts.Length - 1];
				switch (value)
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
				FieldInfo field = typeof(LayoutOptions).GetFields().FirstOrDefault(fi => fi.IsStatic && fi.Name == value);
				if (field != null)
					return (LayoutOptions)field.GetValue(null);
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(LayoutOptions)}");
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is LayoutOptions options))
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
	}
}