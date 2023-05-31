using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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