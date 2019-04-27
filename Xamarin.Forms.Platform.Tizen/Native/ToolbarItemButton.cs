using System;
using System.ComponentModel;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class ToolbarItemButton : Button
	{
		const string StyleDefault = "default";
		const string StyleDefaultToolbarIcon = "naviframe/drawers";
		const string StyleLeftToolBarButton = "naviframe/title_left";
		const string StyleRightToolbarButton = "naviframe/title_right";

		ToolbarItem _item;

		public ToolbarItemButton(ToolbarItem item) : base(Forms.NativeParent)
		{
			_item = item;
			BackgroundColor = EColor.Transparent;

			Clicked += OnClicked;
			Deleted += OnDeleted;
			_item.PropertyChanged += OnToolbarItemPropertyChanged;

			UpdateText();
			UpdateIsEnabled();
			UpdateIcon();
		}

		void OnDeleted(object sender, EventArgs e)
		{
			Clicked -= OnClicked;
			Deleted -= OnDeleted;
			_item.PropertyChanged -= OnToolbarItemPropertyChanged;
		}

		void OnClicked(object sender, EventArgs e)
		{
			((IMenuItemController)_item).Activate();
		}

		void OnToolbarItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ToolbarItem.TextProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == ToolbarItem.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
			}
			else if (e.PropertyName == ToolbarItem.IconImageSourceProperty.PropertyName)
			{
				UpdateIcon();
			}
		}

		void UpdateText()
		{
			UpdateStyle();
			Text = _item.Text;
		}

		void UpdateIsEnabled()
		{
			IsEnabled = _item.IsEnabled;
		}

		void UpdateIcon()
		{
			if (_item.IconImageSource.IsNullOrEmpty())
			{
				//On 5.0, the part content should be removed before the style is changed, otherwise, EFL does not remove the part content.
				Image = null;
				UpdateStyle();
			}
			else
			{
				// In reverse, the style should be set before setting the part content.
				UpdateStyle();
				Native.Image iconImage = new Native.Image(Forms.NativeParent);
				_ = iconImage.LoadFromImageSourceAsync(_item.IconImageSource);
				Image = iconImage;
			}
		}

		void UpdateStyle()
		{
			if (_item.IconImageSource.IsNullOrEmpty())
			{
				if (string.IsNullOrEmpty(_item.Text))
				{
					// We assumed the default toolbar icon is "naviframe/drawer" if there are no icon and text.
					Style = StyleDefaultToolbarIcon;
				}
				else
				{
					if (_item.Order == ToolbarItemOrder.Primary)
						Style = StyleRightToolbarButton;
					else
						Style = StyleLeftToolBarButton;
				}
			}
			else
			{
				Style = StyleDefault;
			}
		}
	}
}
