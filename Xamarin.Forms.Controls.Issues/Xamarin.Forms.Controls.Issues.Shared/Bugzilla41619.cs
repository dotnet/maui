using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41619, "[WinRT/UWP] Slider binding works incorrectly", PlatformAffected.WinRT)]
	public class Bugzilla41619 : TestContentPage
	{
		protected override void Init()
		{
			var vm = new Bugzilla41619ViewModel { SliderValue = 5 };
			BindingContext = vm;
			var label = new Label();
			label.SetBinding(Label.TextProperty, "SliderValue");
			var slider = new Slider
			{
				Maximum = 10,
				Minimum = 1,
			};
			slider.SetBinding(Slider.ValueProperty, "SliderValue", BindingMode.TwoWay);
			Content = new StackLayout
			{
				Children =
				{
					label,
					slider,
					new Label { Text = "The initial slider value above should be 5." }
				}
			};
		}

		[Preserve(AllMembers = true)]
		class Bugzilla41619ViewModel : INotifyPropertyChanged
		{
			private double _sliderValue;

			public double SliderValue
			{
				get { return _sliderValue; }
				set
				{
					_sliderValue = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}