using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal class FormsListView : Windows.UI.Xaml.Controls.ListView, IEmptyView
	{
		ContentControl _emptyViewContentControl;
		FrameworkElement _emptyView;

		public Visibility EmptyViewVisibility
		{
			get { return (Visibility)GetValue(EmptyViewVisibilityProperty); }
			set { SetValue(EmptyViewVisibilityProperty, value); }
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility), typeof(FormsListView), new PropertyMetadata(Visibility.Collapsed));

		public void SetEmptyView(FrameworkElement emptyView)
		{
			_emptyView = emptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			if (_emptyView != null)
			{
				_emptyViewContentControl.Content = _emptyView;
			}
		}
	}
}
