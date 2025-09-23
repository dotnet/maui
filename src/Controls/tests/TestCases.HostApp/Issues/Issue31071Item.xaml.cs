namespace Maui.Controls.Sample
{
	public partial class Issue31071Item : ContentView
	{
		public Issue31071Item()
		{
			InitializeComponent();
			MessagingCenter.Send(this, string.Empty, 1);
		}

		~Issue31071Item()
		{
			MessagingCenter.Send(this, string.Empty, -1);
		}
	}
}