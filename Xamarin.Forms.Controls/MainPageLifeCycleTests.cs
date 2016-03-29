namespace Xamarin.Forms.Controls
{
	public class MainPageLifeCycleTests : ContentPage
	{
		int _numTimesStarted;
		int _numTimesSlept;
		int _numTimesResumed;

		readonly StackLayout _numTimesStartedLayout;
		readonly StackLayout _numTimesSleptLayout;
		readonly StackLayout _numTimesResumedLayout;

		public MainPageLifeCycleTests ()
		{
			object timesStarted;
			if (!Application.Current.Properties.TryGetValue ("TimesStarted", out timesStarted)) {
				Application.Current.Properties["TimesStarted"] = 0;
			} 
			var numTimesStarted = (int)Application.Current.Properties["TimesStarted"];
			

			object timesSlept;
			if (!Application.Current.Properties.TryGetValue ("TimesSlept", out timesSlept)) {
				Application.Current.Properties["TimesSlept"] = 0;
			}
			var numTimesSlept = (int)Application.Current.Properties["TimesSlept"];
	

			object timesResumed;
			if (!Application.Current.Properties.TryGetValue ("TimesResumed", out timesResumed)) {
				Application.Current.Properties["TimesResumed"] = 0;
			}
			var numTimesResumed = (int)Application.Current.Properties["TimesResumed"];

			_numTimesStartedLayout = BuildLabelLayout ("TimesStarted", numTimesStarted);
			_numTimesSleptLayout = BuildLabelLayout ("TimesSlept", numTimesSlept);
			_numTimesResumedLayout = BuildLabelLayout ("TimesResumed", numTimesResumed);

			var layout = new StackLayout {
				Children = {
					_numTimesStartedLayout,
					_numTimesSleptLayout,
					_numTimesResumedLayout
				}
			};

			Content = layout;
		}

		StackLayout BuildLabelLayout (string title, int property)
		{
			var labelTitle = new Label {
				Text = title
			};

			var valueLabel = new Label {
				Text = property.ToString ()
			};

			return new StackLayout {
				Children = {
					labelTitle,
					valueLabel
				}
			};
		}

		public void UpdateLabels ()
		{
			((Label)_numTimesStartedLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesStarted"]).ToString ();
			((Label)_numTimesSleptLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesSlept"]).ToString ();
			((Label)_numTimesResumedLayout.Children[1]).Text = ((int)Application.Current.Properties["TimesResumed"]).ToString ();
		}
	}
}