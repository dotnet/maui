using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class LineBreakModeLayoutPage : ContentPage
	{
		public LineBreakModeLayoutPage()
		{
			InitializeComponent();
			this.BindingContext = this;
		}

		public ObservableCollection<object> Items { get; set; } = new()
		{
			new(),
			new(),
			new(),
			new(),
		};
	}
}