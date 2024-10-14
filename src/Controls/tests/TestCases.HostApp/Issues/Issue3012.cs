using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 3012, "[macOS] Entry focus / unfocus behavior", PlatformAffected.macOS)]
public class Issue3012 : TestContentPage
{
	Label _focusedCountLabel = new Label
	{
		Text = "Focused count: 0",
		AutomationId = "FocusedCountLabel"
	};
	int _focusedCount;
	int FocusedCount
	{
		get { return _focusedCount; }
		set
		{
			_focusedCount = value;
			_focusedCountLabel.Text = $"Focused count: {value}";
		}
	}

	Label _unfocusedCountLabel = new Label
	{
		Text = "Unfocused count: 0",
		AutomationId = "UnfocusedCountLabel"
	};
	int _unfocusedCount;
	int UnfocusedCount
	{
		get { return _unfocusedCount; }
		set
		{
			_unfocusedCount = value;
			_unfocusedCountLabel.Text = $"Unfocused count: {value}";
		}
	}

	protected override void Init()
	{
		var entry = new Entry
		{
			AutomationId = "FocusTargetEntry"
		};
		entry.Focused += (sender, e) =>
		{
			FocusedCount++;
		};
		entry.Unfocused += (sender, e) =>
		{
			UnfocusedCount++;
		};

		var dumbyEntry = new Entry()
		{
			Placeholder = "I'm just here as another focus target",
			AutomationId = "DumbyEntry"
		};

		var divider = new BoxView
		{
			HeightRequest = 1,
			BackgroundColor = Colors.Black
		};

		StackLayout stackLayout = new StackLayout();
		stackLayout.Children.Add(dumbyEntry);
		stackLayout.Children.Add(divider);
		stackLayout.Children.Add(entry);
		stackLayout.Children.Add(_focusedCountLabel);
		stackLayout.Children.Add(_unfocusedCountLabel);

		Content = stackLayout;
	}
}
