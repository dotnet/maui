using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class IndicatorView : IIndicatorView
	{
		public Paint IndicatorsColor => IndicatorColor.AsPaint();

		public Paint PositionIndicatorColor => SelectedIndicatorColor.AsPaint();
	}
}
