using System;
using System.Collections.Specialized;
using System.ComponentModel;

// NOTE: warning disabled for netstandard projects
#pragma warning disable 0436
using MaybeNullWhenAttribute = System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute;
#pragma warning restore 0436

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An abstract base class for subscribing to an event via WeakReference.
	/// See WeakNotifyCollectionChangedProxy below for sublcass usage.
	/// </summary>
	/// <typeparam name="TSource">The object type that has the event</typeparam>
	/// <typeparam name="TEventHandler">The event handler type of the event</typeparam>
	abstract class WeakEventProxy<TSource, TEventHandler>
		where TSource : class
		where TEventHandler : Delegate
	{
		WeakReference<TSource>? _source;
		WeakReference<TEventHandler>? _handler;

		public bool TryGetSource([MaybeNullWhen(false)] out TSource source)
		{
			if (_source is not null && _source.TryGetTarget(out source))
			{
				return source is not null;
			}

			source = default;
			return false;
		}

		public bool TryGetHandler([MaybeNullWhen(false)] out TEventHandler handler)
		{
			if (_handler is not null && _handler.TryGetTarget(out handler))
			{
				return handler is not null;
			}

			handler = default;
			return false;
		}

		public virtual void Subscribe(TSource source, TEventHandler handler)
		{
			_source = new WeakReference<TSource>(source);
			_handler = new WeakReference<TEventHandler>(handler);
		}

		public virtual void Unsubscribe()
		{
			_source = null;
			_handler = null;
		}
	}

	/// <summary>
	/// A "proxy" class for subscribing INotifyCollectionChanged via WeakReference.
	/// General usage is to store this in a member variable and call Subscribe()/Unsubscribe() appropriately.
	/// Your class should have a finalizer that calls Unsubscribe() to prevent WeakNotifyCollectionChangedProxy objects from leaking.
	/// </summary>
	class WeakNotifyCollectionChangedProxy : WeakEventProxy<INotifyCollectionChanged, NotifyCollectionChangedEventHandler>
	{
		public WeakNotifyCollectionChangedProxy() { }

		public WeakNotifyCollectionChangedProxy(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
		{
			Subscribe(source, handler);
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (TryGetHandler(out var handler))
			{
				handler(sender, e);
			}
			else
			{
				Unsubscribe();
			}
		}

		public override void Subscribe(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
		{
			if (TryGetSource(out var s))
			{
				s.CollectionChanged -= OnCollectionChanged;
			}

			source.CollectionChanged += OnCollectionChanged;
			base.Subscribe(source, handler);
		}

		public override void Unsubscribe()
		{
			if (TryGetSource(out var s))
			{
				s.CollectionChanged -= OnCollectionChanged;
			}
			base.Unsubscribe();
		}
	}

	/// <summary>
	/// A "proxy" class for subscribing INotifyPropertyChanged via WeakReference.
	/// General usage is to store this in a member variable and call Subscribe()/Unsubscribe() appropriately.
	/// Your class should have a finalizer that calls Unsubscribe() to prevent WeakNotifyPropertyChangedProxy objects from leaking.
	/// </summary>
	class WeakNotifyPropertyChangedProxy : WeakEventProxy<INotifyPropertyChanged, PropertyChangedEventHandler>
	{
		public WeakNotifyPropertyChangedProxy() { }

		public WeakNotifyPropertyChangedProxy(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
		{
			Subscribe(source, handler);
		}

		void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (TryGetHandler(out var handler))
			{
				handler(sender, e);
			}
			else
			{
				Unsubscribe();
			}
		}

		public override void Subscribe(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
		{
			if (TryGetSource(out var s))
			{
				s.PropertyChanged -= OnPropertyChanged;
			}

			source.PropertyChanged += OnPropertyChanged;

			base.Subscribe(source, handler);
		}

		public override void Unsubscribe()
		{
			if (TryGetSource(out var s))
			{
				s.PropertyChanged -= OnPropertyChanged;
			}

			base.Unsubscribe();
		}
	}

	class WeakBrushChangedProxy : WeakEventProxy<Brush, EventHandler>
	{
		void OnBrushChanged(object? sender, EventArgs e)
		{
			if (TryGetHandler(out var handler))
			{
				handler(sender, e);
			}
			else
			{
				Unsubscribe();
			}
		}

		public override void Subscribe(Brush source, EventHandler handler)
		{
			if (TryGetSource(out var s))
			{
				s.PropertyChanged -= OnBrushChanged;

				if (s is GradientBrush g)
					g.InvalidateGradientBrushRequested -= OnBrushChanged;
			}

			source.PropertyChanged += OnBrushChanged;
			if (source is GradientBrush gradientBrush)
				gradientBrush.InvalidateGradientBrushRequested += OnBrushChanged;

			base.Subscribe(source, handler);
		}

		public override void Unsubscribe()
		{
			if (TryGetSource(out var s))
			{
				s.PropertyChanged -= OnBrushChanged;

				if (s is GradientBrush g)
					g.InvalidateGradientBrushRequested -= OnBrushChanged;
			}
			base.Unsubscribe();
		}
	}
}
