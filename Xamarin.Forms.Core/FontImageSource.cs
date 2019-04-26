using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using ValidateValueDelegate = Xamarin.Forms.BindableProperty.ValidateValueDelegate;

namespace Xamarin.Forms
{
	public partial class FontImageSource : ImageSource
	{
		private static BindableProperty CreateBindableProperty<T>(
			string name,
			T defaultValue = default(T))
		{
			return BindableProperty.Create(
				propertyName: name,
				returnType: typeof(T),
				declaringType: typeof(FontImageSource),
				defaultValue: defaultValue
			);
		}

		public override bool IsEmpty => string.IsNullOrEmpty(Glyph);

		public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
		public static readonly BindableProperty SizeProperty = CreateBindableProperty(nameof(Size), 30d);

		public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
		public static readonly BindableProperty GlyphProperty = CreateBindableProperty<string>(nameof(Glyph));

		public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
		public static readonly BindableProperty ColorProperty = CreateBindableProperty<Color>(nameof(Color));

		public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
		public static readonly BindableProperty FontFamilyProperty = CreateBindableProperty<string>(nameof(FontFamily));

		private static BindableProperty[] BindableProperties = new[]
		{
			FontFamilyProperty,
			GlyphProperty,
			ColorProperty,
			SizeProperty,
		};
		protected override void OnPropertyChanged(string propertyName = null)
		{
			for (var i = 0; i < BindableProperties.Length; i++)
			{
				var bindableProperty = BindableProperties[i];
				if (propertyName == bindableProperty.PropertyName)
					OnSourceChanged();
			}
			
			base.OnPropertyChanged(propertyName);
		}
	}
}
