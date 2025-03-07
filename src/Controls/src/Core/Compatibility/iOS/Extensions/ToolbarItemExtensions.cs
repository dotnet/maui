#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class ToolbarItemExtensions
	{
		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName = false, bool forcePrimary = false)
		{
			// if (item.Order == ToolbarItemOrder.Secondary && !forcePrimary)
			// {
			// 	return new SecondaryToolbarItem(item);
			// }

			return new PrimaryToolbarItem(item, forceName);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		internal static SecondaryToolbarItem ToSecondaryToolbarItem(this ToolbarItem item)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			var action = UIAction.Create(item.Text, null, null, _ =>
            {
                item.Command?.Execute(item.CommandParameter);
                ((IMenuItemController)item).Activate();
            });

			if (item.IconImageSource != null && !item.IconImageSource.IsEmpty)
			{
				item.IconImageSource.LoadImage(item.FindMauiContext(), result =>
				{
					action.Image = result?.Value;
				});
			}

            return new SecondaryToolbarItem(item, action);
		}

		sealed class PrimaryToolbarItem : UIBarButtonItem
		{
			readonly bool _forceName;
			readonly WeakReference<ToolbarItem> _item;

			public PrimaryToolbarItem(ToolbarItem item, bool forceName)
			{
				_forceName = forceName;
				_item = new(item);

				if (item.IconImageSource != null && !item.IconImageSource.IsEmpty && !forceName)
					UpdateIconAndStyle(item);
				else
					UpdateTextAndStyle(item);
				UpdateIsEnabled(item);

				Clicked += OnClicked;
				item.PropertyChanged += OnPropertyChanged;

				if (item != null && !string.IsNullOrEmpty(item.AutomationId))
					AccessibilityIdentifier = item.AutomationId;

#pragma warning disable CS0618 // Type or member is obsolete
				this.SetAccessibilityHint(item);
				this.SetAccessibilityLabel(item);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void OnClicked(object sender, EventArgs e)
			{
				if (_item.TryGetTarget(out var item))
				{
					((IMenuItemController)item).Activate();
				}
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && _item.TryGetTarget(out var item))
					item.PropertyChanged -= OnPropertyChanged;
				base.Dispose(disposing);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (!_item.TryGetTarget(out var item))
					return;

				if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
					UpdateIsEnabled(item);
				else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
				{
					if (item.IconImageSource == null || item.IconImageSource.IsEmpty || _forceName)
						UpdateTextAndStyle(item);
				}
				else if (e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
				{
					if (!_forceName)
					{
						if (item.IconImageSource != null && !item.IconImageSource.IsEmpty)
							UpdateIconAndStyle(item);
						else
							UpdateTextAndStyle(item);
					}
				}
#pragma warning disable CS0618 // Type or member is obsolete
				else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
					this.SetAccessibilityHint(item);
				else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
					this.SetAccessibilityLabel(item);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void UpdateIconAndStyle(ToolbarItem item)
			{
				if (item?.IconImageSource == null)
				{
					Image = null;
					Style = UIBarButtonItemStyle.Plain;
				}
				else
				{
					var mauiContext = item.FindMauiContext();
					if (mauiContext is null)
					{
						return;
					}
					item.IconImageSource.LoadImage(mauiContext, result =>
					{
						Image = item.IconImageSource is not FontImageSource
							? result?.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal)
							: result?.Value;
						Style = UIBarButtonItemStyle.Plain;
					});
				}
			}

			void UpdateIsEnabled(ToolbarItem item)
			{
				Enabled = item.IsEnabled;
			}

			void UpdateTextAndStyle(ToolbarItem item)
			{
				Title = item.Text;
#pragma warning disable CA1416, CA1422 // TODO: [UnsupportedOSPlatform("ios8.0")]
				Style = UIBarButtonItemStyle.Bordered;
#pragma warning restore CA1416, CA1422
				Image = null;
			}
		}

		internal sealed class SecondaryToolbarItem
		{
			readonly WeakReference<ToolbarItem> _item;
			readonly WeakReference<UIAction> _nativeItem;

			public UIAction PlatformAction
			{
				get
				{
					if (_nativeItem.TryGetTarget(out var nativeItem))
					{
						return nativeItem;
					}

					return null;
				}
			}

			public SecondaryToolbarItem(ToolbarItem item, UIAction nativeItem)
			{
				_item = new(item);
				_nativeItem = new(nativeItem);

				UpdateText(item);
				UpdateIcon(item);
				UpdateIsEnabled(item);

				item.PropertyChanged += OnPropertyChanged;

				if (item is not null && !string.IsNullOrEmpty(item.AutomationId)
					&& _nativeItem.TryGetTarget(out var nativeAction))
				{
					nativeAction.AccessibilityIdentifier = item.AutomationId;
				}

				//this.SetAccessibilityHint(item);
				//this.SetAccessibilityLabel(item);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (!_item.TryGetTarget(out var item))
					return;

				if (e.PropertyName == MenuItem.TextProperty.PropertyName)
					UpdateText(item);
				else if (e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
					UpdateIcon(item);
				else if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
					UpdateIsEnabled(item);
			}

			void UpdateIcon(ToolbarItem item)
			{
				if (_nativeItem.TryGetTarget(out var nativeItem))
				{
					nativeItem.Image = item.IconImageSource?.GetPlatformMenuImage(item.FindMauiContext());
				}
			}

			void UpdateIsEnabled(ToolbarItem item)
			{
				if (_nativeItem.TryGetTarget(out var nativeItem))
				{
					nativeItem.UpdateIsEnabled(item.IsEnabled);
				}
			}

			void UpdateText(ToolbarItem item)
			{
				if (_nativeItem.TryGetTarget(out var nativeItem))
				{
					nativeItem.Title = item.Text;
				}
			}
		}
	}
}