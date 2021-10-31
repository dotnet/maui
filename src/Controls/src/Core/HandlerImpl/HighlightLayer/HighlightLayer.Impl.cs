#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
#if IOS || ANDROID || WINDOWS
	public partial class HighlightLayer : IHighlightLayer
	{
		public IWindow Window { get; }

		public HighlightLayer(IWindow window)
		{
			this.Window = window;
		}
	}
#else
	public partial class HighlightLayer : IHighlightLayer
	{
		public IWindow Window { get; }

		public HighlightLayer(IWindow window)
		{
			this.Window = window;
		}

		public bool AddHighlight(Maui.IView view)
		{
			throw new NotImplementedException();
		}

		public bool RemoveHighlight(Maui.IView view)
		{
			throw new NotImplementedException();
		}

		public void ClearHighlights()
		{
			throw new NotImplementedException();
		}
	}
#endif
}
