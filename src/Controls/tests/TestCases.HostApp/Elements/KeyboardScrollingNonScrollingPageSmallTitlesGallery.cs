﻿using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample
{
	public class KeyboardScrollingNonScrollingPageSmallTitlesGallery : ContentViewGalleryPage
	{
		public KeyboardScrollingNonScrollingPageSmallTitlesGallery()
		{
			On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);
			Add(new KeyboardScrollingEntriesPage());
			Add(new KeyboardScrollingEditorsPage());
			Add(new KeyboardScrollingEntryNextEditorPage());
		}

		protected override bool SupportsScroll
		{
			get { return false; }
		}
	}
}
