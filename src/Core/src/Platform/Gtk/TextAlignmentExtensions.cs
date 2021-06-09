using Gtk;

namespace Microsoft.Maui
{
	public static class TextAlignmentExtensions
	{
		
		// https://developer.gnome.org/gtk3/stable/GtkWidget.html#GtkWidget--halign
		// How to distribute horizontal space if widget gets extra space, see GtkAlign
		
		internal static Align ToGtkAlign(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Start:
					return Align.Start;
				case TextAlignment.End:
					return Align.End;
				default:
					return Align.Center;
			}
		}
		
		public static Justification ToJustification(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Start:
					return Justification.Left;
				case TextAlignment.End:
					return Justification.Right;
				default:
					return Justification.Center;
			}
		}
		
		/// <summary>
		/// https://developer.gnome.org/gtk3/stable/GtkLabel.html#gtk-label-set-xalign
		/// The xalign property determines the horizontal aligment of the label text inside the labels size allocation.
		/// Compare this to “halign”, which determines how the labels size allocation is positioned in the space available for the label.
		/// </summary>
		/// <param name="alignment"></param>
		/// <returns></returns>
		public static float ToXyAlign(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Start:
					return 0f;
				case TextAlignment.End:
					return 1f;
				default:
					return 0.5f;
			}
		}

	}
}
