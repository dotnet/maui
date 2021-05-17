using System;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public class PageView : Page
	{
		public PageView(ElmSharp.EvasObject parent) : base(parent)
		{
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}
}
