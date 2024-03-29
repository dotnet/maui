﻿using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32842,
	"[WinRT] ItemSelected Not Ignored When a ListView Item Contains a TapGestureRecognizer", PlatformAffected.WinRT)]
	public partial class Bugzilla32842 : ContentPage
	{
		public Bugzilla32842()
		{

			List<string> items = new List<string> { "item1", "item2", "item3" };

			InitializeComponent();

			MainList.ItemsSource = items;
			MainList.ItemSelected += MainListSelectionChanged;
		}

		int _boxTaps;
		int _listSelections;

		void MainListSelectionChanged(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null)
			{
				return;
			}

			_listSelections += 1;

			ListResults.Text = $"Selections = {_listSelections}";
		}

		void BoxTapped(object sender, EventArgs args)
		{
			_boxTaps += 1;

			BoxResults.Text = $"Box Taps = {_boxTaps}";
		}
	}
#endif
}
