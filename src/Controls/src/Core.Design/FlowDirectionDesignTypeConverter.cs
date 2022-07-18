using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Design
{
	public class FlowDirectionDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[]
			{
				"MatchParent",
				"LeftToRight",
				"RightToLeft"
			};
	}
}
