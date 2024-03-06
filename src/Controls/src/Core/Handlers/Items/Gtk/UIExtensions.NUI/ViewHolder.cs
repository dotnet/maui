using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Gtk.UIExtensions.NUI
{
	public enum ViewHolderState
	{
		Normal,
		Selected,
		Focused,
	}

	public class ViewHolder : Gtk.EventBox
	{
		ViewHolderState _state;
		bool _isSelected;
		bool _isFocused;

		Widget? _content;

		public ViewHolder()
		{
			Initialize();
		}

		public object? ViewCategory { get; set; }

		public Rect Bounds { get; protected set; }

		public void UpdateBounds(Rect bounds)
		{
			Bounds = bounds;
		}

		public Widget? Content
		{
			get
			{
				return _content;
			}
			set
			{
				if (_content != null)
				{
					_content.CanFocus = false;
					_content.FocusOnClick = false;
					_content.FocusInEvent -= OnContentFocused;
					_content.FocusOutEvent -= OnContentUnfocused;
					
					Remove(_content);
				}

				_content = value;

				if (_content != null)
				{
					_content.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
					_content.CanFocus = true;
					_content.FocusOnClick = true;
					_content.WidthSpecification(LayoutParamPolicies.MatchParent);
					_content.HeightSpecification(LayoutParamPolicies.MatchParent);
					_content.WidthResizePolicy(ResizePolicyType.FillToParent);
					_content.HeightResizePolicy(ResizePolicyType.FillToParent);

					// _content.FocusInEvent += OnContentFocused;
					_content.FocusOutEvent += OnContentUnfocused;
					_content.ButtonPressEvent+=OnContentOnButtonPressEvent;
					// _content.FocusGrabbed += OnContentFocused;
					Child = _content;
				}
			}
		}

		void OnContentOnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			State = ViewHolderState.Focused;
		}

		public new ViewHolderState State
		{
			get { return _state; }
			set
			{
				if (value == ViewHolderState.Normal)
					_isSelected = false;
				else if (value == ViewHolderState.Selected)
					_isSelected = true;

				_state = _isFocused ? ViewHolderState.Focused : (_isSelected ? ViewHolderState.Selected : ViewHolderState.Normal);

				UpdateState();
			}
		}

		public event EventHandler? RequestSelected;

		public event EventHandler? StateUpdated;

		public void UpdateSelected()
		{
			State = ViewHolderState.Selected;
		}

		public void ResetState()
		{
			State = ViewHolderState.Normal;
		}

		protected void Initialize()
		{
			CanFocus = true;

			this.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
			this.AddEvents ((int)Gdk.EventMask.FocusChangeMask);
			TouchEvent += OnTouchEvent;
			KeyPressEvent += OnKeyEvent;
			FocusGrabbed += OnFocused;
			FocusOutEvent += OnUnfocused;
			ButtonPressEvent+=OnButtonPressEvent;
			// no need for that:
			//SizeAllocated += OnLayout;
		}

		void OnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			RequestSelected?.Invoke(this, EventArgs.Empty);
		}

		void OnLayout(object? sender, SizeAllocatedArgs e)
		{
			var bounds = e.Allocation.ToRect();
			bounds.X = 0;
			bounds.Y = 0;
			foreach (var child in Children)
			{
				child.UpdateBounds(bounds);
			}
		}

		void OnUnfocused(object? sender, EventArgs e)
		{
			_isFocused = false;
			State = _isSelected ? ViewHolderState.Selected : ViewHolderState.Normal;
		}

		void OnFocused(object? sender, EventArgs e)
		{
			_isFocused = true;
			State = ViewHolderState.Focused;
		}

		void OnContentUnfocused(object? sender, EventArgs e)
		{
			OnUnfocused(this, e);
		}

		void OnContentFocused(object? sender, EventArgs e)
		{
			OnFocused(this, e);
		}

		void OnKeyEvent(object source, KeyPressEventArgs e)
		{
			if (e.Event.SendEvent)
			{
				RequestSelected?.Invoke(this, EventArgs.Empty);
				//return true;
			}

			// return false;
		}

		void OnTouchEvent(object source, TouchEventArgs e)
		{
			// if (e.Touch.GetState(0) == PointStateType.Down)
			// {
			//     return true;
			// }
			// else if (e.Touch.GetState(0) == PointStateType.Up && this.IsInside(e.Touch.GetLocalPosition(0)))
			// {
			//     RequestSelected?.Invoke(this, EventArgs.Empty);
			//     return true;
			// }
			// return false;
		}

		protected virtual void UpdateState()
		{
			if (State == ViewHolderState.Selected)
				_isSelected = true;
			else if (State == ViewHolderState.Normal)
				_isSelected = false;
			else if (State == ViewHolderState.Focused)
				this.RaiseToTop();

			StateUpdated?.Invoke(this, EventArgs.Empty);
		}
	}
}