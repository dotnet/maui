namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25893, "Setting MenuFlyoutSubItem IconImageSource throws a NullReferenceException", PlatformAffected.UWP)]
	public partial class Issue25893 : TestContentPage
	{
		MenuFlyout _contextMenu;

		public Issue25893()
		{
			InitializeComponent();

			_contextMenu = FlyoutBase.GetContextFlyout(ViewWithMenu) as MenuFlyout;

			if (_contextMenu is null)
				return;

			var itemsCount = _contextMenu.Count;
			InfoLabel.Text = $"{itemsCount}";
		}

		protected override void Init()
		{

		}

		void AddButtonClicked(object sender, EventArgs e)
		{
			if (_contextMenu is null)
				return;

			var itemsCount = _contextMenu.Count;
			InfoLabel.Text = $"{itemsCount}";

			var itemNumber = itemsCount > 0 ? itemsCount : 1;
			_contextMenu.Add(new MenuFlyoutSubItem { IconImageSource = "dotnet_bot.png", Text = $"Added Item {itemNumber}" });
		}

		void RemoveButtonClicked(object sender, EventArgs e)
		{
			if (_contextMenu is null)
				return;

			var itemsCount = _contextMenu.Count;
			InfoLabel.Text = $"{itemsCount}";

			if (itemsCount > 0)
			{
				_contextMenu.RemoveAt(itemsCount - 1);
			}	
		}
    }
}