using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18891, "CollectionView with many items (10,000+) hangs or crashes on iOS", PlatformAffected.iOS)]
	public partial class Issue18891 : ContentPage
	{
		readonly Stopwatch _stopwatch;

		public Issue18891()
		{
			_stopwatch = Stopwatch.StartNew();
			InitializeComponent();
			TestCollectionView.ItemsSource = Enumerable.Range(0, 200000).Select(i => i.ToString()).ToList();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if(propertyName == HeightProperty.PropertyName || propertyName == WidthProperty.PropertyName)
			{
				_stopwatch.Stop();
				TestLabel.Text = $"{_stopwatch.ElapsedMilliseconds}";
			}
		}
	}
}