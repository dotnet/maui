// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample
{
	[Preserve(AllMembers = true)]
	public class GestureRecognizerGallery : ContentViewGalleryPage
	{
		public GestureRecognizerGallery()
		{
			Add(new PointerGestureRecognizerEvents());
		}
	}
}

