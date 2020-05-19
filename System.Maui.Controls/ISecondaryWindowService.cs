using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Maui.Controls
{
	public interface ISecondaryWindowService
	{
		Task OpenSecondaryWindow(Type pageType);
	}
}
