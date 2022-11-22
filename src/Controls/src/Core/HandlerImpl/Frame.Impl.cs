using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Frame.xml" path="Type[@FullName='Microsoft.Maui.Controls.Frame']/Docs/*" />
	public partial class Frame : IView
	{
		IShadow IView.Shadow
		{
			get
			{
				if (!HasShadow)
					return null;

				if (base.Shadow != null)
					return base.Shadow;

#if IOS
				// The way the shadow is applied in .NET MAUI on iOS is the same way it was applied in Forms
				// so on iOS we just return the shadow that was hard coded into the renderer
				// On Android it sets the elevation on the CardView and on WinUI Forms just ignored HasShadow
				if(HasShadow)
					return new Shadow() { Radius = 5, Opacity = 0.8f, Offset = new Point(0, 0), Brush = Brush.Black };
#endif

				return null;
			}
		}
	}
}
