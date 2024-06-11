using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Controls
{
	static class FlexExtensions
	{
		public static Rect GetFrame(this Flex.Item item)
		{
			return new Rect(item.Frame[0], item.Frame[1], item.Frame[2], item.Frame[3]);
		}

		public static Size GetConstraints(this Flex.Item item)
		{
			var widthConstraint = -1d;
			var heightConstraint = -1d;
			var parent = item.Parent;
			do
			{
				if (parent == null)
					break;
				if (widthConstraint < 0 && !float.IsNaN(parent.Width))
					widthConstraint = (double)parent.Width;
				if (heightConstraint < 0 && !float.IsNaN(parent.Height))
					heightConstraint = (double)parent.Height;
				parent = parent.Parent;
			} while (widthConstraint < 0 || heightConstraint < 0);
			return new Size(widthConstraint, heightConstraint);
		}

		public static Flex.Basis ToFlexBasis(this FlexBasis basis)
		{
			if (basis.IsAuto)
				return Flex.Basis.Auto;
			if (basis.IsRelative)
				return new Flex.Basis(basis.Length, isRelative: true);
			return new Flex.Basis(basis.Length, isRelative: false);
		}
	}
}
