using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using static Maui.Controls.Sample.Issues.CollectionViewDynamicallyLoad;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 13126, "[Bug] Regression: 5.0.0-pre5 often fails to draw dynamically loaded collection view content",
		PlatformAffected.iOS, issueTestNumber: 1)]
	public class Issue13126_2 : TestContentPage
	{
		_13126VM _vm;
		const string Success = "Success";

		protected override void Init()
		{
			var collectionView = BindingWithConverter();

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition() { Height = GridLength.Star },
				}
			};

			grid.Children.Add(collectionView);

			Content = grid;

			_vm = new _13126VM();
			BindingContext = _vm;
		}

		protected async override void OnParentSet()
		{
			base.OnParentSet();
			_vm.IsBusy = true;

			await Task.Delay(1000);

			using (_vm.Data.BeginMassUpdate())
			{
				_vm.Data.Add(Success);
			}

			_vm.IsBusy = false;
		}
	}

	internal class _13126VM : INotifyPropertyChanged
	{
		private bool _isBusy;

		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}

			set
			{
				_isBusy = value;
				OnPropertyChanged(nameof(IsBusy));
			}
		}

		public OptimizedObservableCollection<string> Data { get; } = new OptimizedObservableCollection<string>();

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
