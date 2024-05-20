using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19803, "[iOS] Setting Binding on Span GridItemsLayout results in NullReferenceException", PlatformAffected.iOS)]
	public partial class Issue19803 : ContentPage
	{
		public Issue19803()
		{
			InitializeComponent();
			BindingContext = this;
		}

		public int ColumnCount { get; set; } = 3;

		private void UpdateColumnCount(object sender, EventArgs e)
		{
			ColumnCount++;
			Dispatcher.Dispatch(() =>
			{
				OnPropertyChanged(nameof(ColumnCount));
			});
		}
	}
}