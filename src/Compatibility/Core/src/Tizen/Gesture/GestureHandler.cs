using System;
using NGestureDetector = Tizen.NUI.GestureDetector;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public abstract class GestureHandler : IRegisterable, IDisposable
	{
		bool _disposedValue;

		public IGestureRecognizer Recognizer { get; private set; }

		public NGestureDetector NativeDetector { get; }

		protected IVisualElementRenderer Renderer { get; private set; }
		protected View View => Renderer.Element as View;

		public void Attach(IVisualElementRenderer renderer)
		{
			Renderer = renderer;
			NativeDetector.Attach(Renderer.NativeView);
		}

		public void Detach()
		{
			NativeDetector.Detach(Renderer.NativeView);
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