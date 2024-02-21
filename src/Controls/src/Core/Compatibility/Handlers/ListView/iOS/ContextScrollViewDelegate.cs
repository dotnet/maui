#nullable disable
using System;
using System.Collections.Generic;
using ObjCRuntime;
using UIKit;
using NSAction = System.Action;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal sealed class iOS7ButtonContainer : UIView
	{
		readonly nfloat _buttonWidth;

		public iOS7ButtonContainer(nfloat buttonWidth) : base(new RectangleF(0, 0, 0, 0))
		{
			_buttonWidth = buttonWidth;
			ClipsToBounds = true;
		}

		public override void LayoutSubviews()
		{
			var width = Frame.Width;
			nfloat takenSpace = 0;

			for (var i = 0; i < Subviews.Length; i++)
			{
				var view = Subviews[i];

				var pos = Subviews.Length - i;
				var x = width - _buttonWidth * pos;
				view.Frame = new RectangleF(x, 0, view.Frame.Width, view.Frame.Height);

				takenSpace += view.Frame.Width;
			}
		}
	}

	internal sealed class ContextScrollViewDelegate : UIScrollViewDelegate
	{
		readonly nfloat _finalButtonSize;
		UIView _backgroundView;
		List<UIButton> _buttons;
		UITapGestureRecognizer _closer;
		UIView _container;
		Controls.Compatibility.Platform.iOS.GlobalCloseContextGestureRecognizer _globalCloser;

		bool _isDisposed;
		static WeakReference<UIScrollView> s_scrollViewBeingScrolled;
		UITableView _table;

		public ContextScrollViewDelegate(UIView container, List<UIButton> buttons, bool isOpen)
		{
			IsOpen = isOpen;
			_container = container;
			_buttons = buttons;

			for (var i = 0; i < buttons.Count; i++)
			{
				var b = buttons[i];
				b.Hidden = !isOpen;

				ButtonsWidth += b.Frame.Width;
				_finalButtonSize = b.Frame.Width;
			}
		}

		public nfloat ButtonsWidth { get; }

		public Action ClosedCallback { get; set; }

		public bool IsOpen { get; private set; }

		public override void DraggingStarted(UIScrollView scrollView)
		{
			if (ShouldIgnoreScrolling(scrollView))
				return;

			s_scrollViewBeingScrolled = new WeakReference<UIScrollView>(scrollView);

			if (!IsOpen)
				SetButtonsShowing(true);

			var cell = GetContextCell(scrollView);
			if (!cell.Selected)
				return;

			if (!IsOpen)
				RemoveHighlight(scrollView);
		}

		public void PrepareForDeselect(UIScrollView scrollView)
		{
			RestoreHighlight(scrollView);
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			if (ShouldIgnoreScrolling(scrollView))
				return;

			var width = _finalButtonSize;
			var count = _buttons.Count;
			var ioffset = scrollView.ContentOffset.X / (float)count;

			if (ioffset > width)
				width = ioffset + 1;

			for (var i = count - 1; i >= 0; i--)
			{
				var b = _buttons[i];
				var rect = b.Frame;
				b.Frame = new RectangleF(scrollView.Frame.Width + (count - (i + 1)) * ioffset, 0, width, rect.Height);
			}

			if (scrollView.ContentOffset.X == 0)
			{
				IsOpen = false;
				SetButtonsShowing(false);
				RestoreHighlight(scrollView);

				s_scrollViewBeingScrolled = null;
				ClearCloserRecognizer(GetContextCell(scrollView));
				ClosedCallback?.Invoke();
			}
		}

		public void Unhook(UIScrollView scrollView)
		{
			RestoreHighlight(scrollView);
			ClearCloserRecognizer(GetContextCell(scrollView));
		}

		public override void WillEndDragging(UIScrollView scrollView, PointF velocity, ref PointF targetContentOffset)
		{
			if (ShouldIgnoreScrolling(scrollView))
				return;

			var width = ButtonsWidth;
			var x = targetContentOffset.X;
			var parentThreshold = scrollView.Frame.Width * .4f;
			var contentThreshold = width * .8f;

			if (x >= parentThreshold || x >= contentThreshold)
			{
				IsOpen = true;
				targetContentOffset = new PointF(width, 0);
				RemoveHighlight(scrollView);

				if (_globalCloser == null)
				{
					UIView view = scrollView;
					while (view.Superview != null)
					{
						view = view.Superview;
						var table = view as UITableView;
						if (table != null)
						{
							ContextActionsCell contentCell = GetContextCell(scrollView);
							NSAction close = () =>
							{
								RestoreHighlight(scrollView);
								IsOpen = false;
								scrollView.SetContentOffset(new PointF(0, 0), true);
								ClearCloserRecognizer(contentCell);
								contentCell = null;
							};

							_table = table;
							_globalCloser = new Controls.Compatibility.Platform.iOS.GlobalCloseContextGestureRecognizer(scrollView, close);
							_globalCloser.ShouldRecognizeSimultaneously = (recognizer, r) => r == _table?.PanGestureRecognizer;
							table.AddGestureRecognizer(_globalCloser);

							_closer = new UITapGestureRecognizer(close);
							contentCell.ContentCell.AddGestureRecognizer(_closer);
						}
					}
				}
			}
			else
			{
				ClearCloserRecognizer(GetContextCell(scrollView));

				IsOpen = false;
				targetContentOffset = new PointF(0, 0);
				RestoreHighlight(scrollView);
			}
		}

		static bool ShouldIgnoreScrolling(UIScrollView scrollView)
		{
			if (s_scrollViewBeingScrolled == null)
				return false;

			UIScrollView scrollViewBeingScrolled;
			if (!s_scrollViewBeingScrolled.TryGetTarget(out scrollViewBeingScrolled)
				|| ReferenceEquals(scrollViewBeingScrolled, scrollView)
				|| !ReferenceEquals(((ContextScrollViewDelegate)scrollViewBeingScrolled.Delegate)?._table, ((ContextScrollViewDelegate)scrollView.Delegate)?._table))
				return false;

			scrollView.SetContentOffset(new PointF(0, 0), false);
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				ClosedCallback = null;

				s_scrollViewBeingScrolled = null;
				_table = null;
				_backgroundView = null;
				_container = null;

				_buttons = null;
			}

			base.Dispose(disposing);
		}

		void ClearCloserRecognizer(ContextActionsCell cell)
		{
			if (_globalCloser == null || _globalCloser.State == UIGestureRecognizerState.Cancelled)
				return;

			cell?.ContentCell?.RemoveGestureRecognizer(_closer);
			_closer.Dispose();
			_closer = null;

			_table.RemoveGestureRecognizer(_globalCloser);
			_table = null;
			_globalCloser.Dispose();
			_globalCloser = null;
		}

		ContextActionsCell GetContextCell(UIScrollView scrollView)
		{
			var view = scrollView?.Superview?.Superview;
			var cell = view as ContextActionsCell;
			while (view?.Superview != null)
			{
				cell = view as ContextActionsCell;
				if (cell != null)
					break;

				view = view.Superview;
			}

			return cell;
		}

		void RemoveHighlight(UIScrollView scrollView)
		{
			var subviews = scrollView.Superview.Superview.Subviews;

			var count = 0;
			for (var i = 0; i < subviews.Length; i++)
			{
				var s = subviews[i];
				if (s.Frame.Height > 1)
					count++;
			}

			if (count <= 1)
				return;

			_backgroundView = subviews[0];
			_backgroundView.RemoveFromSuperview();

			var cell = GetContextCell(scrollView);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
		}

		void RestoreHighlight(UIScrollView scrollView)
		{
			if (_backgroundView == null)
				return;

			var cell = GetContextCell(scrollView);
			cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
			cell.SetSelected(true, false);

			scrollView.Superview.Superview.InsertSubview(_backgroundView, 0);
			_backgroundView = null;
		}

		void SetButtonsShowing(bool show)
		{
			for (var i = 0; i < _buttons.Count; i++)
				_buttons[i].Hidden = !show;
		}
	}
}