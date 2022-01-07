using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface ISwipeView : IContentView
	{
		public double Threshold { get; }

		public ISwipeItems LeftItems { get; }

		public ISwipeItems RightItems { get; }

		public ISwipeItems TopItems { get; }

		public ISwipeItems BottomItems { get; }

		public bool IsOpen { get; set; }
	}
}
