using System;

namespace Microsoft.Maui.Controls
{
	public partial class Element : Maui.IElement
	{
		IElementHandler _handler;

		Maui.IElement Maui.IElement.Parent => Parent;

		public IElementHandler Handler
		{
			get => _handler;
			set => SetHandler(value);
		}

		public virtual void OnAttachingHandler() { }

		public virtual void OnAttachedHandler() { }

		public virtual void OnDetachingHandler() { }

		public virtual void OnDetachedHandler() { }

		public event EventHandler AttachingHandler;

		public event EventHandler AttachedHandler;

		public event EventHandler DetachingHandler;

		public event EventHandler DetachedHandler;

		void SetHandler(IElementHandler newHandler)
		{
			if (newHandler == _handler)
				return;

			var previousHandler = _handler;

			if (_handler != null)
			{
				DetachingHandler?.Invoke(this, EventArgs.Empty);
				OnDetachingHandler();
			}

			if (newHandler != null)
			{
				AttachingHandler?.Invoke(this, EventArgs.Empty);
				OnAttachingHandler();
			}

			_handler = newHandler;

			if (_handler?.VirtualView != this)
				_handler?.SetVirtualView(this);

			if (this is VisualElement ve)
				ve.IsPlatformEnabled = Handler != null;

			if (_handler != null)
			{
				AttachedHandler?.Invoke(this, EventArgs.Empty);
				OnAttachedHandler();
			}

			if (previousHandler != null)
			{
				DetachedHandler?.Invoke(this, EventArgs.Empty);
				OnDetachedHandler();
			}
		}
	}
}