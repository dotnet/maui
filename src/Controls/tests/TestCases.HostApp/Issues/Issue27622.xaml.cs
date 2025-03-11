using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27622, "Windows CollectionView Keyboard Navigation")]
	public partial class Issue27622 : TestContentPage
	{
		public Issue27622()
		{
			InitializeComponent();

			CollectionView27622.On<WindowsOS>().SetSingleSelectionFollowsFocus(true);
		}

		protected override void Init()
		{
			BindingContext = new ViewModel27622();
		}

		void OnNoneRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (CollectionView27622 is not null)
				CollectionView27622.SelectionMode = SelectionMode.None;
		}

		void OnSingleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (CollectionView27622 is not null)
				CollectionView27622.SelectionMode = SelectionMode.Single;
		}

		void OnMultipleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (CollectionView27622 is not null)
				CollectionView27622.SelectionMode = SelectionMode.Multiple;
		}
	}

	public class ViewModel27622
	{
		public ObservableCollection<Model27622> Items { get; set; }

		public ViewModel27622()
		{
			var collection = new ObservableCollection<Model27622>();

			for (var i = 0; i < 20; i++)
			{
				collection.Add(new Model27622()
				{
					Text = (i + 1).ToString()
				});
			}

			Items = collection;
		}
	}

	public class Model27622
	{
		public string Text { get; set; }

		public Model27622()
		{

		}
	}
}