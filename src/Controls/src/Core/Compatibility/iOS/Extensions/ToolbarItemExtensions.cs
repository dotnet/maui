#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
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
		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName)
		{
			return ToUIBarButtonItem(item, false, false);
		}

		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName = false, bool forcePrimary = false)
		{
			if (item.Order == ToolbarItemOrder.Secondary && !forcePrimary)
				return new SecondaryToolbarItem(item);
			return new PrimaryToolbarItem(item, forceName);
		}

		internal static SecondarySubToolbarItem ToSecondarySubToolbarItem(this ToolbarItem item)
		{
			var weakItem = new WeakReference<ToolbarItem>(item);

			var action = UIAction.Create(item.Text, null, null, _ =>
			{
				if (weakItem.TryGetTarget(out var targetItem))
				{
					if (targetItem is IMenuItemController menuItemController)
					{
						menuItemController.Activate();
					}
					else
					{
						targetItem.Command?.Execute(targetItem.CommandParameter);
					}
				}
			});

			if (item.IconImageSource != null && !item.IconImageSource.IsEmpty)
			{
				item.IconImageSource.LoadImage(item.FindMauiContext(), result =>
				{
					action.Image = result?.Value;
				});
			}

			return new SecondarySubToolbarItem(item, action);
		}

		static UIImage ScaleImageToSystemDefaults(ImageSource imageSource, UIImage uIImage)
		{
			var icon = uIImage;

			var originalImageSize = icon?.Size ?? CGSize.Empty;

			// The largest height you can use for navigation bar icons in iOS.
			// Per Apple's Human Interface Guidelines, the navigation bar height is 44 points,
			// so using the full height ensures maximum visual clarity and maintains consistency
			// with iOS design standards. This allows icons to utilize the entire available
			// vertical space within the navigation bar container.
			var defaultIconHeight = 44f;
			var buffer = 0.1;
			// We only check height because the navigation bar constrains vertical space (44pt height),
			// but allows horizontal flexibility. Width can vary based on icon design and content,
			// while height must fit within the fixed navigation bar bounds to avoid clipping.

			// if the image is bigger than the default available size, resize it
			if (icon is not null && originalImageSize.Height - defaultIconHeight > buffer)
			{
				if (imageSource is not FontImageSource fontImageSource || !fontImageSource.IsSet(FontImageSource.SizeProperty))
				{
					icon = icon.ResizeImageSource(originalImageSize.Width, defaultIconHeight, originalImageSize);
				}
			}

			return icon;
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
						Image = result?.Value;
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

		internal sealed class SecondarySubToolbarItem
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

			public SecondarySubToolbarItem(ToolbarItem item, UIAction nativeItem)
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

		sealed class SecondaryToolbarItem : UIBarButtonItem
		{
			readonly WeakReference<ToolbarItem> _item;

			public SecondaryToolbarItem(ToolbarItem item) : base(new SecondaryToolbarItemContent())
			{
				_item = new(item);
				UpdateText(item);
				UpdateIcon(item);
				UpdateIsEnabled(item);

				((SecondaryToolbarItemContent)CustomView).TouchUpInside += OnClicked;
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

				if (e.PropertyName == MenuItem.TextProperty.PropertyName)
					UpdateText(item);
				else if (e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
					UpdateIcon(item);
				else if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
					UpdateIsEnabled(item);
#pragma warning disable CS0618 // Type or member is obsolete
				else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
					this.SetAccessibilityHint(item);
				else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
					this.SetAccessibilityLabel(item);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void UpdateIcon(ToolbarItem item)
			{
				if (item.IconImageSource is not null && !item.IconImageSource.IsEmpty)
				{
					item.IconImageSource.LoadImage(item.FindMauiContext(), result =>
					{
						((SecondaryToolbarItemContent)CustomView).Image = ScaleImageToSystemDefaults(item.IconImageSource, result?.Value);
					});
				}
				else
				{
					((SecondaryToolbarItemContent)CustomView).Image = null;
				}
			}

			void UpdateIsEnabled(ToolbarItem item)
			{
				((UIControl)CustomView).Enabled = item.IsEnabled;
			}

			void UpdateText(ToolbarItem item)
			{
				((SecondaryToolbarItemContent)CustomView).Text = item.Text;
			}

			sealed class SecondaryToolbarItemContent : UIControl
			{
				readonly UIImageView _imageView;
				readonly UILabel _label;

				public SecondaryToolbarItemContent()
				{
					BackgroundColor = UIColor.Clear;
					_imageView = new UIImageView { BackgroundColor = UIColor.Clear };
					AddSubview(_imageView);

					_label = new UILabel { BackgroundColor = UIColor.Clear, Lines = 1, LineBreakMode = UILineBreakMode.TailTruncation, Font = UIFont.SystemFontOfSize(10) };
					AddSubview(_label);
				}

				public override bool Enabled
				{
					get { return base.Enabled; }
					set
					{
						base.Enabled = value;
						_label.Enabled = value;
						_imageView.Alpha = value ? 1f : 0.25f;
					}
				}

				public UIImage Image
				{
					get { return _imageView.Image; }
					set { _imageView.Image = value; }
				}

				public string Text
				{
					get { return _label.Text; }
					set { _label.Text = value; }
				}

				public override void LayoutSubviews()
				{
					base.LayoutSubviews();

					const float padding = 5f;
					var imageSize = _imageView.SizeThatFitsImage(Bounds.Size);
					var fullStringSize = _label.SizeThatFits(Bounds.Size);

					if (imageSize.Width > 0 && (string.IsNullOrEmpty(Text) || fullStringSize.Width > Bounds.Width / 3))
					{
						_imageView.Frame = new RectangleF(PointF.Empty, imageSize);
						_imageView.Center = new PointF(Bounds.GetMidX(), Bounds.GetMidY());
						_label.Hidden = true;
						return;
					}

					_label.Hidden = false;
					var availableWidth = Bounds.Width - padding * 3 - imageSize.Width;
					var stringSize = _label.SizeThatFits(new SizeF(availableWidth, Bounds.Height - padding * 2));

					availableWidth = Bounds.Width;
					availableWidth -= stringSize.Width;
					availableWidth -= imageSize.Width;

					var x = availableWidth / 2;

					var frame = new RectangleF(new PointF(x, Bounds.GetMidY() - imageSize.Height / 2), imageSize);
					_imageView.Frame = frame;

					frame.X = frame.Right + (imageSize.Width > 0 ? padding : 0);
					frame.Size = stringSize;
					frame.Height = Bounds.Height;
					frame.Y = 0;
					_label.Frame = frame;
				}
			}
		}
	}
}