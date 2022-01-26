using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Editor.xml" path="Type[@FullName='Microsoft.Maui.Controls.Editor']/Docs" />
	public partial class Editor : IEditor
	{
		double _previousWidthConstraint;
		double _previousHeightConstraint;
		Rectangle _previousBounds;

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		void IEditor.Completed()
		{
			(this as IEditorController).SendCompleted();
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			_previousBounds = bounds;
			return base.ArrangeOverride(bounds);
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (AutoSize == EditorAutoSizeOption.Disabled &&
				Width > 0 &&
				Height > 0)
			{
				if (TheSame(_previousHeightConstraint, heightConstraint) &&
					TheSame(_previousWidthConstraint, widthConstraint))
				{
					return new Size(Width, Height);
				}
				else if (TheSame(_previousHeightConstraint, _previousBounds.Height) &&
					TheSame(_previousWidthConstraint, _previousBounds.Width))
				{
					return new Size(Width, Height);
				}
			}

			_previousWidthConstraint = widthConstraint;
			_previousHeightConstraint = heightConstraint;
			return base.MeasureOverride(widthConstraint, heightConstraint);

			bool TheSame(double width, double otherWidth)
			{
				return width == otherWidth ||
					(width - otherWidth) < double.Epsilon;
			}
		}
	}
}