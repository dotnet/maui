using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	public partial class _23732Page : ContentPage
	{
		public ObservableCollection<string> Models { get; } = new();
		public _23732Page(bool isPage4)
		{
			InitializeComponent();
			BindingContext = this;
			for (int i = 0; i <= 6; i++)
			{
				Models.Add(string.Empty);
			}

			if (isPage4)
			{
				var label = new Label
				{
					Text = "page4",
					AutomationId = "label",
					HorizontalTextAlignment = TextAlignment.Center
				};
				(Content as Grid)?.Children.Insert(0, label);
			}
		}

		private string _label;
		public string Label
		{
			get => _label;
			set
			{
				if (_label != value)
				{
					_label = value;
					OnPropertyChanged();
				}
			}
		}
	}

	[Issue(IssueTracker.Github, 23732, "TabBar content not displayed properly", PlatformAffected.Android)]
	public partial class Issue23732 : TabbedPage
	{
		public Issue23732()
		{
			for (int i = 1; i <= 5; i++)
			{
				Children.Add(CreateNavigationPage($"Page {i}", $"page{i}"));
			}
		}

		private NavigationPage CreateNavigationPage(string title, string label)
		{
			var mainPage = new _23732Page(label == "page4")
			{
				Label = label
			};

			return new NavigationPage(mainPage)
			{
				Title = title
			};
		}
	}
}