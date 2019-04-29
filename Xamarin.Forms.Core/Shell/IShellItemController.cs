using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		bool ProposeSection(ShellSection shellSection, bool setValue = true);
	}
}