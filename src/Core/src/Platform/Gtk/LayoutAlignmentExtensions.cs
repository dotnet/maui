using Gtk;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{

	public static class LayoutAlignmentExtensions
	{

		public static Align ToPlatform(this LayoutAlignment alignment) =>
			alignment switch
			{
				LayoutAlignment.Start => Align.Start,
				LayoutAlignment.End => Align.End,
				LayoutAlignment.Fill => Align.Fill,
				_ => Align.Center
			};

		public static LayoutAlignment ToMaui(this Align alignment) =>
			alignment switch
			{
				Align.Start => LayoutAlignment.Start,
				Align.End => LayoutAlignment.End,
				Align.Fill => LayoutAlignment.Fill,
				_ => LayoutAlignment.Center
			};
	}

}