using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57114, "Forms gestures are not supported on UIViews that have native gestures", PlatformAffected.iOS)]
	public class Bugzilla57114 : TestContentPage
	{
		public static string _57114NativeGestureFiredMessage = "_57114NativeGestureFiredMessage";

		Label _results;
		bool _nativeGestureFired;
		bool _formsGestureFired;

		const string Testing = "Testing...";
		const string Success = "Success";
		const string ViewAutomationId = "_57114View";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = $"Tap the Aqua View below. If the label below changes from '{Testing}' to '{Success}', the test has passed."
			};

			_results = new Label { Text = Testing };

			var view = new _57114View
			{
				AutomationId = ViewAutomationId,
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Colors.Aqua,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var tap = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					_formsGestureFired = true;
					UpdateResults();
				})
			};

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<object>(this, _57114NativeGestureFiredMessage, NativeGestureFired);
#pragma warning restore CS0618 // Type or member is obsolete

			view.GestureRecognizers.Add(tap);

			var layout = new StackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Children =
				{
					instructions, _results, view
				}
			};

			Content = layout;
		}

		void NativeGestureFired(object obj)
		{
			_nativeGestureFired = true;
			UpdateResults();
		}

		void UpdateResults()
		{
			if (_nativeGestureFired && _formsGestureFired)
			{
				_results.Text = Success;
			}
			else
			{
				_results.Text = Testing;
			}
		}

		[Preserve(AllMembers = true)]
		public class _57114View : View
		{
		}
	}
}