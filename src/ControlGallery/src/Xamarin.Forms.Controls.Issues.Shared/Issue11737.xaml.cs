using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
	[NUnit.Framework.Category(UITestCategories.Brush)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11737, "[Bug] SwipeView Closes After Command Executes Despite SwipeBehaviorOnInvoked=RemainOpen (iOS)", PlatformAffected.iOS)]
	public partial class Issue11737 : TestContentPage
	{
		public Issue11737()
		{
#if APP
			Title = "Issue 11737";
			InitializeComponent();
			BindingContext = new Issue11737ViewModel();
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	class Issue11737Model : BindableObject
	{
		int _count = 1;

		public string Title { get; set; }

		public int Count
		{
			get { return _count; }
			set
			{
				_count = value;
				OnPropertyChanged();
			}
		}
	}

	[Preserve(AllMembers = true)]
	class Issue11737ViewModel : BindableObject
	{
		public ObservableCollection<Issue11737Model> Items { get; private set; } = new ObservableCollection<Issue11737Model>();

		public ICommand IncrementCommand => new Command<Issue11737Model>(Increment);
		public ICommand DecrementCommand => new Command<Issue11737Model>(Decrement);


		public Issue11737ViewModel()
		{
			Items = new ObservableCollection<Issue11737Model>
			{
				new Issue11737Model(){ Title = "item 1" , Count=2 },
				new Issue11737Model(){ Title = "item 2" , Count=5 },
				new Issue11737Model(){ Title = "item 3" , Count=3 },
			};
		}

		public void Increment(Issue11737Model item)
		{
			item.Count++;
		}
		public void Decrement(Issue11737Model item)
		{
			item.Count--;
		}
	}
}