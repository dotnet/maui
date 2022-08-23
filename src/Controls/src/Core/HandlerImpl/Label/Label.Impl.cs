using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs" />
	public partial class Label : ILabel
	{
		Font ITextStyle.Font => this.ToFont();

#pragma warning disable RS0016 // Add public types and members to the declared API
		protected override Graphics.Size ArrangeOverride(Graphics.Rect bounds)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			var result = base.ArrangeOverride(bounds);


			return result;
		}
	}
}