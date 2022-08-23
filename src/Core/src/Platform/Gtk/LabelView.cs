using Cairo;
using Gtk;

namespace Microsoft.Maui.Platform
{

	public class LabelView : Label
	{

		public float LineHeight { get; set; }

		protected override bool OnDrawn(Context cr)
		{
			if (LineHeight > 1)
				Layout.LineSpacing = LineHeight;

			return base.OnDrawn(cr);
		}

	}

}