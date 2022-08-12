using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	/// <summary>
	/// Indicates that this element has a UpdateToolTip to show
	/// </summary>
	public interface IToolTipContainer
	{
		ToolTip? ToolTip { get; }
	}
}
