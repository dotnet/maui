﻿using System;
using System.Linq;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1618, "Horizontal ScrollView in ListView crashes", PlatformAffected.Android)]
	public class Issue1618
		: ContentPage
	{
		public Issue1618()
		{
			Content = new ListView
			{
				ItemsSource = Enumerable.Range(0, 100).Select(i => Enumerable.Repeat(i, 500).Aggregate(string.Empty, (s, n) => s += n + " ")),
				ItemTemplate = new DataTemplate(typeof(MyCell))
			};
		}

		class MyCell
			: ViewCell
		{
			public MyCell()
			{
				var scroll = new ScrollView
				{
					Orientation = ScrollOrientation.Horizontal,
					Content = new Label()
				};

				scroll.Content.SetBinding(Label.TextProperty, ".");

				View = scroll;
			}
		}
	}
}
