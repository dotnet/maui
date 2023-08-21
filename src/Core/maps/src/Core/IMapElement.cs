// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Maps
{
	public interface IMapElement : IElement, IStroke
	{
		object? MapElementId { get; set; }
	}
}
