using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public abstract class DataTemplateSelector : ContentControl
	{
		public abstract System.Windows.DataTemplate SelectTemplate(object item, DependencyObject container);

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			ContentTemplate = SelectTemplate(newContent, this);
		}
	}
}