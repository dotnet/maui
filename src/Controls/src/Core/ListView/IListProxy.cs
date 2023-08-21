// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls
{
	public interface IListProxy : IList
	{
		event NotifyCollectionChangedEventHandler CollectionChanged;
		IEnumerable ProxiedEnumerable { get; }
		bool TryGetValue(int index, out object value);
	}
}
