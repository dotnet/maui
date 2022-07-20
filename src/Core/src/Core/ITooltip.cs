using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to display Tooltip text for a view
	/// </summary>
	public interface ITooltip
	{
		/// <summary>
		/// Gets the Tooltip text value.
		/// </summary>
		string? TooltipText { get; }
	}
}
