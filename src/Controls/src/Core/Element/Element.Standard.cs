#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Element
	{
		/// <summary>
		/// Maps the abstract <see cref="AutomationProperties.IsInAccessibleTreeProperty"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="element">The associated <see cref="Element"/> instance</param>
		public static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, Element element)
		{
		}

		/// <summary>
		/// Maps the abstract <see cref="AutomationProperties.ExcludedWithChildrenProperty"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="element">The associated <see cref="Element"/> instance.</param>
		public static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, Element element)
		{
		}

		static void MapAutomationPropertiesIsInAccessibleTree(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesIsInAccessibleTree(handler, e);
		}

		static void MapAutomationPropertiesExcludedWithChildren(IElementHandler handler, IElement element)
		{
			if (element is Element e)
				MapAutomationPropertiesExcludedWithChildren(handler, e);
		}
	}
}
