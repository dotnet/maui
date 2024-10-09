﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36788, "Truncation Issues with Relative Layouts")]
	public class Bugzilla36788 : TestContentPage // or TestFlyoutPage, etc ...
	{
		Label _resultLabel;
		Label _testLabel;
		View _container;

		protected override void Init()
		{
			// Initialize ui here instead of ctor
			var stackLayout = new StackLayout
			{
				Spacing = 8
			};

			var longString = "Very long text in single line to be truncated at tail. Adding extra text to make sure it gets truncated. And even more extra text because otherwise the test might fail if we're in, say, landscape orientation rather than portrait.";

			var contentView = new ContentView
			{
				Padding = 16,
				BackgroundColor = Colors.Gray,
				Content = new Label
				{
					BackgroundColor = Colors.Aqua,
					Text = longString,
					LineBreakMode = LineBreakMode.TailTruncation
				}
			};

			stackLayout.Children.Add(contentView);

			contentView = new ContentView
			{
				Padding = 16,
				BackgroundColor = Colors.Gray,
				Content = new Microsoft.Maui.Controls.Compatibility.RelativeLayout
				{
					BackgroundColor = Colors.Navy,
					Children = {
						{new Label {
							BackgroundColor = Colors.Blue,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (0)},
						{new Label {
							BackgroundColor = Colors.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (0), Microsoft.Maui.Controls.Compatibility.Constraint.Constant (40)},
						{new Label {
							BackgroundColor = Colors.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (10), Microsoft.Maui.Controls.Compatibility.Constraint.Constant (80)},
					}
				}
			};

			stackLayout.Children.Add(contentView);

			contentView = new ContentView
			{
				Padding = 16,
				BackgroundColor = Colors.Gray,
				IsClippedToBounds = true,
				Content = _container = new Microsoft.Maui.Controls.Compatibility.RelativeLayout
				{
					IsClippedToBounds = true,
					BackgroundColor = Colors.Navy,
					Children = {
						{_testLabel = new Label {
							BackgroundColor = Colors.Blue,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (0)},
						{new Label {
							BackgroundColor = Colors.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (0), Microsoft.Maui.Controls.Compatibility.Constraint.Constant (40)},
						{new Label {
							BackgroundColor = Colors.Fuchsia,
							Text = longString,
							LineBreakMode = LineBreakMode.TailTruncation
						}, Microsoft.Maui.Controls.Compatibility.Constraint.Constant (10), Microsoft.Maui.Controls.Compatibility.Constraint.Constant (80)},
					}
				}
			};

			stackLayout.Children.Add(contentView);

			_resultLabel = new Label();
			stackLayout.Children.Add(_resultLabel);

			Content = stackLayout;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(200);

			double fuzzFactor = 15; // labels sometimes overflow slightly, thanks hinting

			if (Math.Abs(_testLabel.Width - _container.Width) < fuzzFactor)
				_resultLabel.Text = "Passed";
		}
	}
}
