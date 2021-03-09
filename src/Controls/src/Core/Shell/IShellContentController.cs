using System;

namespace Microsoft.Maui.Controls
{
	public interface IShellContentController : IElementController
	{
		Page GetOrCreateContent();

		void RecyclePage(Page page);

		Page Page { get; }
		event EventHandler IsPageVisibleChanged;
	}
}