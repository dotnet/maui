#nullable disable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public abstract partial class Layout
	{
		static Layout() => RemapForControls();

		private static new void RemapForControls()
		{
			VisualElement.RemapIfNeeded();
		}
	}
}
