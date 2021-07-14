#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Page))]
	public partial class Window : NavigableElement, IWindow
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(
			nameof(Title), typeof(string), typeof(Window), default(string?));

		public static readonly BindableProperty PageProperty = BindableProperty.Create(
			nameof(Page), typeof(Page), typeof(Window), default(Page?),
			propertyChanged: OnPageChanged);

		ReadOnlyCollection<Element>? _logicalChildren;

		public Window()
		{
			AlertManager = new AlertManager(this);
			ModalNavigationManager = new ModalNavigationManager(this);
			Navigation = new NavigationImpl(this);

			InternalChildren.CollectionChanged += OnCollectionChanged;
		}

		public Window(Page page)
			: this()
		{
			Page = page;
		}

		public string? Title
		{
			get => (string?)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public Page? Page
		{
			get => (Page?)GetValue(PageProperty);
			set => SetValue(PageProperty, value);
		}

		public event EventHandler<ModalPoppedEventArgs>? ModalPopped;

		public event EventHandler<ModalPoppingEventArgs>? ModalPopping;

		public event EventHandler<ModalPushedEventArgs>? ModalPushed;

		public event EventHandler<ModalPushingEventArgs>? ModalPushing;

		public event EventHandler? PopCanceled;

		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(Page))
				Handler?.UpdateValue(nameof(IWindow.Content));
		}

		internal ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		internal AlertManager AlertManager { get; }

		internal ModalNavigationManager ModalNavigationManager { get; }

		internal IMauiContext MauiContext =>
			Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext is null.");

		IView IWindow.Content =>
			Page ?? throw new InvalidOperationException("No page was set on the window.");

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

		static void OnPageChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not Window window)
				return;

			var oldPage = oldValue as Page;
			if (oldPage != null)
			{
				window.InternalChildren.Remove(oldPage);
				oldPage.AttachedHandler -= OnPageAttachedHandler;
				oldPage.DetachedHandler -= OnPageDetachedHandler;
			}

			var newPage = newValue as Page;
			if (newPage != null)
			{
				window.InternalChildren.Add(newPage);
				newPage.NavigationProxy.Inner = window.NavigationProxy;
			}

			window.ModalNavigationManager.SettingNewPage();

			if (newPage != null)
			{
				newPage.AttachedHandler += OnPageAttachedHandler;
				newPage.DetachedHandler += OnPageDetachedHandler;
			}

			void OnPageAttachedHandler(object? sender, EventArgs e)
			{
				window.ModalNavigationManager.PageAttachedHandler();
				window.AlertManager.Subscribe();
			}

			void OnPageDetachedHandler(object? sender, EventArgs e)
			{
				window.AlertManager.Unsubscribe();
			}
		}

		void SendWindowAppearing()
		{
			Page?.SendAppearing();
		}

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
				return _owner.ModalNavigationManager.ModalStack;
			}

			protected override async Task<Page?> OnPopModal(bool animated)
			{
				Page modal = _owner.ModalNavigationManager.ModalStack[_owner.ModalNavigationManager.ModalStack.Count - 1];
				if (_owner.OnModalPopping(modal))
				{
					_owner.OnPopCanceled();
					return null;
				}

				Page result = await _owner.ModalNavigationManager.PopModalAsync(animated);
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
					await _owner.ModalNavigationManager.PushModalAsync(modal, animated);
				}
				else
				{
					await _owner.ModalNavigationManager.PushModalAsync(modal, animated);
					modal.NavigationProxy.Inner = this;
				}

				_owner.OnModalPushed(modal);
			}
		}
	}
}