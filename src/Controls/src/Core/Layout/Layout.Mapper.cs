#nullable disable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public abstract partial class Layout
	{
		static Layout()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(Layout), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;
		}
	}
}
