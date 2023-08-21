// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		partial void HandlePlatformUnloadedLoaded()
		{
			if (Window != null)
			{
				OnLoadedCore();
			}
			else
			{
				OnUnloadedCore();
			}
		}
	}
}
