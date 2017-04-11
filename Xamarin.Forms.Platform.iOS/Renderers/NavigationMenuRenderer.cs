using System;
using System.Linq;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class NavigationMenuRenderer : ViewRenderer
	{
		UICollectionView _collectionView;

		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);
			var pad = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
			var size = pad ? 220 : 142;
			var margin = pad ? 27 : 12;
			var bottomMargin = (int)(margin * 0.8);

			_collectionView = new UICollectionView(new RectangleF(0, 0, 100, 100),
				new UICollectionViewFlowLayout
				{
					ItemSize = new SizeF(size, size + 30),
					ScrollDirection = UICollectionViewScrollDirection.Vertical,
					SectionInset = new UIEdgeInsets(margin, margin, bottomMargin, margin),
					MinimumInteritemSpacing = margin,
					MinimumLineSpacing = margin
				})
			{ DataSource = new DataSource((NavigationMenu)Element), BackgroundColor = UIColor.White };

			using (var navigationCellId = new NSString("NavigationCell"))
				_collectionView.RegisterClassForCell(typeof(NavigationCell), navigationCellId);

			SetNativeControl(_collectionView);
		}

		sealed class NavigationCell : UICollectionViewCell
		{
			readonly UIButton _image = new UIButton(RectangleF.Empty);

			readonly UILabel _nameLabel = new UILabel(RectangleF.Empty) { BackgroundColor = UIColor.Clear, TextAlignment = UITextAlignment.Center, Font = UIFont.SystemFontOfSize(14) };

			string _icon;

			[Export("initWithFrame:")]
			public NavigationCell(RectangleF frame) : base(frame)
			{
				SetupLayer();
				_image.TouchUpInside += (object sender, EventArgs e) =>
				{
					if (Selected != null)
						Selected();
				};
				_image.ContentMode = UIViewContentMode.ScaleAspectFit;
				_image.Center = ContentView.Center;

				ContentView.AddSubview(_image);
				ContentView.AddSubview(_nameLabel);
			}

			public string Icon
			{
				get { return _icon; }
				set
				{
					_icon = value;
					_image.SetImage(new UIImage(_icon), UIControlState.Normal);
				}
			}

			public string Name
			{
				get { return _nameLabel.Text; }
				set { _nameLabel.Text = value; }
			}

			public new Action Selected { get; set; }

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				_image.Frame = new RectangleF(0, 0, ContentView.Frame.Width, ContentView.Frame.Height - 30);
				var sizeThatFits = _nameLabel.SizeThatFits(ContentView.Frame.Size);
				_nameLabel.Frame = new RectangleF(0, ContentView.Frame.Height - 15 - sizeThatFits.Height / 2, ContentView.Frame.Width, sizeThatFits.Height);
			}

			void SetupLayer()
			{
				var layer = _image.Layer;

				layer.ShadowRadius = 6;
				layer.ShadowColor = UIColor.Black.CGColor;
				layer.ShadowOpacity = 0.2f;
				layer.ShadowOffset = new SizeF();

				layer.RasterizationScale = UIScreen.MainScreen.Scale;
				layer.ShouldRasterize = true;
			}
		}

		class DataSource : UICollectionViewDataSource
		{
			readonly NavigationMenu _menu;

			public DataSource(NavigationMenu menu)
			{
				_menu = menu;
			}

			public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
			{
				var cell = (NavigationCell)collectionView.DequeueReusableCell(new NSString("NavigationCell"), indexPath);
				var target = _menu.Targets.Skip(indexPath.Row).FirstOrDefault();

				if (target != null)
				{
					cell.Name = target.Title;
					cell.Icon = target.Icon?.File;
					cell.Selected = () => _menu.SendTargetSelected(target);
				}
				else
				{
					cell.Selected = null;
					cell.Icon = "";
					cell.Name = "";
				}

				return cell;
			}

			public override nint GetItemsCount(UICollectionView collectionView, nint section)
			{
				return _menu.Targets.Count();
			}
		}
	}
}