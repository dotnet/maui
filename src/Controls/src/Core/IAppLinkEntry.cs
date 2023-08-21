// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
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