using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public interface IAppLinkEntry
	{
		Uri AppLinkUri { get; set; }

		string Description { get; set; }

		bool IsLinkActive { get; set; }

		IDictionary<string, string> KeyValues { get; }

		ImageSource Thumbnail { get; set; }

		string Title { get; set; }
	}
}