using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui
{
	public interface ITabBar : IFrameworkElement
	{
		IList<ITab> Children { get; }
	}

	public interface ITab : IFrameworkElement
	{
		IFrameworkElement Content { get; }
	}
}
