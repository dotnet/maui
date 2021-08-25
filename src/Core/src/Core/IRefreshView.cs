using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IRefreshView : IView
	{
		bool IsRefreshing { get; set; }
		Paint? RefreshColor { get; }
		IView Content { get; }
	}
}
