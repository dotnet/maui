using System.Windows;
using System.Windows.Media;

namespace System.Maui.Platform.WPF.Controls
{
	public class FormsTabbedPage : FormsMultiPage
	{
		public static readonly DependencyProperty BarBackgroundColorProperty = DependencyProperty.Register("BarBackgroundColor", typeof(Brush), typeof(FormsTabbedPage));
		public static readonly DependencyProperty BarTextColorProperty = DependencyProperty.Register("BarTextColor", typeof(Brush), typeof(FormsTabbedPage));

		public Brush BarBackgroundColor
		{
			get { return (Brush)GetValue(BarBackgroundColorProperty); }
			set { SetValue(BarBackgroundColorProperty, value); }
		}

		public Brush BarTextColor
		{
			get { return (Brush)GetValue(BarTextColorProperty); }
			set { SetValue(BarTextColorProperty, value); }
		}

		public FormsTabbedPage()
		{
			this.DefaultStyleKey = typeof(FormsTabbedPage);
		}
	}
}
