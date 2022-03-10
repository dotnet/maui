using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/RefreshView.xml" path="Type[@FullName='Microsoft.Maui.Controls.RefreshView']/Docs" />
	public partial class RefreshView : IRefreshView
	{
		Paint IRefreshView.RefreshColor => RefreshColor?.AsPaint();

		IView IRefreshView.Content => base.Content;

		// This shouldn't need be needed once TemplatedView inherits from Controls.Layout
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (Content == null)
				return new Size(100, 100);

			var margin = Margin;

			// Adjust the constraints to account for the margins
			widthConstraint -= margin.HorizontalThickness;
			heightConstraint -= margin.VerticalThickness;

			// Use the old measurement override to figure out the xplat size
			var measure = OnMeasure(widthConstraint, heightConstraint).Request;

			// Make sure the native control gets measured
			var nativeMeasure = Handler?.GetDesiredSize(measure.Width, measure.Height);

			// Account for the margins when reporting the desired size value
			DesiredSize = new Size(measure.Width + margin.HorizontalThickness,
				measure.Height + margin.VerticalThickness);

			return DesiredSize;
		}
	}
}