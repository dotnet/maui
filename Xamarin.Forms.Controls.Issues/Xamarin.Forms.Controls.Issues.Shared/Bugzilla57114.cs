using System;
using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif

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
		const string AutomationId = "_57114View";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = $"Tap the Aqua View below. If the label below changes from '{Testing}' to '{Success}', the test has passed."
			};

			_results = new Label { Text = Testing };

			var view = new _57114View
			{
				AutomationId = AutomationId,
				HeightRequest = 200, WidthRequest = 200,
				BackgroundColor = Color.Aqua,
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

			MessagingCenter.Subscribe<object>(this, _57114NativeGestureFiredMessage, NativeGestureFired);

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

#if UITEST
		[Test]
		public void _57114BothTypesOfGesturesFire()
		{
			RunningApp.WaitForElement(Testing);
			RunningApp.Tap(AutomationId);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}