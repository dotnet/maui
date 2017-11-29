using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2775, "ViewCell background conflicts with ListView Semi-Transparent and Transparent backgrounds")]
	public class Issue2775 : TestContentPage 
	{
		protected override void Init ()
		{
			var list  = new ListView {
				ItemsSource = GetList ("Normal BG Blue"),
				BackgroundColor = Color.Blue,
				ItemTemplate = new DataTemplate (typeof(NormalCell))
			};

			var listTransparent = new ListView {
				ItemsSource = GetList ("Normal BG Transparent"),
				BackgroundColor = Color.Transparent,
				ItemTemplate = new DataTemplate (typeof(NormalCell))
			};

			var listSemiTransparent = new ListView {
				ItemsSource = GetList ("Normal BG SEMI Transparent"),
				BackgroundColor = Color.FromHex("#801B2A39"),
				ItemTemplate = new DataTemplate (typeof(NormalCell))
			};

			var listContextActions = new ListView {
				ItemsSource = GetList ("ContextActions BG PINK"),
				BackgroundColor = Color.Pink,
				ItemTemplate = new DataTemplate (typeof(ContextActionsCell))
			};

			var listContextActionsTransparent = new ListView {
				ItemsSource = GetList ("ContextActions BG Transparent"),
				BackgroundColor = Color.Transparent,
				ItemTemplate = new DataTemplate (typeof(ContextActionsCell))
			};

			var listContextActionsSemiTransparent = new ListView {
				ItemsSource = GetList ("ContextActions BG Semi Transparent"),
				BackgroundColor = Color.FromHex("#801B2A39"),
				ItemTemplate = new DataTemplate (typeof(ContextActionsCell))
			};

			Content = new StackLayout {
				Children = {
					list,
					listTransparent,
					listSemiTransparent,
					listContextActions,
					listContextActionsTransparent,
					listContextActionsSemiTransparent
				},
				BackgroundColor = Color.Red
			};
		}

		[Preserve (AllMembers = true)]
		internal class ContextActionsCell : ViewCell
		{
			public ContextActionsCell ()
			{
				ContextActions.Add (new MenuItem{ Text = "action" });
				var label = new Label ();
				label.SetBinding (Label.TextProperty, "Name");
				View = label;
			}
		}

		[Preserve (AllMembers = true)]
		internal class NormalCell : ViewCell
		{
			public NormalCell ()
			{
				var label = new Label ();
				label.SetBinding (Label.TextProperty, "Name");
				View = label;
			}
		}

		List<ListItemViewModel> GetList (string description)
		{
			var itemList = new List<ListItemViewModel> ();
			for (var i = 1; i < 3; i++) {
				itemList.Add (new ListItemViewModel () { Name = description });
			}
			return itemList;
		}
	
		[Preserve (AllMembers = true)]
		public class ListItemViewModel
		{
			public string Name { get; set; }
		}

#if UITEST
		[Test]
		public void Issue2775Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2775");
			RunningApp.Screenshot ("I see the Label");
		}
#endif
	}
}
