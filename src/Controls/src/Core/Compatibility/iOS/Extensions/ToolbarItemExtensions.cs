#nullable disable
using System.ComponentModel;
using CoreGraphics;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class ToolbarItemExtensions
	{
		public static UIKit.UIBarButtonItem ToUIBarButtonItem(this Microsoft.Maui.Controls.ToolbarItem item, bool forceName)
		{
			return ToUIBarButtonItem(item, false, false);
		}

		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName = false, bool forcePrimary = false)
		{
			if (item.Order == ToolbarItemOrder.Secondary && !forcePrimary)
				return new SecondaryToolbarItem(item);
			return new PrimaryToolbarItem(item, forceName);
		}

		sealed class PrimaryToolbarItem : UIBarButtonItem
		{
			readonly bool _forceName;
			readonly ToolbarItem _item;

			public PrimaryToolbarItem(ToolbarItem item, bool forceName)
			{
				_forceName = forceName;
				_item = item;

				if (item.IconImageSource != null && !item.IconImageSource.IsEmpty && !forceName)
					UpdateIconAndStyle();
				else
					UpdateTextAndStyle();
				UpdateIsEnabled();

				Clicked += (sender, e) => ((IMenuItemController)_item).Activate();
				item.PropertyChanged += OnPropertyChanged;

				if (item != null && !string.IsNullOrEmpty(item.AutomationId))
					AccessibilityIdentifier = item.AutomationId;

				this.SetAccessibilityHint(item);
				this.SetAccessibilityLabel(item);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_item.PropertyChanged -= OnPropertyChanged;
				base.Dispose(disposing);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
					UpdateIsEnabled();
				else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
				{
					if (_item.IconImageSource == null || _item.IconImageSource.IsEmpty || _forceName)
						UpdateTextAndStyle();
				}
				else if (e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
				{
					if (!_forceName)
					{
						if (_item.IconImageSource != null && !_item.IconImageSource.IsEmpty)
							UpdateIconAndStyle();
						else
							UpdateTextAndStyle();
					}
				}
#pragma warning disable CS0618 // Type or member is obsolete
				else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
					this.SetAccessibilityHint(_item);
				else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
					this.SetAccessibilityLabel(_item);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void UpdateIconAndStyle()
			{
				if (_item?.IconImageSource == null)
				{
					Image = null;
					Style = UIBarButtonItemStyle.Plain;
				}
				else
				{
					_item.IconImageSource.LoadImage(_item.FindMauiContext(), result =>
					{
						Image = result?.Value;
						Style = UIBarButtonItemStyle.Plain;
					});
				}
			}

			void UpdateIsEnabled()
			{
				Enabled = _item.IsEnabled;
			}

			void UpdateTextAndStyle()
			{
				Title = _item.Text;
#pragma warning disable CA1416, CA1422 // TODO: [UnsupportedOSPlatform("ios8.0")]
				Style = UIBarButtonItemStyle.Bordered;
#pragma warning restore CA1416, CA1422
				Image = null;
			}
		}

		sealed class SecondaryToolbarItem : UIBarButtonItem
		{
			readonly ToolbarItem _item;

			public SecondaryToolbarItem(ToolbarItem item) : base(new SecondaryToolbarItemContent())
			{
				_item = item;
				UpdateText();
				UpdateIcon();
				UpdateIsEnabled();

				((SecondaryToolbarItemContent)CustomView).TouchUpInside += (sender, e) => ((IMenuItemController)_item).Activate();
				item.PropertyChanged += OnPropertyChanged;

				if (item != null && !string.IsNullOrEmpty(item.AutomationId))
					AccessibilityIdentifier = item.AutomationId;

				this.SetAccessibilityHint(item);
				this.SetAccessibilityLabel(item);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_item.PropertyChanged -= OnPropertyChanged;
				base.Dispose(disposing);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == MenuItem.TextProperty.PropertyName)
					UpdateText();
				else if (e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
					UpdateIcon();
				else if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
					UpdateIsEnabled();
#pragma warning disable CS0618 // Type or member is obsolete
				else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
					this.SetAccessibilityHint(_item);
				else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
#pragma warning restore CS0618 // Type or member is obsolete
					this.SetAccessibilityLabel(_item);
			}

			void UpdateIcon()
			{
				if (_item.IconImageSource != null && !_item.IconImageSource.IsEmpty)
				{
					_item.IconImageSource.LoadImage(_item.FindMauiContext(), result =>
					{
						((SecondaryToolbarItemContent)CustomView).Image = result?.Value;
					});
				}
				else
				{
					((SecondaryToolbarItemContent)CustomView).Image = null;
				}
			}

			void UpdateIsEnabled()
			{
				((UIControl)CustomView).Enabled = _item.IsEnabled;
			}

			void UpdateText()
			{
				((SecondaryToolbarItemContent)CustomView).Text = _item.Text;
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