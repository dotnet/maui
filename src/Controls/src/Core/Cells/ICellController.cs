// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public interface ICellController
	{
		event EventHandler ForceUpdateSizeRequested;

		void SendAppearing();
		void SendDisappearing();
	}
}
