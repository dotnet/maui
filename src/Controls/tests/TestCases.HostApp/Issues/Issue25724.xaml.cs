#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25724, "ObjectDisposedException When Toggling Header/Footer in CollectionView Dynamically", PlatformAffected.iOS)]

	public partial class Issue25724 : ContentPage
	{
		object? header;
		object? footer;
		public Issue25724()
		{
			InitializeComponent();
			BindingContext = new Issue25724ViewModel();
		}

		void ToggleHeader(object? sender, System.EventArgs e)
		{
			header = CollectionView.Header ?? header;

			if (CollectionView.Header == null)
				CollectionView.Header = header;
			else
				CollectionView.Header = null;
		}

		void ToggleFooter(object? sender, System.EventArgs e)
		{
			footer = CollectionView.Footer ?? footer;

			if (CollectionView.Footer == null)
				CollectionView.Footer = footer;
			else
				CollectionView.Footer = null;
		}

		internal class Issue25724ViewModel
		{
			public ObservableCollection<string> ItemList { get; set; }
			public Issue25724ViewModel()
			{
				// Initialize the ItemList
				ItemList = new ObservableCollection<string>();
			}

		}

	}
}