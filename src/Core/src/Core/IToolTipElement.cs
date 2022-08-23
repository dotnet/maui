using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	/// <summary>
	/// Indicates that this element has a ToolTip to show
	/// </summary>
	public interface IToolTipElement
	{
		ToolTip? ToolTip { get; }
	}
}
