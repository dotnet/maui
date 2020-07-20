using System.Windows;
using WBrush = System.Windows.Media.Brush;

namespace Xamarin.Forms.Platform.WPF.Controls
{
	public class FormsTabbedPage : FormsMultiPage
	{
		public static readonly DependencyProperty BarBackgroundColorProperty = DependencyProperty.Register("BarBackgroundColor", typeof(WBrush), typeof(FormsTabbedPage));
		public static readonly DependencyProperty BarTextColorProperty = DependencyProperty.Register("BarTextColor", typeof(WBrush), typeof(FormsTabbedPage));

		public WBrush BarBackgroundColor
		{
			get { return (WBrush)GetValue(BarBackgroundColorProperty); }
			set { SetValue(BarBackgroundColorProperty, value); }
		}

		public WBrush BarTextColor
		{
			get { return (WBrush)GetValue(BarTextColorProperty); }
			set { SetValue(BarTextColorProperty, value); }
		}

		public FormsTabbedPage()
		{
			this.DefaultStyleKey = typeof(FormsTabbedPage);
		}
	}
}
