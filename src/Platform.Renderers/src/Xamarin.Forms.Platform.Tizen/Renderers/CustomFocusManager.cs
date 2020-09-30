using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	class CustomFocusManager : IDisposable
	{
		static bool s_reorderTriggered;
		static readonly ObservableCollection<CustomFocusManager> s_tabIndexList = new ObservableCollection<CustomFocusManager>();

		VisualElement _nextUp;
		VisualElement _nextDown;
		VisualElement _nextLeft;
		VisualElement _nextRight;
		VisualElement _nextForward;
		VisualElement _nextBackward;
		int _tabIndex = -1;
		bool _isTabStop = true;

		static CustomFocusManager()
		{
			s_tabIndexList.CollectionChanged += TabIndexCollectionChanged;
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

		public int TabIndex
		{
			get
			{
				return _tabIndex;
			}
			set
			{
				if (IsTabStop)
				{
					if (_tabIndex > -1)
					{
						s_tabIndexList.Remove(this);
					}
					if (value > -1)
					{
						s_tabIndexList.Add(this);
					}
				}
				_tabIndex = value;
			}
		}

		public bool IsTabStop
		{
			get
			{
				return _isTabStop;
			}
			set
			{
				if (TabIndex > -1)
				{
					if (_isTabStop)
					{
						s_tabIndexList.Remove(this);
					}
					if (value)
					{
						s_tabIndexList.Add(this);
					}
				}
				_isTabStop = value;
			}
		}

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

		public static void StartReorderTabIndex()
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() =>
				{
					StartReorderTabIndex();
				});
				return;
			}
			if (!s_reorderTriggered)
			{
				s_reorderTriggered = true;
				Device.BeginInvokeOnMainThread(ReorderTabIndex);
			}
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
			if (s_tabIndexList.Contains(this))
			{
				s_tabIndexList.Remove(this);
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

		static void ReorderTabIndex()
		{
			s_reorderTriggered = false;
			var list = s_tabIndexList.Where((t) => t.IsTabStop && ElementIsVisible(t)).OrderBy(x => x.Element.TabIndex);
			CustomFocusManager first = null;
			CustomFocusManager last = null;
			foreach (var item in list)
			{
				if (first == null)
					first = item;

				if (last != null)
				{
					item.NextBackward = last.Element;
					last.NextForward = item.Element;
				}
				last = item;
			}
			if (first != null && last != null)
			{
				first.NextBackward = last.Element;
				last.NextForward = first.Element;
			}
		}

		static void TabIndexCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			StartReorderTabIndex();
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
#pragma warning disable CS0618 // Type or member is obsolete
			if (parentPage is MasterDetailPage mdPage && mdPage.Master == parent && !mdPage.IsPresented)
#pragma warning restore CS0618 // Type or member is obsolete
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