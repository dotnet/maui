using CoreGraphics;
using Foundation;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootHeader : UICollectionViewController, IAppearanceObserver
	{
		#region IAppearanceObserver

		Color _defaultBackgroundColor = new Color(0.964);
		Color _defaultForegroundColor = Color.Black;
		Color _defaultUnselectedColor = Color.Black.MultiplyAlpha(0.7);

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		protected virtual void ResetAppearance()
		{
			SetValues(_defaultBackgroundColor, _defaultForegroundColor, _defaultUnselectedColor);
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			SetValues(appearance.BackgroundColor.IsDefault ? _defaultBackgroundColor : appearance.BackgroundColor,
				appearance.ForegroundColor.IsDefault ? _defaultForegroundColor : appearance.ForegroundColor,
				appearance.UnselectedColor.IsDefault ? _defaultUnselectedColor : appearance.UnselectedColor);
		}

		void SetValues(Color backgroundColor, Color foregroundColor, Color unselectedColor)
		{
			CollectionView.BackgroundColor = new Color(backgroundColor.R, backgroundColor.G, backgroundColor.B, .863).ToUIColor();

			bool reloadData = _selectedColor != foregroundColor || _unselectedColor != unselectedColor;

			_selectedColor = foregroundColor;
			_unselectedColor = unselectedColor;

			if (reloadData)
				CollectionView.ReloadData();
		}

		#endregion IAppearanceObserver

		static readonly NSString CellId = new NSString("HeaderCell");

		readonly IShellContext _shellContext;
		UIView _bar;
		UIView _bottomShadow;
		Color _selectedColor;
		Color _unselectedColor;

		public ShellSectionRootHeader(IShellContext shellContext) : base(new UICollectionViewFlowLayout())
		{
			_shellContext = shellContext;
		}

		public double SelectedIndex { get; set; }
		public ShellSection ShellSection { get; set; }

		public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return false;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var headerCell = (ShellSectionHeaderCell)collectionView.DequeueReusableCell(CellId, indexPath);

			var selectedItems = collectionView.GetIndexPathsForSelectedItems();

			var shellContent = ShellSection.Items[indexPath.Row];
			headerCell.Label.Text = shellContent.Title;
			headerCell.Label.SetNeedsDisplay();

			headerCell.SelectedColor = _selectedColor.ToUIColor();
			headerCell.UnSelectedColor = _unselectedColor.ToUIColor();

			if (selectedItems.Length > 0 && selectedItems[0].Row == indexPath.Row)
				headerCell.Selected = true;
			else
				headerCell.Selected = false;

			return headerCell;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return ShellSection.Items.Count;
		}

		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;
			var item = ShellSection.Items[row];
			if (item != ShellSection.CurrentItem)
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, item);
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;
			var item = ShellSection.Items[row];
			IShellController shellController = _shellContext.Shell;

			if (item == ShellSection.CurrentItem)
				return true;
			return shellController.ProposeNavigation(ShellNavigationSource.ShellContentChanged, (ShellItem)ShellSection.Parent, ShellSection, item, ShellSection.Stack, true);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			LayoutBar();

			_bottomShadow.Frame = new CGRect(0, CollectionView.Frame.Bottom, CollectionView.Frame.Width, 0.5);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			CollectionView.ScrollsToTop = false;
			CollectionView.Bounces = false;
			CollectionView.AlwaysBounceHorizontal = false;
			CollectionView.ShowsHorizontalScrollIndicator = false;
			CollectionView.ClipsToBounds = false;

			_bar = new UIView(new CGRect(0, 0, 20, 20));
			_bar.BackgroundColor = UIColor.White;
			_bar.Layer.ZPosition = 9001; //its over 9000!
			CollectionView.AddSubview(_bar);

			_bottomShadow = new UIView(new CGRect(0, 0, 10, 1));
			_bottomShadow.BackgroundColor = Color.Black.MultiplyAlpha(0.3).ToUIColor();
			_bottomShadow.Layer.ZPosition = 9002;
			CollectionView.AddSubview(_bottomShadow);

			var flowLayout = Layout as UICollectionViewFlowLayout;
			flowLayout.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
			flowLayout.MinimumInteritemSpacing = 0;
			flowLayout.MinimumLineSpacing = 0;
			flowLayout.EstimatedItemSize = new CGSize(70, 35);

			CollectionView.RegisterClassForCell(typeof(ShellSectionHeaderCell), CellId);

			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, ShellSection);
			((INotifyCollectionChanged)ShellSection.Items).CollectionChanged += OnShellSectionItemsChanged;

			UpdateSelectedIndex();
			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);
				((INotifyCollectionChanged)ShellSection.Items).CollectionChanged -= OnShellSectionItemsChanged;
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;

				ShellSection = null;
				_bar.RemoveFromSuperview();
				_bar.Dispose();
				_bar = null;
			}
		}

		protected void LayoutBar()
		{
			var layout = CollectionView.GetLayoutAttributesForItem(NSIndexPath.FromItemSection((int)SelectedIndex, 0));

			var frame = layout.Frame;

			if (_bar.Frame.Height != 2)
			{
				_bar.Frame = new CGRect(frame.X, frame.Bottom - 2, frame.Width, 2);
			}
			else
			{
				UIView.Animate(.25, () => _bar.Frame = new CGRect(frame.X, frame.Bottom - 2, frame.Width, 2));
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				UpdateSelectedIndex();
			}
		}

		protected virtual void UpdateSelectedIndex(bool animated = false)
		{
			SelectedIndex = ShellSection.Items.IndexOf(ShellSection.CurrentItem);
			LayoutBar();

			CollectionView.SelectItem(NSIndexPath.FromItemSection((int)SelectedIndex, 0), false, UICollectionViewScrollPosition.CenteredHorizontally);
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CollectionView.ReloadData();
		}

		public class ShellSectionHeaderCell : UICollectionViewCell
		{
			public UIColor SelectedColor { get; set; }
			public UIColor UnSelectedColor { get; set; }

			public override bool Selected
			{
				get => base.Selected;
				set
				{
					base.Selected = value;
					Label.TextColor = value ? SelectedColor : UnSelectedColor;
				}
			}

			[Export("initWithFrame:")]
			public ShellSectionHeaderCell(CGRect frame) : base(frame)
			{
				Label = new UILabel();
				Label.TextAlignment = UITextAlignment.Center;
				Label.Font = UIFont.BoldSystemFontOfSize(14);
				ContentView.AddSubview(Label);
			}

			public UILabel Label { get; }

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				Label.Frame = Bounds;
			}

			public override CGSize SizeThatFits(CGSize size)
			{
				return new CGSize(Label.SizeThatFits(size).Width + 30, 35);
			}
		}
	}
}