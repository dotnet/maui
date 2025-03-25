using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22183, "RadioButton with value cannot display selected state correctly", PlatformAffected.iOS)]
	public partial class Issue22183 : ContentPage
	{
		public Issue22183()
		{
			InitializeComponent();

			BindingContext = new Issue22183ViewModel();
		}
	}

	public class Issue22183ViewModel
	{
		public ObservableCollection<Issue22183Model> ItemSource { get; } = new ObservableCollection<Issue22183Model>(
			[new Issue22183Model()
			{
				GroupId = 0,
			},
			new Issue22183Model()
			{
				GroupId = 1,
			}]);
	}

	public class Issue22183Model : BindableObject
	{
		public int GroupId { get; set; }

		public string False => $"False_{GroupId}";

		public string True => $"True_{GroupId}";

		public static readonly BindableProperty ShowOptionsProperty =
			BindableProperty.Create(nameof(ShowOptions), typeof(bool), typeof(Issue22183Model));

		public bool ShowOptions
		{
			set => SetValue(ShowOptionsProperty, value);
			get => (bool)GetValue(ShowOptionsProperty);
		}

		public ObservableCollection<Options> Options { get; }

		public Issue22183Model()
		{
			ShowOptions = false;

			Options = new ObservableCollection<Options>();

			for (int i = 0; i < 10; i++)
			{
				Options.Add(new Options
				{
					Name = $"Options_{i}",
				});
			}
		}
	}

	public class Options
	{
		public string Name { get; set; }
	}
}