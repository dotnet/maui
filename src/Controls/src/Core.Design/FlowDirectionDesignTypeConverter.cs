using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for FlowDirection values.
	/// </summary>
	public class FlowDirectionDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[]
			{
				"MatchParent",
				"LeftToRight",
				"RightToLeft"
			};
	}
}
