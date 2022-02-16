using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface IMenuFlyoutSubItem : IMenuFlyoutItemBase, IList<IMenuFlyoutItemBase>, IImageSourcePart
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }
	}
}
