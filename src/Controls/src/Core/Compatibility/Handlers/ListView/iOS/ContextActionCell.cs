#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.iOS.Resources;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal sealed class ContextActionsCell : UITableViewCell, INativeElementView
	{
		public const string Key = "ContextActionsCell";

		static readonly UIImage DestructiveBackground;
		static readonly UIImage NormalBackground;
		readonly List<UIButton> _buttons = new List<UIButton>();
		readonly List<MenuItem> _menuItems = new List<MenuItem>();

#pragma warning disable CS0618 // Type or member is obsolete
		Cell _cell;
#pragma warning restore CS0618 // Type or member is obsolete
		UIButton _moreButton;
		UIScrollView _scroller;
		UITableView _tableView;
		bool _isDiposed;

		static ContextActionsCell()
		{
			var rect = new RectangleF(0, 0, 1, 1);
			var size = rect.Size;
			using var renderer = new UIGraphicsImageRenderer(size);
			DestructiveBackground = renderer.CreateImage((UIGraphicsImageRendererContext ctx) =>
			{
				FillRect(ctx, rect, ColorExtensions.Red.CGColor);
			});
			NormalBackground = renderer.CreateImage((UIGraphicsImageRendererContext ctx) =>
			{
				FillRect(ctx, rect, ColorExtensions.LightGray.CGColor);
			});
		}

		public ContextActionsCell() : base(UITableViewCellStyle.Default, Key)
		{
		}

		public ContextActionsCell(string templateId) : base(UITableViewCellStyle.Default, Key + templateId)
		{
		}

		public UITableViewCell ContentCell { get; private set; }

		public bool IsOpen => ScrollDelegate.IsOpen;

		ContextScrollViewDelegate ScrollDelegate => (ContextScrollViewDelegate)_scroller.Delegate;

		Element INativeElementView.Element
		{
			get
			{
				var boxedCell = ContentCell as INativeElementView;
				if (boxedCell == null)
				{
					throw new InvalidOperationException($"Implement {nameof(INativeElementView)} on cell renderer: {ContentCell.GetType().AssemblyQualifiedName}");
				}

				return boxedCell.Element;
			}
		}

		public void Close()
		{
			if (_scroller == null)
				return;

			_scroller.ContentOffset = new PointF(0, 0);
		}

		static void FillRect(UIGraphicsImageRendererContext ctx, RectangleF rect, CGColor color)
		{
			var context = ctx.CGContext;
			context.SetFillColor(color);
			context.FillRect(rect);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// Leave room for 1px of play because the border is 1 or .5px and must be accounted for.
			if (_scroller == null || (_scroller.Frame.Width == ContentView.Bounds.Width && Math.Abs(_scroller.Frame.Height - ContentView.Bounds.Height) < 1))
				return;

			Update(_tableView, _cell, ContentCell);

			if (ContentCell is ViewCellRenderer.ViewTableCell && ContentCell.Subviews.Length > 0 && Math.Abs(ContentCell.Subviews[0].Frame.Height - Bounds.Height) > 1)
			{
				// Something goes weird inside iOS where LayoutSubviews wont get called when updating the bounds if the user
				// forces us to flip flop between a ContextActionCell and a normal cell in the middle of actually displaying the cell
				// so here we are going to hack it a forced update. Leave room for 1px of play because the border is 1 or .5px and must
				// be accounted for.
				//
				// Fixes https://bugzilla.xamarin.com/show_bug.cgi?id=39450
				ContentCell.LayoutSubviews();
			}
		}

		public void PrepareForDeselect()
		{
			ScrollDelegate.PrepareForDeselect(_scroller);
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			return ContentCell.SizeThatFits(size);
		}
		public override void RemoveFromSuperview()
		{
			base.RemoveFromSuperview();
			//Some cells are removed  when using ScrollTo but Disposed is not called causing leaks
			//ListviewRenderer disposes it's subviews but there's a chance these were removed already
			//but the dispose logic wasn't called.
			Dispose(true);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public void Update(UITableView tableView, Cell cell, UITableViewCell nativeCell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var parentListView = cell.RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
			var recycling = parentListView != null &&
				((parentListView.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0);
			if (_cell != cell && recycling)
			{
				if (_cell != null)
					((INotifyCollectionChanged)_cell.ContextActions).CollectionChanged -= OnContextItemsChanged;

				((INotifyCollectionChanged)cell.ContextActions).CollectionChanged += OnContextItemsChanged;
			}

#pragma warning disable CS0618 // Type or member is obsolete
			var height = Frame.Height + (parentListView != null && parentListView.SeparatorVisibility == SeparatorVisibility.None ? 0.5f : 0f);
#pragma warning restore CS0618 // Type or member is obsolete
			var width = ContentView.Frame.Width;

			nativeCell.Frame = new RectangleF(0, 0, width, height);
			nativeCell.SetNeedsLayout();

			var handler = new PropertyChangedEventHandler(OnMenuItemPropertyChanged);

			_tableView = tableView;

			if (_cell != null)
			{
				if (!recycling)
					_cell.PropertyChanged -= OnCellPropertyChanged;
				if (_menuItems.Count > 0)
				{
					if (!recycling)
						((INotifyCollectionChanged)_cell.ContextActions).CollectionChanged -= OnContextItemsChanged;

					foreach (var item in _menuItems)
						item.PropertyChanged -= handler;
				}

				_menuItems.Clear();
			}

			_menuItems.AddRange(cell.ContextActions);

			_cell = cell;
			if (!recycling)
			{
				cell.PropertyChanged += OnCellPropertyChanged;
				((INotifyCollectionChanged)_cell.ContextActions).CollectionChanged += OnContextItemsChanged;
			}

			var isOpen = false;

			if (_scroller == null)
			{
				_scroller = new UIScrollView(new RectangleF(0, 0, width, height));
				_scroller.ScrollsToTop = false;
				_scroller.ShowsHorizontalScrollIndicator = false;

				_scroller.PreservesSuperviewLayoutMargins = true;

				ContentView.AddSubview(_scroller);
			}
			else
			{
				_scroller.Frame = new RectangleF(0, 0, width, height);
				isOpen = ScrollDelegate.IsOpen;

				for (var i = 0; i < _buttons.Count; i++)
				{
					var b = _buttons[i];
					b.RemoveFromSuperview();
					b.Dispose();
				}

				_buttons.Clear();

				ScrollDelegate.Unhook(_scroller);
				ScrollDelegate.Dispose();
			}

			if (ContentCell != nativeCell)
			{
				ContentCell?.RemoveFromSuperview();
				ContentCell = null;

				ContentCell = nativeCell;

				//Hack: if we have a ImageCell the insets are slightly different,
				//the inset numbers user below were taken using the Reveal app from the default cells
#pragma warning disable CS0618 // Type or member is obsolete
				if ((ContentCell as CellTableViewCell)?.Cell is ImageCell)
				{
					nfloat imageCellInsetLeft = 57;
					nfloat imageCellInsetRight = 0;
					if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
					{
						imageCellInsetLeft = 89;
						imageCellInsetRight = imageCellInsetLeft / 2;
					}
					SeparatorInset = new UIEdgeInsets(0, imageCellInsetLeft, 0, imageCellInsetRight);
				}
#pragma warning restore CS0618 // Type or member is obsolete

				_scroller.AddSubview(nativeCell);
			}

			SetupButtons(width, height);

			UIView container = null;

			var totalWidth = width;
			for (var i = _buttons.Count - 1; i >= 0; i--)
			{
				var b = _buttons[i];
				totalWidth += b.Frame.Width;
				_scroller.AddSubview(b);
			}

			_scroller.Delegate = new ContextScrollViewDelegate(container, _buttons, isOpen);
			_scroller.ContentSize = new SizeF(totalWidth, height);

			if (isOpen)
				_scroller.SetContentOffset(new PointF(ScrollDelegate.ButtonsWidth, 0), false);
			else
				_scroller.SetContentOffset(new PointF(0, 0), false);

			if (ContentCell != null)
			{
				SelectionStyle = ContentCell.SelectionStyle;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDiposed)
			{
				_isDiposed = true;

				if (_scroller != null)
				{
					_scroller.Delegate = null;
					_scroller.Dispose();
					_scroller = null;
				}

				_tableView = null;

				_moreButton?.Dispose();
				_moreButton = null;

				for (var i = 0; i < _buttons.Count; i++)
					_buttons[i].Dispose();

				var handler = new PropertyChangedEventHandler(OnMenuItemPropertyChanged);

				foreach (var item in _menuItems)
					item.PropertyChanged -= handler;

				_buttons.Clear();
				_menuItems.Clear();

				if (_cell != null)
				{
					if (_cell.HasContextActions)
						((INotifyCollectionChanged)_cell.ContextActions).CollectionChanged -= OnContextItemsChanged;
					_cell = null;
				}
			}

			base.Dispose(disposing);
		}

		void ActivateMore()
		{
			var displayed = new HashSet<nint>();
			for (var i = 0; i < _buttons.Count; i++)
			{
				var tag = _buttons[i].Tag;
				if (tag >= 0)
					displayed.Add(tag);
			}

			var frame = _moreButton.Frame;

			var x = frame.X - _scroller.ContentOffset.X;

			var path = _tableView.IndexPathForCell(this);
			var rowPosition = _tableView.RectForRowAtIndexPath(path);
			var sourceRect = new RectangleF(x, rowPosition.Y, rowPosition.Width, rowPosition.Height);
			var actionSheet = new MoreActionSheetController();

			for (var i = 0; i < _cell.ContextActions.Count; i++)
			{
				if (displayed.Contains(i))
					continue;

				var item = _cell.ContextActions[i];
				var weakItem = new WeakReference<MenuItem>(item);
				var action = UIAlertAction.Create(item.Text, UIAlertActionStyle.Default, a =>
				{
					if (_scroller == null)
						return;

					_scroller.SetContentOffset(new PointF(0, 0), true);
					if (weakItem.TryGetTarget(out MenuItem mi))
						((IMenuItemController)mi).Activate();
				});
				actionSheet.AddAction(action);
			}

			var controller = GetController();
			if (controller == null)
				throw new InvalidOperationException("No UIViewController found to present.");

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone || (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && actionSheet.PopoverPresentationController == null))
			{
				var cancel = UIAlertAction.Create(StringResources.Cancel, UIAlertActionStyle.Cancel, null);
				actionSheet.AddAction(cancel);
			}
			else
			{
				if (actionSheet.PopoverPresentationController != null)
				{
					actionSheet.PopoverPresentationController.SourceView = _tableView;
					actionSheet.PopoverPresentationController.SourceRect = sourceRect;
				}
			}

			controller.PresentViewController(actionSheet, true, null);
		}

		void CullButtons(nfloat acceptableTotalWidth, ref bool needMoreButton, ref nfloat largestButtonWidth)
		{
			while (largestButtonWidth * (_buttons.Count + (needMoreButton ? 1 : 0)) > acceptableTotalWidth && _buttons.Count > 1)
			{
				needMoreButton = true;

				var button = _buttons[_buttons.Count - 1];
				_buttons.RemoveAt(_buttons.Count - 1);

				if (largestButtonWidth == button.Frame.Width)
					largestButtonWidth = GetLargestWidth();
			}

			if (needMoreButton && _cell.ContextActions.Count - _buttons.Count == 1)
				_buttons.RemoveAt(_buttons.Count - 1);
		}

		UIButton GetButton(MenuItem item)
		{
			var button = new UIButton(new RectangleF(0, 0, 1, 1));

			if (!item.IsDestructive)
				button.SetBackgroundImage(NormalBackground, UIControlState.Normal);
			else
				button.SetBackgroundImage(DestructiveBackground, UIControlState.Normal);

			button.SetTitle(item.Text, UIControlState.Normal);

#pragma warning disable CA1416, CA1422 // TODO: 'UIButton.TitleEdgeInsets' is unsupported on: 'ios' 15.0 and later
			button.TitleEdgeInsets = new UIEdgeInsets(0, 15, 0, 15);
#pragma warning restore CA1416, CA1422

			button.Enabled = item.IsEnabled;

			return button;
		}

		UIViewController GetController()
		{
			Element e = _cell;
			while (e.RealParent != null)
			{
				var renderer = (IPlatformViewHandler)e.RealParent.ToHandler(e.FindMauiContext());
				if (renderer.ViewController != null)
					return renderer.ViewController;

				e = e.RealParent;
			}

			return null;
		}

		nfloat GetLargestWidth()
		{
			nfloat largestWidth = 0;
			for (var i = 0; i < _buttons.Count; i++)
			{
				var frame = _buttons[i].Frame;
				if (frame.Width > largestWidth)
					largestWidth = frame.Width;
			}

			return largestWidth;
		}

		void OnButtonActivated(object sender, EventArgs e)
		{
			var button = (UIButton)sender;
			if (button.Tag == -1)
				ActivateMore();
			else
			{
				_scroller.SetContentOffset(new PointF(0, 0), true);
				((IMenuItemController)_cell.ContextActions[(int)button.Tag]).Activate();
			}
		}

		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "HasContextActions")
			{
				if (_cell == null)
					return;

#pragma warning disable CS0618 // Type or member is obsolete
				var recycling = _cell.RealParent is ListView parentListView &&
					((parentListView.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0);
#pragma warning restore CS0618 // Type or member is obsolete

				if (!recycling)
					ReloadRow();
			}
		}

		void OnContextItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var parentListView = _cell?.RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
			var recycling = parentListView != null &&
				((parentListView.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0);
			if (recycling)
				Update(_tableView, _cell, ContentCell);
			else
				ReloadRow();
			// TODO: Perhaps make this nicer if it's open while adding
		}

		void OnMenuItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var parentListView = _cell.RealParent as ListView;
#pragma warning restore CS0618 // Type or member is obsolete
			var recycling = parentListView != null &&
				((parentListView.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0);
			if (recycling)
				Update(_tableView, _cell, ContentCell);
			else
				ReloadRow();
		}

		void ReloadRow()
		{
			if (_scroller.ContentOffset.X > 0)
			{
				((ContextScrollViewDelegate)_scroller.Delegate).ClosedCallback = () =>
				{
					ReloadRowCore();
					((ContextScrollViewDelegate)_scroller.Delegate).ClosedCallback = null;
				};

				_scroller.SetContentOffset(new PointF(0, 0), true);
			}
			else
				ReloadRowCore();
		}

		void ReloadRowCore()
		{
			if (_cell.RealParent == null)
				return;

			var path = Microsoft.Maui.Controls.Compatibility.Platform.iOS.CellExtensions.GetIndexPath(_cell);

			var selected = path.Equals(_tableView.IndexPathForSelectedRow);

			_tableView.ReloadRows(new[] { path }, UITableViewRowAnimation.None);

			if (selected)
			{
				_tableView.SelectRow(path, false, UITableViewScrollPosition.None);
				_tableView.Source.RowSelected(_tableView, path);
			}
		}

		UIView SetupButtons(nfloat width, nfloat height)
		{
			MenuItem destructive = null;
			nfloat largestWidth = 0, acceptableSize = width * 0.80f;

			for (var i = 0; i < _cell.ContextActions.Count; i++)
			{
				var item = _cell.ContextActions[i];

				if (_buttons.Count == 3)
				{
					if (destructive != null)
						break;
					if (!item.IsDestructive)
						continue;

					_buttons.RemoveAt(_buttons.Count - 1);
				}

				if (item.IsDestructive)
					destructive = item;

				var button = GetButton(item);
				button.Tag = i;
				var buttonWidth = button.TitleLabel.SizeThatFits(new SizeF(width, height)).Width + 30;
				if (buttonWidth > largestWidth)
					largestWidth = buttonWidth;

				if (destructive == item)
					_buttons.Insert(0, button);
				else
					_buttons.Add(button);
			}

			var needMore = _cell.ContextActions.Count > _buttons.Count;

			if (_cell.ContextActions.Count > 2)
				CullButtons(acceptableSize, ref needMore, ref largestWidth);

			var resize = false;
			if (needMore)
			{
				if (largestWidth * 2 > acceptableSize)
				{
					largestWidth = acceptableSize / 2;
					resize = true;
				}

				var button = new UIButton(new RectangleF(0, 0, largestWidth, height));
				button.SetBackgroundImage(NormalBackground, UIControlState.Normal);
#pragma warning disable CA1416, CA1422 // TODO: 'UIButton.TitleEdgeInsets' is unsupported on: 'ios' 15.0 and later
				button.TitleEdgeInsets = new UIEdgeInsets(0, 15, 0, 15);
#pragma warning restore CA1416, CA1422
				button.SetTitle(StringResources.More, UIControlState.Normal);

				var moreWidth = button.TitleLabel.SizeThatFits(new SizeF(width, height)).Width + 30;
				if (moreWidth > largestWidth)
				{
					largestWidth = moreWidth;
					CullButtons(acceptableSize, ref needMore, ref largestWidth);

					if (largestWidth * 2 > acceptableSize)
					{
						largestWidth = acceptableSize / 2;
						resize = true;
					}
				}

				button.Tag = -1;
				button.TouchUpInside += OnButtonActivated;
				if (resize)
					button.TitleLabel.AdjustsFontSizeToFitWidth = true;

				_moreButton = button;
				_buttons.Add(button);
			}

			var handler = new PropertyChangedEventHandler(OnMenuItemPropertyChanged);
			var totalWidth = _buttons.Count * largestWidth;
			for (var n = 0; n < _buttons.Count; n++)
			{
				var b = _buttons[n];

				if (b.Tag >= 0)
				{
					var item = _cell.ContextActions[(int)b.Tag];
					item.PropertyChanged += handler;
				}

				var offset = (n + 1) * largestWidth;
				var x = width - offset + totalWidth;
				b.Frame = new RectangleF(x, 0, largestWidth, height);
				if (resize)
					b.TitleLabel.AdjustsFontSizeToFitWidth = true;

				b.SetNeedsLayout();

				if (b != _moreButton)
					b.TouchUpInside += OnButtonActivated;
			}

			return null;
		}

		internal static void SetupSelection(UITableView table)
		{
			if (table.GestureRecognizers == null)
				return;

			for (var i = 0; i < table.GestureRecognizers.Length; i++)
			{
				var r = table.GestureRecognizers[i] as SelectGestureRecognizer;
				if (r != null)
					return;
			}

			table.AddGestureRecognizer(new SelectGestureRecognizer());
		}

		private sealed class SelectGestureRecognizer : UITapGestureRecognizer
		{
			NSIndexPath _lastPath;

			public SelectGestureRecognizer() : base(Tapped)
			{
				ShouldReceiveTouch = (recognizer, touch) =>
				{
					var table = (UITableView)View;
					var pos = touch.LocationInView(table);

					_lastPath = table.IndexPathForRowAtPoint(pos);
					if (_lastPath == null)
						return false;

					var cell = table.CellAt(_lastPath) as ContextActionsCell;

					return cell != null;
				};
			}

			static void Tapped(UIGestureRecognizer recognizer)
			{
				var selector = (SelectGestureRecognizer)recognizer;

				if (selector._lastPath == null)
					return;

				var table = (UITableView)recognizer.View;
				if (!selector._lastPath.Equals(table.IndexPathForSelectedRow))
					table.SelectRow(selector._lastPath, false, UITableViewScrollPosition.None);
				table.Source.RowSelected(table, selector._lastPath);
			}
		}

		private sealed class MoreActionSheetController : UIAlertController
		{
			public override UIAlertControllerStyle PreferredStyle => UIAlertControllerStyle.ActionSheet;

			public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
			{
				DismissViewController(false, null);
			}
		}
	}
}
