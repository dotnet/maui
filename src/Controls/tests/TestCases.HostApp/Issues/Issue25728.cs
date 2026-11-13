using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25728, "Java.Lang.IllegalArgumentException when clearing Entry text with StringFormat binding on Android", PlatformAffected.Android)]
public class Issue25728 : ContentPage
{
	public Issue25728()
	{
		var vm = new Issue25728ViewModel();

		var entry = new Entry
		{
			Keyboard = Keyboard.Numeric,
			AutomationId = "FloatEntry"
		};
		entry.SetBinding(Entry.TextProperty, new Binding(nameof(Issue25728ViewModel.FloatValue), stringFormat: "{0:F2}"));

		BindingContext = vm;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children = { entry }
		};
	}

	public class Issue25728ViewModel : INotifyPropertyChanged
	{
		float _floatValue;

		public float FloatValue
		{
			get => _floatValue;
			set
			{
				if (!Equals(_floatValue, value))
				{
					_floatValue = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
