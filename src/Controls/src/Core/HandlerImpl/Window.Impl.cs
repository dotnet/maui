#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Window : VisualElement, IWindow
	{
		ReadOnlyCollection<Element>? _logicalChildren;
		Page? _page;

		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		internal IMauiContext MauiContext => Page?.Handler?.MauiContext
			?? throw new InvalidOperationException("MauiContext is null");

		internal ModalNavigationService ModalNavigationService { get; }

		public Window()
		{
			ModalNavigationService = new ModalNavigationService(this);
			Navigation = new NavigationImpl(this);
			InternalChildren.CollectionChanged += OnCollectionChanged;
		}

		public Window(Page page)
			: this()
		{
			Page = page;
		}


		void SendWindowAppearing()
		{
			Page?.SendAppearing();
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var item = (Element?)e.OldItems[i];
					OnChildRemoved(item, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				foreach (Element item in e.NewItems)
				{
					OnChildAdded(item);

					// TODO once we have better life cycle events on pages 
					if (item is Page)
					{
						SendWindowAppearing();
					}
				}
			}
		}

		public Page? Page
		{
			get => _page;
			set
			{
				if (_page != null)
				{
					InternalChildren.Remove(_page);

					if (_page != null)
						_page.AttachedHandler -= OnPageAttachedHandler;
				}

				_page = value;

				if (_page != null)
					InternalChildren.Add(_page);

				if (value is NavigableElement ne)
					ne.NavigationProxy.Inner = NavigationProxy;

				ModalNavigationService.SettingNewPage();

				if (value != null)
					value.AttachedHandler += OnPageAttachedHandler;
			}
		}

		void OnPageAttachedHandler(object? sender, EventArgs e)
		{
			ModalNavigationService.PageAttachedHandler();
		}

		IView IWindow.View
		{
			get => Page ?? throw new InvalidOperationException("No page was set on the window.");
			set => Page = (Page)value;
		}


		public event EventHandler<ModalPoppedEventArgs>? ModalPopped;

		public event EventHandler<ModalPoppingEventArgs>? ModalPopping;

		public event EventHandler<ModalPushedEventArgs>? ModalPushed;

		public event EventHandler<ModalPushingEventArgs>? ModalPushing;

		public event EventHandler? PopCanceled;

		void OnModalPopped(Page modalPage)
		{
			var args = new ModalPoppedEventArgs(modalPage);
			ModalPopped?.Invoke(this, args);
			(Parent as Application)?.NotifyOfWindowModalEvent(args);
		}

		bool OnModalPopping(Page modalPage)
		{
			var args = new ModalPoppingEventArgs(modalPage);
			ModalPopping?.Invoke(this, args);
			(Parent as Application)?.NotifyOfWindowModalEvent(args);
			return args.Cancel;
		}

		void OnModalPushed(Page modalPage)
		{
			var args = new ModalPushedEventArgs(modalPage);
			ModalPushed?.Invoke(this, args);
			(Parent as Application)?.NotifyOfWindowModalEvent(args);
		}

		void OnModalPushing(Page modalPage)
		{
			var args = new ModalPushingEventArgs(modalPage);
			ModalPushing?.Invoke(this, args);
			(Parent as Application)?.NotifyOfWindowModalEvent(args);
		}


		void OnPopCanceled()
		{
			PopCanceled?.Invoke(this, EventArgs.Empty);
		}

		class NavigationImpl : NavigationProxy
		{
			readonly Window _owner;

			public NavigationImpl(Window owner)
			{
				_owner = owner;
			}

			protected override IReadOnlyList<Page> GetModalStack()
			{
				return _owner.ModalNavigationService.ModalStack;
			}

			protected override async Task<Page?> OnPopModal(bool animated)
			{
				Page modal = _owner.ModalNavigationService.ModalStack[_owner.ModalNavigationService.ModalStack.Count - 1];
				if (_owner.OnModalPopping(modal))
				{
					_owner.OnPopCanceled();
					return null;
				}

				Page result = await _owner.ModalNavigationService.PopModalAsync(animated);
				result.Parent = null;
				_owner.OnModalPopped(result);
				return result;
			}

			protected override async Task OnPushModal(Page modal, bool animated)
			{
				_owner.OnModalPushing(modal);

				modal.Parent = _owner;

				if (modal.NavigationProxy.ModalStack.Count == 0)
				{
					modal.NavigationProxy.Inner = this;
					await _owner.ModalNavigationService.PushModalAsync(modal, animated);
				}
				else
				{
					await _owner.ModalNavigationService.PushModalAsync(modal, animated);
					modal.NavigationProxy.Inner = this;
				}

				_owner.OnModalPushed(modal);
			}
		}
	}
}
