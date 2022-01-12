using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class MauiNavigationView : NavigationView
	{
		public MauiNavigationView()
		{
			IsPaneToggleButtonVisible = false;
			PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
			RegisterPropertyChangedCallback(IsBackButtonVisibleProperty, BackButtonVisibleChanged);
		}

		void BackButtonVisibleChanged(DependencyObject sender, DependencyProperty dp)
		{
			IsBackEnabled = (IsBackButtonVisible == NavigationViewBackButtonVisible.Visible);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var HeaderContent = (ContentControl)GetTemplateChild("HeaderContent");

			// This is used to left pad the content/header when the backbutton is visible
			// Because our backbutton is inside the appbar title we don't care about padding 
			// the content and header by the size of the backbutton.
			if (GetTemplateChild("ContentLeftPadding") is Grid g)
				g.Visibility = UI.Xaml.Visibility.Collapsed;

			Binding visibilityBinding = new Binding();
			visibilityBinding.Source = this;
			visibilityBinding.Path = new PropertyPath("Header.Visibility");
			visibilityBinding.Mode = BindingMode.OneWay;
			visibilityBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(HeaderContent, UIElement.VisibilityProperty, visibilityBinding);

			Binding backgroundBinding = new Binding();
			backgroundBinding.Source = this;
			backgroundBinding.Path = new PropertyPath("Header.Background");
			backgroundBinding.Mode = BindingMode.OneWay;
			backgroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(HeaderContent, BackgroundProperty, backgroundBinding);

			Binding isBackButtonVisible = new Binding();
			isBackButtonVisible.Source = this;
			isBackButtonVisible.Path = new PropertyPath("Header.IsBackButtonVisible");
			isBackButtonVisible.Mode = BindingMode.OneWay;
			isBackButtonVisible.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			BindingOperations.SetBinding(this, IsBackButtonVisibleProperty, isBackButtonVisible);
		}
	}
}
