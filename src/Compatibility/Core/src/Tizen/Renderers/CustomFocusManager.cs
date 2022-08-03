using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[Obsolete]
	class CustomFocusManager : IDisposable
	{
		VisualElement _nextUp;
		VisualElement _nextDown;
		VisualElement _nextLeft;
		VisualElement _nextRight;
		VisualElement _nextForward;
		VisualElement _nextBackward;

		static CustomFocusManager()
		{
		}

		public CustomFocusManager(VisualElement element, Widget nativeView)
		{
			Element = element;
			NativeView = nativeView;
		}

		~CustomFocusManager()
		{
			Dispose(false);
		}

		VisualElement Element { get; }
		Widget NativeView { get; }

		public VisualElement NextUp
		{
			get => _nextUp;
			set
			{
				_nextUp = value;
				SetUpFocus(value, FocusDirection.Up);
			}
		}

		public VisualElement NextDown
		{
			get => _nextDown;
			set
			{
				_nextDown = value;
				SetUpFocus(value, FocusDirection.Down);
			}
		}

		public VisualElement NextLeft
		{
			get => _nextLeft;
			set
			{
				_nextLeft = value;
				SetUpFocus(value, FocusDirection.Left);
			}
		}

		public VisualElement NextRight
		{
			get => _nextRight;
			set
			{
				_nextRight = value;
				SetUpFocus(value, FocusDirection.Right);
			}
		}

		public VisualElement NextForward
		{
			get => _nextForward;
			set
			{
				_nextForward = value;
				SetUpFocus(value, FocusDirection.Next);
			}
		}

		public VisualElement NextBackward
		{
			get => _nextBackward;
			set
			{
				_nextBackward = value;
				SetUpFocus(value, FocusDirection.Previous);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (NextUp != null)
				{
					NextUp.PropertyChanged -= OnNextViewPropertyChanged;
				}
				if (NextDown != null)
				{
					NextDown.PropertyChanged -= OnNextViewPropertyChanged;
				}
				if (NextLeft != null)
				{
					NextLeft.PropertyChanged -= OnNextViewPropertyChanged;
				}
				if (NextRight != null)
				{
					NextRight.PropertyChanged -= OnNextViewPropertyChanged;
				}
				if (NextForward != null)
				{
					NextForward.PropertyChanged -= OnNextViewPropertyChanged;
				}
				if (NextBackward != null)
				{
					NextBackward.PropertyChanged -= OnNextViewPropertyChanged;
				}
			}
		}

		void SetUpFocus(VisualElement next, FocusDirection direction)
		{
			SetNativeCustomFocus(next, direction);
			if (next != null && !next.IsPlatformEnabled)
			{
				next.PropertyChanged += OnNextViewPropertyChanged;
			}
		}

		void SetNativeCustomFocus(VisualElement next, FocusDirection direction)
		{
			if (next != null)
			{
				var nextView = Platform.GetRenderer(next)?.NativeView ?? null;
				if (nextView != null)
				{
					//FixMe: Widget.SetNextFocusObject is not allow null of nextView due to bug, so we can't reset custom focus to default behavior
					NativeView?.SetNextFocusObject(nextView, direction);
				}
			}
		}

		void OnNextViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Renderer" && sender is VisualElement next)
			{
				SetNativeCustomFocus(next, GetFocusDirection(next));
			}
		}

		FocusDirection GetFocusDirection(VisualElement nextView)
		{
			if (nextView == NextUp)
				return FocusDirection.Up;
			if (nextView == NextDown)
				return FocusDirection.Down;
			if (nextView == NextLeft)
				return FocusDirection.Left;
			if (nextView == NextRight)
				return FocusDirection.Right;
			if (nextView == NextForward)
				return FocusDirection.Next;
			if (nextView == NextBackward)
				return FocusDirection.Previous;
			return FocusDirection.Next;
		}

		static bool PageIsVisible(Page page)
		{
			if (page == null)
			{
				return false;
			}

			var parent = page.Parent;
			var parentPage = page.Parent as Page;

			if (parent == Application.Current && page.Navigation.ModalStack.Count == 0)
			{
				return true;
			}

			if (parentPage == null && page.Navigation.ModalStack.LastOrDefault() == page)
			{
				return true;
			}

			if (parentPage is IPageContainer<Page> container && container.CurrentPage != page)
			{
				return false;
			}
			if (parentPage is FlyoutPage flyoutPage && flyoutPage.Flyout == parent && !flyoutPage.IsPresented)
			{
				return false;
			}
			return PageIsVisible(parentPage);
		}

		static bool ElementIsVisible(CustomFocusManager manager)
		{
			Element parent = manager.Element;
			while (parent != null && !(parent is Page))
			{
				parent = parent.Parent;
			}
			var ret = PageIsVisible(parent as Page);
			return ret;
		}
	}
}