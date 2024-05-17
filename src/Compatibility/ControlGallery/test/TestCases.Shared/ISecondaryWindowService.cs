using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public interface ISecondaryWindowService
	{
		Task OpenSecondaryWindow(Type pageType);
	}
}
