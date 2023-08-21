// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public interface IGridController
	{
		void InvalidateMeasureInernalNonVirtual(InvalidationTrigger trigger);
	}
}