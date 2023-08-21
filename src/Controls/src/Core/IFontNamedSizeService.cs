// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls.Internals
{
	[Obsolete]
	public interface IFontNamedSizeService
	{
		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);
	}
}