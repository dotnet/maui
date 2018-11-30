using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	public interface IShellItemController : IElementController
	{
		Task GoToPart(List<string> parts, Dictionary<string, string> queryData);

		bool ProposeSection(ShellSection shellSection, bool setValue = true);
	}
}