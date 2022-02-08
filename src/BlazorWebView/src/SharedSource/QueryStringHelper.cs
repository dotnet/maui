// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

namespace Microsoft.AspNetCore.Components.WebView
{
	public static class QueryStringHelper
	{
		public static string RemovePossibleQueryString(string? url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return string.Empty;
			}
			var indexOfQueryString = url.IndexOf('?');
			return (indexOfQueryString == -1)
				? url
				: url.Substring(0, indexOfQueryString);
		}
	}
}
