using System;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	public class MauiCommandBar : CommandBar
	{
		Microsoft.UI.Xaml.Controls.Button _moreButton;
		Microsoft.UI.Xaml.Controls.ItemsControl _primaryItemsControl;
		bool _isInValidLocation;

		public MauiCommandBar()
		{
			PrimaryCommands.VectorChanged += OnCommandsChanged;
			SecondaryCommands.VectorChanged += OnCommandsChanged;
			UpdateVisibility();
			WatchForContentChanges();
		}

		// Set by the container if the container is a valid place to show a toolbar.
		// This exists to provide consistency with the other platforms; we've got 
		// rules in place that limit toolbars to Navigation Page and to Tabbed 
		// and Flyout-Detail Pages when they're currently displaying a Navigation Page
		public bool IsInValidLocation
		{
			get { return _isInValidLocation; }
			set
			{
				_isInValidLocation = value;
				UpdateVisibility();
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_moreButton = GetTemplateChild("MoreButton") as Microsoft.UI.Xaml.Controls.Button;
			_primaryItemsControl = GetTemplateChild("PrimaryItemsControl") as Microsoft.UI.Xaml.Controls.ItemsControl;
		}

		void OnCommandsChanged(IObservableVector<ICommandBarElement> sender, IVectorChangedEventArgs args)
		{
			UpdateVisibility();
		}

		void UpdateVisibility()
		{
			// Determine whether we have a title (or some other content) inside this command bar
			var frameworkElement = Content as FrameworkElement;

			// Apply the rules for consistency with other platforms

			// Not in one of the acceptable toolbar locations from the other platforms
			if (!IsInValidLocation)
			{
				// If there's no title to display (e.g., toolbarplacement is set to bottom)
				// or the title is collapsed (e.g., because it's empty)
				if (frameworkElement == null || frameworkElement.Visibility != WVisibility.Visible)
				{
					// Just collapse the whole thing
					Visibility = WVisibility.Collapsed;
					return;
				}
			
				// The title needs to be visible, but we're not allowed to show a toolbar
				// So we need to hide the toolbar items

				Visibility = WVisibility.Visible;

				if (_moreButton != null)
				{
					_moreButton.Visibility = WVisibility.Collapsed;
				}

				if (_primaryItemsControl != null)
				{
					_primaryItemsControl.Visibility = WVisibility.Collapsed;
				}

				return;
			}

			// We're in one of the acceptable toolbar locations from the other platforms so the normal rules apply

			if (_primaryItemsControl != null)
			{
				// This is normally visible by default, but it might have been collapsed by the toolbar consistency rules above
				_primaryItemsControl.Visibility = WVisibility.Visible;
			}

			// Are there any commands to display?
			var visibility = PrimaryCommands.Count + SecondaryCommands.Count > 0 ? WVisibility.Visible : WVisibility.Collapsed;

			if (_moreButton != null)
			{
				// The "..." button should only be visible if we have commands to display
				_moreButton.Visibility = visibility;

				// There *is* an OverflowButtonVisibility property that does more or less the same thing, 
				// but it became available in 10.0.14393.0 and we have to support 10.0.10240
			}
			
			if (frameworkElement != null && frameworkElement.Visibility != WVisibility.Collapsed)
			{
				// If there's a title to display, we have to be visible whether or not we have commands
				Visibility = WVisibility.Visible;
			}
			else
			{
				// Otherwise, visibility depends on whether we have commands
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
			var binding = new Microsoft.UI.Xaml.Data.Binding
			{
				Source = this,
				Path = new PropertyPath(nameof(Content)),
				Mode = Microsoft.UI.Xaml.Data.BindingMode.OneWay
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
			(d as MauiCommandBar)?.UpdateVisibility();
		}
	}
}