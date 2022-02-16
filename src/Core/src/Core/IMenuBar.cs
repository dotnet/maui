using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface IMenuBar : IList<IMenuBarItem>, IElement
	{
		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }
	}
}
