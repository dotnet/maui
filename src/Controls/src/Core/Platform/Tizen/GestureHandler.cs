using System;
using NGestureDetector = Tizen.NUI.GestureDetector;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public abstract class GestureHandler : IDisposable, IRegisterable
	{
		bool _disposedValue;

		IViewHandler? _handler;
		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;
		protected View? View => Element as View;
		protected NView? NativeView => _handler?.ToPlatform();


		public IGestureRecognizer Recognizer { get; private set; }

		public NGestureDetector NativeDetector { get; }


		public void Attach(IViewHandler handler)
		{
			_handler = handler;
			NativeDetector.Attach(NativeView);
		}

		public void Detach()
		{
			NativeDetector.Detach(NativeView);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected abstract NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer);

		protected GestureHandler(IGestureRecognizer recognizer)
		{
			Recognizer = recognizer;
			NativeDetector = CreateNativeDetector(recognizer);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					NativeDetector.Dispose();
				}
				_disposedValue = true;
			}
		}
	}
}