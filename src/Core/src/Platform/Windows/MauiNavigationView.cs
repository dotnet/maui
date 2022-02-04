using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	// This is needed by WinUI because of 
	// https://github.com/microsoft/microsoft-ui-xaml/issues/2698#issuecomment-648751713
	[Microsoft.UI.Xaml.Data.Bindable]
	public class MauiNavigationView : NavigationView
	{
		internal StackPanel? TopNavArea { get; private set; }
		internal ItemsRepeater? TopNavMenuItemsHost { get; private set; }
		internal Grid? PaneContentGrid { get; private set; }
		internal event EventHandler? OnApplyTemplateFinished;

		public MauiNavigationView()
		{
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PaneContentGrid = (Grid)GetTemplateChild("PaneContentGrid");
			TopNavArea = ((StackPanel)GetTemplateChild("TopNavArea"));
			TopNavMenuItemsHost = ((ItemsRepeater)GetTemplateChild("TopNavMenuItemsHost"));
			OnApplyTemplateCore();
			OnApplyTemplateFinished?.Invoke(this, EventArgs.Empty);
		}

		private protected virtual void OnApplyTemplateCore()
		{

		}
	}
}
