using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Label)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11875, "[Bug] Label MaxLine not updating value after changing from View Model",
		PlatformAffected.Android)]
	public partial class Issue11875 : TestContentPage
	{
		public Issue11875()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue11875ViewModel();
#endif
		}

		protected override void Init()
		{

		}
	}

	public class Issue11875ViewModel : BindableObject
	{
		int _maxLines;

		public Issue11875ViewModel()
		{
			MaxLines = 2;
		}

		public int MaxLines
		{
			get { return _maxLines; }
			set
			{
				_maxLines = value;
				OnPropertyChanged();
			}
		}

		public ICommand IncreaseCommand => new Command(Increase);
		public ICommand DecreaseCommand => new Command(Decrease);

		void Increase()
		{
			MaxLines++;
		}

		void Decrease()
		{
			if (MaxLines > 0)
				MaxLines--;
		}
	}
}