using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public interface ISecondaryWindowService
	{
		Task OpenSecondaryWindow(Type pageType);
	}
}
