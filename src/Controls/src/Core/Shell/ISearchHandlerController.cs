// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public interface ISearchHandlerController
	{
		event EventHandler<ListProxyChangedEventArgs> ListProxyChanged;

		IReadOnlyList<object> ListProxy { get; }

		void ClearPlaceholderClicked();

		void ItemSelected(object obj);

		void QueryConfirmed();
	}
}