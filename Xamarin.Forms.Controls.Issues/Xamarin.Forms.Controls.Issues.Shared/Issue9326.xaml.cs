using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
/*
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
*/

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	// To test this sample include <PackageReference Include="ReactiveUI" Version="11.2.3" /> in Xamarin.Forms.Controls
	// and uncomment the ViewModel
#if UITEST
    [Category(UITestCategories.RefreshView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9326, "[Bug] RefreshView should delay effects of IsEnabled while a refresh is happening and iOS is handling IsEnabled false incorrectly",
		PlatformAffected.iOS)]
	public partial class Issue9326 : TestContentPage
	{
		public Issue9326()
		{
#if APP
			InitializeComponent();
			//BindingContext = new Issue9326ViewModel();
#endif
		}

		protected override void Init()
		{

		}
	}

	/*
    public class Issue9326ViewModel : ReactiveObject
    {
        readonly ObservableAsPropertyHelper<bool> _isRefreshing;

        public Issue9326ViewModel()
        {
            Items = new ObservableCollection<string>
            {
                "one", "two", "three"
            };

            RefreshCommand = ReactiveCommand.Create(SimulateWork);

            this.WhenAnyObservable(x => x.RefreshCommand.IsExecuting)
                .ToProperty(this, x => x.IsRefreshing, out _isRefreshing);
        }

        public ObservableCollection<string> Items { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public bool IsRefreshing => _isRefreshing.Value;

        async void SimulateWork()
        {
            await Task.Delay(2000);
            Items.Add("Test");
        }
    }
	*/
}