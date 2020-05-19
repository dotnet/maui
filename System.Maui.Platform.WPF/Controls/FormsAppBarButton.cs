using System.Windows;
using System.Windows.Controls;


namespace System.Maui.Platform.WPF.Controls
{
	public class FormsAppBarButton : System.Windows.Controls.Button
	{
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FormsElementIcon), typeof(FormsAppBarButton));
		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(FormsAppBarButton));

		public FormsElementIcon Icon
		{
			get { return (FormsElementIcon)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public FormsAppBarButton()
		{
			this.DefaultStyleKey = typeof(FormsAppBarButton);
		}
	}
}
