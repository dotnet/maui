#nullable enable

using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		//public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
		//{
		//	if (radioButton.ResolveControlTemplate() != null)
		//	{
		//		//if (handler.PlatformView is ContentPanel contentPanel && handler.MauiContext != null)
		//		//{
		//		//	// Cleanup the old view when reused
		//		//	contentPanel.Children.Clear();

		//		//	if (handler.VirtualView.PresentedContent is IView view)
		//		//		contentPanel.Children.Add(view.ToPlatform(handler.MauiContext));
		//		//}

		//		return;
		//	}

		//	RadioButtonHandler.MapContent(handler, radioButton);
		//}

		static UI.Xaml.Controls.RadioButton? CreatePlatformView(ViewHandler<IRadioButton, UI.Xaml.Controls.RadioButton> radioButton)
		{
			// If someone is using a completely different type for IRadioButton			
			if (radioButton.VirtualView is not RadioButton rb)
				return null;

			if (rb.ResolveControlTemplate() == null)
			{
				return null;
			}

			return new TemplatedRadioButton();
		}

		class TemplatedRadioButton : UI.Xaml.Controls.RadioButton
		{
			//readonly IContentView _contentView;
			//ContentPanel? _contentPanel;
			public TemplatedRadioButton()
			{
				Style = Microsoft.UI.Xaml.Application.Current.Resources["RadioButtonControlStyle"] as Microsoft.UI.Xaml.Style;
			}

			//protected override void OnApplyTemplate()
			//{
			//	base.OnApplyTemplate();
			//	_contentPanel = (ContentPanel)GetTemplateChild("ContentPanel");
			//	_contentPanel.Children.Clear();
			//	_contentPanel.Children.Add(_contentView.PresentedContent!.ToPlatform(_contentView.Handler!.MauiContext!)!);
			//}

			//protected override Size MeasureOverride(Size availableSize)
			//{
			//	base.MeasureOverride(availableSize);

			//	_contentPanel?.Measure(availableSize);
			//	return _contentPanel!.DesiredSize;
			//}

			//protected override Size ArrangeOverride(Size finalSize)
			//{
			//	return base.ArrangeOverride(finalSize);
			//}
		}

		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();

			//if (Handler?.PlatformView is UI.Xaml.Controls.RadioButton rb)
			//{
			//	rb.Loaded += Rb_Loaded;

			//	void Rb_Loaded(object sender, RoutedEventArgs e)
			//	{
			//		var contentPanel = rb.GetDescendantByName<ContentPanel>("ContentPanel");
			//		if (contentPanel == null || Handler?.MauiContext == null)
			//			return;

			//		// Cleanup the old view when reused
			//		contentPanel.Children.Clear();

			//		if (((IContentView)this).PresentedContent is IView view)
			//			contentPanel.Children.Add(view.ToPlatform(Handler.MauiContext));
			//	}
			//}
		}
	}
}
