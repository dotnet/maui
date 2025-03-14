#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.AccessibilityExtensions;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSectionRootHeader : UICollectionViewController, IAppearanceObserver, IShellSectionRootHeader
	{
		#region IAppearanceObserver

		Color _defaultBackgroundColor = new Color(0.964f);
		Color _defaultForegroundColor = Colors.Black;
		Color _defaultUnselectedColor = Colors.Black.MultiplyAlpha(0.7f);

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
			SetValues(appearance.BackgroundColor ?? _defaultBackgroundColor,
				appearance.ForegroundColor ?? _defaultForegroundColor,
				appearance.UnselectedColor ?? _defaultUnselectedColor);
		}

		void SetValues(Color backgroundColor, Color foregroundColor, Color unselectedColor)
		{
			CollectionView.BackgroundColor = new Color(backgroundColor.Red, backgroundColor.Green, backgroundColor.Blue, .863f).ToPlatform();

			bool reloadData = _selectedColor != foregroundColor || _unselectedColor != unselectedColor;

			_selectedColor = foregroundColor;
			_unselectedColor = unselectedColor;

			if (reloadData)
				ReloadData();
		}

		#endregion IAppearanceObserver

		static readonly NSString CellId = new NSString("HeaderCell");

		readonly IShellContext _shellContext;
		UIView _bar;
		UIView _bottomShadow;
		Color _selectedColor;
		Color _unselectedColor;
		bool _isDisposed;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ShellSectionRootHeader()
		{

		}

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public ShellSectionRootHeader(IShellContext shellContext) : base(new UICollectionViewFlowLayout())
		{
			_shellContext = shellContext;
		}

		public double SelectedIndex { get; set; }
		public ShellSection ShellSection { get; set; }
		IShellSectionController ShellSectionController => ShellSection;

		public UIViewController ViewController => this;

		public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return false;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var reusedCell = (UICollectionViewCell)collectionView.DequeueReusableCell(CellId, indexPath);
			var headerCell = reusedCell as ShellSectionHeaderCell;

			if (headerCell == null)
				return reusedCell;

			var selectedItems = collectionView.GetIndexPathsForSelectedItems();

			var shellContent = ShellSectionController.GetItems()[indexPath.Row];
			headerCell.Label.Text = shellContent.Title;
			headerCell.Label.SetNeedsDisplay();

			headerCell.SelectedColor = _selectedColor.ToPlatform();
			headerCell.UnSelectedColor = _unselectedColor.ToPlatform();

			if (selectedItems.Length > 0 && selectedItems[0].Row == indexPath.Row)
				headerCell.Selected = true;
			else
				headerCell.Selected = false;

			headerCell.SetAccessibilityProperties(shellContent);
			return headerCell;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return ShellSectionController.GetItems().Count;
		}

		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (CollectionView.CellForItem(indexPath) is ShellSectionHeaderCell cell)
				cell.Label.TextColor = _unselectedColor.ToPlatform();
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;

			var item = ShellSectionController.GetItems()[row];

			if (item != ShellSection.CurrentItem)
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, item);

			if (CollectionView.CellForItem(indexPath) is ShellSectionHeaderCell cell)
				cell.Label.TextColor = _selectedColor.ToPlatform();
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;
			var item = ShellSectionController.GetItems()[row];
			IShellController shellController = _shellContext.Shell;

			if (item == ShellSection.CurrentItem)
				return true;
			return shellController.ProposeNavigation(ShellNavigationSource.ShellContentChanged, (ShellItem)ShellSection.Parent, ShellSection, item, ShellSection.Stack, true);
		}

		public override void ViewDidLayoutSubviews()
		{
			if (_isDisposed)
				return;

			base.ViewDidLayoutSubviews();

			LayoutBar();

			_bottomShadow.Frame = new CGRect(0, CollectionView.Frame.Bottom, CollectionView.Frame.Width, 0.5);
		}

		public override void ViewDidLoad()
		{
			if (_isDisposed)
				return;

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
			_bottomShadow.BackgroundColor = Colors.Black.MultiplyAlpha(0.3f).ToPlatform();
			_bottomShadow.Layer.ZPosition = 9002;
			CollectionView.AddSubview(_bottomShadow);

			var flowLayout = Layout as UICollectionViewFlowLayout;
			flowLayout.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
			flowLayout.MinimumInteritemSpacing = 0;
			flowLayout.MinimumLineSpacing = 0;
			flowLayout.EstimatedItemSize = new CGSize(70, 35);

			CollectionView.RegisterClassForCell(GetCellType(), CellId);

			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, ShellSection);
			ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

			UpdateSelectedIndex();
			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
			foreach (var shellContent in ShellSectionController.GetItems())
			{
				shellContent.PropertyChanged += OnShellContentPropertyChanged;
			}
		}

		protected virtual Type GetCellType()
		{
			return typeof(ShellSectionHeaderCell);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);
				ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;
				ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
				foreach (var shellContent in ShellSectionController.GetItems())
				{
					shellContent.PropertyChanged -= OnShellContentPropertyChanged;
				}

				ShellSection = null;
				_bar.RemoveFromSuperview();
				this.RemoveFromParentViewController();
				_bar.Dispose();
				_bar = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		protected void LayoutBar()
		{
			if (SelectedIndex < 0)
				return;

			if (ShellSectionController.GetItems().IndexOf(ShellSection.CurrentItem) != SelectedIndex)
				return;

			var layout = CollectionView.GetLayoutAttributesForItem(NSIndexPath.FromItemSection((int)SelectedIndex, 0));

			if (layout == null)
				return;

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
			if (ShellSection.CurrentItem == null)
				return;

			SelectedIndex = ShellSectionController.GetItems().IndexOf(ShellSection.CurrentItem);

			if (SelectedIndex < 0)
				return;

			LayoutBar();

			CollectionView.SelectItem(NSIndexPath.FromItemSection((int)SelectedIndex, 0), false, UICollectionViewScrollPosition.CenteredHorizontally);
		}

		void OnShellSectionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			HandleEventsOnItemsChange(e);
			ReloadData();
		}

		void HandleEventsOnItemsChange(NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellContent item in e.OldItems)
				{
					item.PropertyChanged -= OnShellContentPropertyChanged;
				}
			}

			if (e.NewItems != null)
			{
				foreach (ShellContent item in e.NewItems)
				{
					item.PropertyChanged += OnShellContentPropertyChanged;
				}
			}
		}

		void OnShellContentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShellContent.Title))
			{
				if (sender is ShellContent shellContent)
				{
					int index = ShellSectionController.GetItems().IndexOf(shellContent);
					if (index >= 0)
					{
						UpdateHeaderTitle(index, shellContent);
					}
				}
			}
		}

		void UpdateHeaderTitle(int index, ShellContent shellContent)
		{
			if (CollectionView.CellForItem(NSIndexPath.FromItemSection(index, 0)) is ShellSectionHeaderCell cell)
			{
				cell.Label.Text = shellContent.Title;
				CollectionView.CollectionViewLayout.InvalidateLayout();
			}
		}

		void ReloadData()
		{
			if (_isDisposed)
				return;

			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();
		}

		public class ShellSectionHeaderCell : UICollectionViewCell
		{
			public UIColor SelectedColor { get; set; }
			public UIColor UnSelectedColor { get; set; }

			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public ShellSectionHeaderCell()
			{

			}

			[Export("initWithFrame:")]
			[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
			public ShellSectionHeaderCell(CGRect frame) : base(frame)
			{
				Label = new UILabel();
				Label.TextAlignment = UITextAlignment.Center;
				Label.Font = UIFont.BoldSystemFontOfSize(14);
				ContentView.AddSubview(Label);
			}

			public override bool Selected
			{
				get => base.Selected;
				set
				{
					base.Selected = value;
					Label.TextColor = value ? SelectedColor : UnSelectedColor;
				}
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
