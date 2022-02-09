using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface IMenuFlyoutItem : IMenuFlyoutItemBase
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }
	}
}
