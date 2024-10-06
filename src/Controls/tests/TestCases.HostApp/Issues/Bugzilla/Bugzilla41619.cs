﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41619, "[WinRT/UWP] Slider binding works incorrectly", PlatformAffected.WinRT)]
	public class Bugzilla41619 : TestContentPage
	{
		const double _success = 6;
		protected override void Init()
		{
			var vm = new Bugzilla41619ViewModel();
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
					new Label { Text = $"The initial slider value above should be {_success}." }
				}
			};
		}

		[Preserve(AllMembers = true)]
		class Bugzilla41619ViewModel : INotifyPropertyChanged
		{
			private double _sliderValue = _success;

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