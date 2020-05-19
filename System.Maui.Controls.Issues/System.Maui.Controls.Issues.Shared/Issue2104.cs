using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;
#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Editor)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2104, "Ensure Multiple Calls to Save Properties Doesn't Cause a Crash")]
	public class Issue2104 : TestContentPage
	{
		const string ErrorMessage = "ErrorMessage";

		protected override void Init()
		{
			var errorMessage = new Label() { AutomationId = ErrorMessage, HeightRequest = 100 };

			Log.Listeners.Add(
				new DelegateLogListener((c, m) => Device.BeginInvokeOnMainThread(() =>
				{
					errorMessage.Text = m;
				})));

			Dictionary<int, object> expectedValues = new Dictionary<int, object>();

			for (int i = 0; i < 40; i++)
			{
				expectedValues.Add(i, DateTime.Now);

			}

			Button AddData = new Button()
			{
				Text = "Click Me To call Properties Save a bunch of times",
				Command = new Command(() =>
				{
					Xamarin.Forms.Application.Current.Properties.Clear();
					Xamarin.Forms.Application.Current.SendSleep();

					for (int i = 0; i < 20; i++)
					{
						Xamarin.Forms.Application.Current.Properties[i.ToString()] = expectedValues[i];
						Xamarin.Forms.Application.Current.SendSleep();
					}

					for (int i = 20; i < 40; i++)
					{
						Xamarin.Forms.Application.Current.Properties[i.ToString()] = expectedValues[i];
						Task.Run(() =>
						{
							Xamarin.Forms.Application.Current.SendSleep();
						});
					}

				}),
				AutomationId = "FillUp"
			};

			Button ClearData = new Button()
			{
				Text = "Click to clear properties",
				AutomationId = "Clear",
				Command = new Command(() =>
				{
					Xamarin.Forms.Application.Current.Properties.Clear();
					Xamarin.Forms.Application.Current.SendSleep();
				}),
			};


			var deserializer = DependencyService.Get<IDeserializer>();

			Button TestData = new Button()
			{
				Text = "Click to test for properties",
				AutomationId = "Test",
				Command = new Command(async () =>
				{
					IDictionary<string, object> properties = await deserializer.DeserializePropertiesAsync();
					if (40 != properties.Count)
					{
						errorMessage.Text = "Invalid Property Count";
					}

					for (int i = 0; i < 40; i++)
					{
						if (!properties[i.ToString()].Equals(expectedValues[i]))
						{
							errorMessage.Text = $"Property Serialized Incorrectly: {properties[i.ToString()]} {expectedValues[i]}";
						}
					}
				}),
			};

			var layout = new StackLayout();
			layout.Children.Add(AddData);
			layout.Children.Add(ClearData);
			layout.Children.Add(TestData);
			layout.Children.Add(errorMessage);
			Content = layout;
		}

#if UITEST

		[Test]
		public void Issue2104Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Clear"));
			RunningApp.Tap(q => q.Marked("Clear"));			
			RunningApp.Tap(q => q.Marked("FillUp")); 
			RunningApp.Tap(q => q.Marked("Test")); 

			var errorMessage = RunningApp.Query(x => x.Marked(ErrorMessage)).First().Text;
			Assert.IsTrue(String.IsNullOrWhiteSpace(errorMessage), errorMessage);
		}

#endif
	}
}
