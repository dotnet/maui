using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public partial class ToolTip
	{
		/// <summary>
		/// ToolTip content.
		/// </summary>
		public object? Content { get; set; }

		/// <summary>
		/// Gets or sets the delay (in milliseconds) before the tooltip is shown.
		/// </summary>
		public int? Delay { get; set; }

		/// <summary>
		/// Gets or sets the duration (in milliseconds) that the tooltip is shown.
		/// </summary>
		public int? Duration { get; set; }
	}
}
