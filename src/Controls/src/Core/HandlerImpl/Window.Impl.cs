#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls
{
	struct MauiContextAware<T>
		where T : notnull
	{
		readonly Maui.IElement _element;

		T? _value;
		IMauiContext? _context;

		public MauiContextAware(Maui.IElement element)
			: this()
		{
			_element = element;
		}

		public T? Value
		{
			get
			{
				if (_context != _element?.Handler?.MauiContext)
				{
					if (_value is IDisposable disposable)
						disposable.Dispose();

					_value = default;
					_context = _element?.Handler?.MauiContext;
				}

				if (_value is null)
				{
					var services = _context?.Services;
					if (services is not null)
						_value ??= services.GetRequiredService<T>();
				}

				return _value;
			}
		}
	}

	public class Window : NavigableElement, IWindow
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(
			nameof(Title), typeof(string), typeof(Window), default(string?));

		public string? Title
		{
			get => (string?)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		ReadOnlyCollection<Element>? _logicalChildren;
		Page? _page;
		MauiContextAware<IAnimationManager> _animationManager;

		ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		public Window()
		{
			_animationManager = new MauiContextAware<IAnimationManager>(this);
			InternalChildren.CollectionChanged += OnCollectionChanged;
		}

		public Window(Page page)
			: this()
		{
			Page = page;
		}

		public IAnimationManager? AnimationManager => _animationManager.Value;

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
					InternalChildren.Remove(_page);

				_page = value;

				if (_page != null)
					InternalChildren.Add(_page);

				if (value is NavigableElement ne)
					ne.NavigationProxy.Inner = NavigationProxy;
			}
		}

		IView IWindow.View => Page ?? throw new InvalidOperationException("No page was set on the window.");
	}
}
