using System;
using Cairo;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{

	/// <summary>
	/// dummy widget to mark handler as not implemented
	/// but avoid <see cref="NotImplementedException"/>
	/// </summary>
	public class NotImplementedView : Gtk.EventBox
	{

		protected NotImplementedView() { }

		public NotImplementedView(string name):this()
		{
			DisplayName = name;

			Drawn += (o, args) =>
			{
				var cr = args.Cr;

				if (DisplayName is not { })
					return;

				var stc = this.StyleContext;
				stc.RenderBackground(cr, 0, 0, Allocation.Width, Allocation.Height);

				var r = base.OnDrawn(cr);

				cr.Save();
				cr.SetSourceColor(Graphics.Colors.Red.ToCairoColor());
				cr.Rectangle(0, 0, Allocation.Width, Allocation.Height);
				cr.Stroke();

				cr.MoveTo(0, Allocation.Height - 12);
				cr.ShowText(DisplayName);
				cr.Restore();
			};
		}

		public string? DisplayName { get; }



	}

}