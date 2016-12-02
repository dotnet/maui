using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.UWP
{
	public class FormsCommandBar : CommandBar
	{
		// TODO Once 10.0.14393.0 is available (and we don't have to support lower versions), enable dynamic overflow: https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.commandbar.isdynamicoverflowenabled.aspx 

		Windows.UI.Xaml.Controls.Button _moreButton;
		
		public FormsCommandBar()
		{
			PrimaryCommands.VectorChanged += OnCommandsChanged;
			SecondaryCommands.VectorChanged += OnCommandsChanged;
			UpdateVisibility();
			WatchForContentChanges();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_moreButton = GetTemplateChild("MoreButton") as Windows.UI.Xaml.Controls.Button;
		}

		void OnCommandsChanged(IObservableVector<ICommandBarElement> sender, IVectorChangedEventArgs args)
		{
			UpdateVisibility();
		}

		void UpdateVisibility()
		{
			var visibility = PrimaryCommands.Count + SecondaryCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		
			if (_moreButton != null)
			{
				// The "..." button should only be visible if we have commands to display
				_moreButton.Visibility = visibility;

				// There *is* an OverflowButtonVisibility property that does more or less the same thing, 
				// but it became available in 10.0.14393.0 and we have to support 10.0.10240
			}

			// If we have a title (or some other content) inside this command bar
			// and that content is not collapsed
			var frameworkElement = Content as FrameworkElement;

			// Temporarily tie the visibility of the toolbar to the visibility of the Title
			// to be consistent with the old style / other platforms
			if (frameworkElement != null && frameworkElement.Visibility == Visibility.Collapsed)
			{
				Visibility = Visibility.Collapsed;
				return;
			}

			if (frameworkElement != null && frameworkElement.Visibility != Visibility.Collapsed)
			{
				Visibility = Visibility.Visible;
			}
			else
			{
				// Otherwise, collapse it if there are no commands
				Visibility = visibility;
			}
		}

		void WatchForContentChanges()
		{
			// If the content of the command bar changes while it's collapsed, we need to 
			// react and update the visibility (e.g., if the bar is placed at the bottom and
			// has no commands, then is moved to the top and now includes the title)

			// There's no event on CommandBar when the content changes, so we'll bind our own
			// dependency property to Content and update our visibility when it changes
			var binding = new Windows.UI.Xaml.Data.Binding
			{
				Source = this,
				Path = new PropertyPath(nameof(Content)),
				Mode = Windows.UI.Xaml.Data.BindingMode.OneWay
			};

			BindingOperations.SetBinding(this, s_contentChangeWatcher, binding);
		}

		static readonly DependencyProperty s_contentChangeWatcher =
			DependencyProperty.Register(
				"ContentChangeWatcher",
				typeof(object),
				typeof(object),
				new PropertyMetadata(null, ContentChanged));

		static void ContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as FormsCommandBar)?.UpdateVisibility();
		}
	}
}