using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAnimation;

namespace Xamarin.Forms.Platform.iOS

{
	public class FormsCAKeyFrameAnimation : CAKeyFrameAnimation
	{
		public int Width { get; set; }

		public int Height { get; set; }
	}
}