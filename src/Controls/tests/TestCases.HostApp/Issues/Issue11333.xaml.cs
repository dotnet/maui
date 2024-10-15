﻿using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11333,
		"[Bug] SwipeView does not work on Android if child has TapGestureRecognizer",
		PlatformAffected.Android)]
	public partial class Issue11333 : TestContentPage
	{
		const string SwipeViewId = "SwipeViewId";

		public Issue11333()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
		}

		void OnTapGestureRecognizerOnTapped(object sender, EventArgs e)
		{
			Debug.WriteLine("Tapped");
		}

		void OnSwipeViewSwipeEnded(object sender, SwipeEndedEventArgs e)
		{
			ResultLabel.Text = e.IsOpen ? "Open" : "Close";
		}
	}


	public class Issue11333Model
	{
		public string Title { get; set; }
		public string Description { get; set; }
	}
}