namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25893, "Setting MenuFlyoutSubItem IconImageSource throws a NullReferenceException", PlatformAffected.UWP)]
	public partial class Issue25893 : TestContentPage
	{
		readonly MenuFlyout _contextMenu;

		public Issue25893()
		{
			InitializeComponent();

			_contextMenu = (MenuFlyout)FlyoutBase.GetContextFlyout(ViewWithMenu);

			if (_contextMenu is null)
				return;

			InfoLabel.Text = $"{_contextMenu.Count}";
		}

		protected override void Init()
		{

		}

		void AddButtonClicked(object sender, EventArgs e)
		{
			var itemsCount = _contextMenu.Count;
			var itemNumber = itemsCount > 0 ? itemsCount : 1;
			_contextMenu.Add(new MenuFlyoutSubItem { IconImageSource = "dotnet_bot.png", Text = $"Added Item {itemNumber}" });

			UpdateInfoLabel();
		}

		void RemoveButtonClicked(object sender, EventArgs e)
		{
			var itemsCount = _contextMenu.Count;

			if (itemsCount > 0)
				_contextMenu.RemoveAt(itemsCount - 1);
			
			UpdateInfoLabel();
		}

		void UpdateInfoLabel()
		{
			var itemsCount = _contextMenu.Count;
			InfoLabel.Text = $"{itemsCount}";
		}
    }
}