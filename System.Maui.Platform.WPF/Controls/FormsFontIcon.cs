using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsFontIcon : FormsElementIcon
	{
		public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("UriSource", typeof(FontImageSource), typeof(FormsFontIcon), new PropertyMetadata(OnSourceChanged));

		public FontImageSource Source
		{
			get { return (FontImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public FormsFontIcon()
		{
			this.DefaultStyleKey = typeof(FormsFontIcon);
		}
		
		private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((FormsFontIcon)o).OnSourceChanged(e.OldValue, e.NewValue);
		}

		private void OnSourceChanged(object oldValue, object newValue)
		{
			if (newValue is FontImageSource src)
			{
				Source = src;
			}
		}
	}
}
