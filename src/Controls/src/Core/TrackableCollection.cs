// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	internal class TrackableCollection<T> : ObservableCollection<T>
	{
		public event EventHandler Clearing;

		protected override void ClearItems()
		{
			Clearing?.Invoke(this, EventArgs.Empty);
			base.ClearItems();
		}
	}
}