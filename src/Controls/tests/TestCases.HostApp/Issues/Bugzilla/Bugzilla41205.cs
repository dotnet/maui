﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41205, "UWP CreateDefault passes string instead of object")]
	public class Bugzilla41205 : TestContentPage
	{
		const string _success = "Pass";

		[Preserve(AllMembers = true)]
		public class ViewModel
		{
			public string Text { get { return _success; } }
		}

		[Preserve(AllMembers = true)]
		public class CustomListView : ListView
		{
			protected override Cell CreateDefault(object item)
			{
				if (item is ViewModel)
				{
					var newTextCell = new TextCell();
					newTextCell.SetBinding(TextCell.TextProperty, nameof(ViewModel.Text));
					return newTextCell;
				}
				return base.CreateDefault("Fail");
			}
		}

		protected override void Init()
		{
			var listView = new CustomListView
			{
				ItemsSource = new[]
				{
					new ViewModel(),
					new ViewModel(),
				}
			};

			Content = listView;
		}
	}
}
