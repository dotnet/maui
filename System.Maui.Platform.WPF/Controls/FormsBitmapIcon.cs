using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Maui.Platform.WPF.Controls
{
	public class FormsBitmapIcon : FormsElementIcon
	{
		public static readonly DependencyProperty UriSourceProperty = DependencyProperty.Register("UriSource", typeof(Uri), typeof(FormsBitmapIcon), new PropertyMetadata(OnSourceChanged));

		public Uri UriSource
		{
			get { return (Uri)GetValue(UriSourceProperty); }
			set { SetValue(UriSourceProperty, value); }
		}

		public FormsBitmapIcon()
		{
			this.DefaultStyleKey = typeof(FormsBitmapIcon);

		}
		
		private static void OnSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			((FormsBitmapIcon)o).OnSourceChanged(e.OldValue, e.NewValue);
		}

		private void OnSourceChanged(object oldValue, object newValue)
		{
			if (newValue is Uri uri && !uri.IsAbsoluteUri)
			{
				var name = Assembly.GetEntryAssembly().GetName().Name;
				UriSource = new Uri(string.Format("pack://application:,,,/{0};component/{1}", name, uri.OriginalString));
			}
		}
	}
}
