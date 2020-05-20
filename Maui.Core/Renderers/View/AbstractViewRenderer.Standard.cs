using System;
using System.Maui.Core;

namespace System.Maui.Platform
{
	public abstract partial class AbstractViewRenderer<TVirtualView, TNativeView>
	{
		public void SetFrame(Rectangle rect)
		{

		}
		public virtual SizeRequest GetDesiredSize (double widthConstraint, double heightConstraint)
			=> new SizeRequest ();

		void SetupContainer () { }
		void RemoveContainer () { }
	}
}
