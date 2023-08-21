// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A collection of fonts.
	/// </summary>
	public interface IFontCollection : IList<FontDescriptor>, ICollection<FontDescriptor>, IEnumerable<FontDescriptor>, IEnumerable
	{
	}
}
