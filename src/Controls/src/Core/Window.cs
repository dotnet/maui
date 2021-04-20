using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public class Window : VisualElement, IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
		public string Title { get; set; }
	}
}
