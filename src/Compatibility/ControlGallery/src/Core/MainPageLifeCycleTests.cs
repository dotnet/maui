using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class MainPageLifeCycleTests : ContentPage
	{
		readonly StackLayout _numTimesStartedLayout;
		readonly StackLayout _numTimesSleptLayout;
		readonly StackLayout _numTimesResumedLayout;

		public MainPageLifeCycleTests()
		{
			if (!Preferences.ContainsKey("TimesStarted"))
			{
				Preferences.Set("TimesStarted", 0);
			}
			var numTimesStarted = Preferences.Get("TimesStarted", 0);

			if (!Preferences.ContainsKey("TimesSlept"))
			{
				Preferences.Set("TimesSlept", 0);
			}
			var numTimesSlept = Preferences.Get("TimesSlept", 0);

			if (!Preferences.ContainsKey("TimesResumed"))
			{
				Preferences.Set("TimesResumed", 0);
			}
			var numTimesResumed = Preferences.Get("TimesResumed", 0);

			_numTimesStartedLayout = BuildLabelLayout("TimesStarted", numTimesStarted);
			_numTimesSleptLayout = BuildLabelLayout("TimesSlept", numTimesSlept);
			_numTimesResumedLayout = BuildLabelLayout("TimesResumed", numTimesResumed);

			var layout = new StackLayout
			{
				Children = {
					_numTimesStartedLayout,
					_numTimesSleptLayout,
					_numTimesResumedLayout
				}
			};

			Content = layout;
		}

		StackLayout BuildLabelLayout(string title, int property)
		{
			var labelTitle = new Label
			{
				Text = title
			};

			var valueLabel = new Label
			{
				Text = property.ToString()
			};

			return new StackLayout
			{
				Children = {
					labelTitle,
					valueLabel
				}
			};
		}

		public void UpdateLabels()
		{
			((Label)_numTimesStartedLayout.Children[1]).Text = Preferences.Get("TimesStarted", 0).ToString();
			((Label)_numTimesSleptLayout.Children[1]).Text = Preferences.Get("TimesSlept", 0).ToString();
			((Label)_numTimesResumedLayout.Children[1]).Text = Preferences.Get("TimesResumed", 0).ToString();
		}
	}
}