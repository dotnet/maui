// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
