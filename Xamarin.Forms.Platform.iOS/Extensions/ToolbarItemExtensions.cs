using System;
using System.Drawing;
using System.ComponentModel;
#if __UNIFIED__
using CoreGraphics;
using UIKit;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
#endif
#if __UNIFIED__
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

#else
using nfloat=System.Single;
using nint=System.Int32;
using nuint=System.UInt32;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public static class ToolbarItemExtensions
	{
		public static UIBarButtonItem ToUIBarButtonItem(this ToolbarItem item, bool forceName = false)
		{
			return item.Order == ToolbarItemOrder.Secondary ? new SecondaryToolbarItem(item) : (UIBarButtonItem)new PrimaryToolbarItem(item, forceName);
		}

		sealed class PrimaryToolbarItem : UIBarButtonItem
		{
			readonly bool _forceName;
			readonly ToolbarItem _item;

			IMenuItemController Controller => _item;

			public PrimaryToolbarItem(ToolbarItem item, bool forceName)
			{
				_forceName = forceName;
				_item = item;

				if (!string.IsNullOrEmpty(item.Icon) && !forceName)
					UpdateIconAndStyle();
				else
					UpdateTextAndStyle();
				UpdateIsEnabled();

				Clicked += (sender, e) => Controller.Activate();
				item.PropertyChanged += OnPropertyChanged;

				if (item != null && !string.IsNullOrEmpty(item.AutomationId))
					AccessibilityIdentifier = item.AutomationId;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_item.PropertyChanged -= OnPropertyChanged;
				base.Dispose(disposing);
			}

			void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == Controller.IsEnabledPropertyName)
					UpdateIsEnabled();
				else if (e.PropertyName == MenuItem.TextProperty.PropertyName)
				{
					if (string.IsNullOrEmpty(_item.Icon) || _forceName)
						UpdateTextAndStyle();
				}
				else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
				{
					if (!_forceName)
					{
						if (!string.IsNullOrEmpty(_item.Icon))
							UpdateIconAndStyle();
						else
							UpdateTextAndStyle();
					}
				}
			}

			void UpdateIconAndStyle()
			{
				var image = UIImage.FromBundle(_item.Icon);
				Image = image;
				Style = UIBarButtonItemStyle.Plain;
			}

			void UpdateIsEnabled()
			{
				Enabled = Controller.IsEnabled;
			}

			void UpdateTextAndStyle()
			{
				Title = _item.Text;
				Style = UIBarButtonItemStyle.Bordered;
				Image = null;
			}
		}

		sealed class SecondaryToolbarItem : UIBarButtonItem
		{
			readonly ToolbarItem _item;
			IMenuItemController Controller => _item;

			public SecondaryToolbarItem(ToolbarItem item) : base(new SecondaryToolbarItemContent())
			{
				_item = item;
				UpdateText();
				UpdateIcon();
				UpdateIsEnabled();

				((SecondaryToolbarItemContent)CustomView).TouchUpInside += (sender, e) => Controller.Activate();
				item.PropertyChanged += OnPropertyChanged;

				if (item != null && !string.IsNullOrEmpty(item.AutomationId))
					AccessibilityIdentifier = item.AutomationId;
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
				else if (e.PropertyName == MenuItem.IconProperty.PropertyName)
					UpdateIcon();
				else if (e.PropertyName == Controller.IsEnabledPropertyName)
					UpdateIsEnabled();
			}

			void UpdateIcon()
			{
				((SecondaryToolbarItemContent)CustomView).Image = string.IsNullOrEmpty(_item.Icon) ? null : new UIImage(_item.Icon);
			}

			void UpdateIsEnabled()
			{
				((UIControl)CustomView).Enabled = Controller.IsEnabled;
			}

			void UpdateText()
			{
				((SecondaryToolbarItemContent)CustomView).Text = _item.Text;
			}

			sealed class SecondaryToolbarItemContent : UIControl
			{
				readonly UIImageView _imageView;
				readonly UILabel _label;

				public SecondaryToolbarItemContent() : base(new RectangleF(0, 0, 75, 20))
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
					var imageSize = _imageView.SizeThatFits(Bounds.Size);
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