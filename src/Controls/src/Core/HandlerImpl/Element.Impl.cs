using System;

namespace Microsoft.Maui.Controls
{
	public partial class Element : Maui.IElement
	{
		IElementHandler _handler;
		EventHandler _attachedHandler;

		Maui.IElement Maui.IElement.Parent => Parent;

		public IElementHandler Handler
		{
			get => _handler;
			set => SetHandler(value);
		}

		public event EventHandler AttachingHandler;

		public event EventHandler AttachedHandler
		{
			add
			{
				_attachedHandler += value;
				if (Handler != null)
					value?.Invoke(this, EventArgs.Empty);
			}
			remove
			{
				_attachedHandler -= value;
			}
		}

		public event EventHandler DetachingHandler;

		public event EventHandler DetachedHandler;

		protected virtual void OnAttachingHandler() { }

		protected virtual void OnAttachedHandler() { }

		protected virtual void OnDetachingHandler() { }

		protected virtual void OnDetachedHandler() { }

		private protected virtual void OnAttachedHandlerCore() => OnAttachedHandler();

		private protected virtual void OnDetachingHandlerCore() => OnDetachingHandler();

		private protected virtual void OnHandlerSet() { }

		void SetHandler(IElementHandler newHandler)
		{
			if (newHandler == _handler)
				return;

			var previousHandler = _handler;

			if (_handler != null)
			{
				DetachingHandler?.Invoke(this, EventArgs.Empty);
				OnDetachingHandlerCore();
			}

			if (newHandler != null)
			{
				AttachingHandler?.Invoke(this, EventArgs.Empty);
				OnAttachingHandler();
			}

			_handler = newHandler;

			if (_handler?.VirtualView != this)
				_handler?.SetVirtualView(this);

			OnHandlerSet();

			if (_handler != null)
			{
				_attachedHandler?.Invoke(this, EventArgs.Empty);
				OnAttachedHandlerCore();
			}

			if (previousHandler != null)
			{
				DetachedHandler?.Invoke(this, EventArgs.Empty);
				OnDetachedHandler();
			}
		}
	}
}