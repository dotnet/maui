namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35736, "SearchHandler QueryIcon, ClearIcon, ClearPlaceholderIcon need to update visually at runtime", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue35736 : Shell
{
	public Issue35736()
	{
		var page = new Issue35736Page();
		Items.Add(new ShellContent
		{
			Title = "Search Icon Test",
			Content = page
		});
	}

	public class Issue35736Page : ContentPage
	{
		readonly SearchHandler _searchHandler;
		bool _useAltQueryIcon;
		bool _useAltClearIcon;
		bool _useAltClearPlaceholderIcon;
		bool _clearPlaceholderEnabled = true;

		readonly Label _queryIconLabel;
		readonly Label _clearIconLabel;
		readonly Label _clearPlaceholderIconLabel;
		readonly Label _clearPlaceholderEnabledLabel;
		readonly Button _toggleClearPlaceholderEnabledBtn;

		public Issue35736Page()
		{
			Title = "Search Icon Test";

			_searchHandler = new SearchHandler
			{
				Placeholder = "Search items...",
				AutomationId = "Issue35736SearchHandler",
				QueryIcon = ImageSource.FromFile("bank.png"),
				ClearIcon = ImageSource.FromFile("bank.png"),
				ClearPlaceholderIcon = ImageSource.FromFile("bank.png"),
				ClearPlaceholderEnabled = true,
				ShowsResults = false,
			};

			Shell.SetSearchHandler(this, _searchHandler);

			_queryIconLabel = new Label { AutomationId = "Issue35736QueryIconLabel", Text = "QueryIcon: bank.png" };
			_clearIconLabel = new Label { AutomationId = "Issue35736ClearIconLabel", Text = "ClearIcon: bank.png" };
			_clearPlaceholderIconLabel = new Label { AutomationId = "Issue35736ClearPlaceholderIconLabel", Text = "ClearPlaceholderIcon: bank.png" };
			_clearPlaceholderEnabledLabel = new Label { AutomationId = "Issue35736ClearPlaceholderEnabledLabel", Text = "ClearPlaceholderEnabled: True" };

			var toggleQueryIconBtn = new Button { Text = "Toggle QueryIcon", AutomationId = "Issue35736ToggleQueryIcon" };
			toggleQueryIconBtn.Clicked += OnToggleQueryIcon;

			var toggleClearIconBtn = new Button { Text = "Toggle ClearIcon", AutomationId = "Issue35736ToggleClearIcon" };
			toggleClearIconBtn.Clicked += OnToggleClearIcon;

			var toggleClearPlaceholderIconBtn = new Button { Text = "Toggle ClearPlaceholderIcon", AutomationId = "Issue35736ToggleClearPlaceholderIcon" };
			toggleClearPlaceholderIconBtn.Clicked += OnToggleClearPlaceholderIcon;

			_toggleClearPlaceholderEnabledBtn = new Button
			{
				Text = "Toggle ClearPlaceholderEnabled (Current: True)",
				AutomationId = "Issue35736ToggleClearPlaceholderEnabled"
			};
			_toggleClearPlaceholderEnabledBtn.Clicked += OnToggleClearPlaceholderEnabled;

			var resetBtn = new Button { Text = "Reset All to Defaults", AutomationId = "Issue35736ResetAll" };
			resetBtn.Clicked += OnResetAll;

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Padding = new Thickness(30, 0),
					Spacing = 15,
					Children =
					{
						new Label { Text = "SearchHandler Icon Demo", FontSize = 20, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
						new VerticalStackLayout
						{
							Spacing = 4,
							Children = { _queryIconLabel, _clearIconLabel, _clearPlaceholderIconLabel, _clearPlaceholderEnabledLabel }
						},
						toggleQueryIconBtn,
						toggleClearIconBtn,
						toggleClearPlaceholderIconBtn,
						_toggleClearPlaceholderEnabledBtn,
						resetBtn,
					}
				}
			};
		}

		void OnToggleQueryIcon(object sender, EventArgs e)
		{
			_useAltQueryIcon = !_useAltQueryIcon;
			var icon = _useAltQueryIcon ? "calculator.png" : "bank.png";
			_searchHandler.QueryIcon = ImageSource.FromFile(icon);
			_queryIconLabel.Text = $"QueryIcon: {icon}";
		}

		void OnToggleClearIcon(object sender, EventArgs e)
		{
			_useAltClearIcon = !_useAltClearIcon;
			var icon = _useAltClearIcon ? "calculator.png" : "bank.png";
			_searchHandler.ClearIcon = ImageSource.FromFile(icon);
			_clearIconLabel.Text = $"ClearIcon: {icon}";
		}

		void OnToggleClearPlaceholderIcon(object sender, EventArgs e)
		{
			_useAltClearPlaceholderIcon = !_useAltClearPlaceholderIcon;
			var icon = _useAltClearPlaceholderIcon ? "calculator.png" : "bank.png";
			_searchHandler.ClearPlaceholderIcon = ImageSource.FromFile(icon);
			_clearPlaceholderIconLabel.Text = $"ClearPlaceholderIcon: {icon}";
		}

		void OnToggleClearPlaceholderEnabled(object sender, EventArgs e)
		{
			_clearPlaceholderEnabled = !_clearPlaceholderEnabled;
			_searchHandler.ClearPlaceholderEnabled = _clearPlaceholderEnabled;
			_clearPlaceholderEnabledLabel.Text = $"ClearPlaceholderEnabled: {_clearPlaceholderEnabled}";
			_toggleClearPlaceholderEnabledBtn.Text = $"Toggle ClearPlaceholderEnabled (Current: {_clearPlaceholderEnabled})";
		}

		void OnResetAll(object sender, EventArgs e)
		{
			_useAltQueryIcon = false;
			_useAltClearIcon = false;
			_useAltClearPlaceholderIcon = false;
			_clearPlaceholderEnabled = true;

			_searchHandler.QueryIcon = null;
			_searchHandler.ClearIcon = null;
			_searchHandler.ClearPlaceholderIcon = null;
			_searchHandler.ClearPlaceholderEnabled = true;

			_queryIconLabel.Text = "QueryIcon: default";
			_clearIconLabel.Text = "ClearIcon: default";
			_clearPlaceholderIconLabel.Text = "ClearPlaceholderIcon: default";
			_clearPlaceholderEnabledLabel.Text = "ClearPlaceholderEnabled: True";
			_toggleClearPlaceholderEnabledBtn.Text = "Toggle ClearPlaceholderEnabled (Current: True)";
		}
	}
}
