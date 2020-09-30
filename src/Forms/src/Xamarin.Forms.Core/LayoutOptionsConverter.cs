using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.LayoutOptionsConverter")]
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
	}
}