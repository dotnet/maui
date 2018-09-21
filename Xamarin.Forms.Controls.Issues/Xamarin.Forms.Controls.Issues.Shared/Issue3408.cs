using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using static Xamarin.Forms.Controls.Issues.Issue3408;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	// This may crash for you on Android if you click too many buttons
	// https://github.com/xamarin/Xamarin.Forms/issues/3603
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3408, "System.ObjectDisposedException: from SwitchCellRenderer when changing ItemSource", PlatformAffected.iOS)]
	public class Issue3408 : TestContentPage
	{
		public static List<Recommendation> GetRecommendations(object e)
		{
			switch (e)
			{
				case List<RecommendationsViewModel> pc: return pc.First().Recommendations;
				case List<RecommendationsViewModel2> pc: return pc.First().Recommendations;
				default: return null;
			}
		}
#if UITEST
		protected override bool Isolate => true;
#endif
		protected override void Init()
		{

			var grd = new Grid();

			var aacountListView = new ListView();
			aacountListView.HasUnevenRows = true;
			aacountListView.ItemTemplate = new AccountDetailsDataTemplateSelector();
			aacountListView.BindingContext = new List<RecommendationsViewModel> { new RecommendationsViewModel() };

			aacountListView.SetBinding(ListView.ItemsSourceProperty, ".");
			var btn = new Button
			{
				Text = "Change Source",
				AutomationId = "btn1",
				Command = new Command(() =>
				{
					aacountListView.BindingContext = new List<RecommendationsViewModel2> { new RecommendationsViewModel2() };
				})
			};
			var btn2 = new Button
			{
				Text = "Change Property",
				AutomationId = "btn2",
				Command = new Command(() =>
				{

					foreach (var item in GetRecommendations(aacountListView.BindingContext))
					{
						item.Name = "New Item Name";
						item.IsBusy = !item.IsBusy;
					}

				})
			};
			grd.Children.Add(aacountListView);
			Grid.SetRow(aacountListView, 0);
			grd.Children.Add(btn);
			Grid.SetRow(btn, 1);
			grd.Children.Add(btn2);
			Grid.SetRow(btn2, 2);
			Content = grd;
		}

#if UITEST
		[Test]
		public void Issue3408Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("btn1"));
			RunningApp.WaitForElement (q => q.Marked ("Click to Change"));
			RunningApp.Tap(q => q.Marked("btn1"));
			RunningApp.WaitForElement(q => q.Marked("This should have changed"));
			RunningApp.Tap(q => q.Marked("btn2"));
			RunningApp.WaitForElement(q => q.Marked("New Item Name"));
		}
#endif

		[Preserve(AllMembers = true)]
		public class RecommendationsBaseViewModel : ViewModelBase
		{
			public string AccountName => $"";
			public List<Recommendation> Recommendations { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class RecommendationsViewModel : RecommendationsBaseViewModel
		{
			public string AccountName => $"Recommendations";

			public RecommendationsViewModel()
			{
				Recommendations = new List<Recommendation>()
			{
					new Recommendation(){ Name = "Click to Change"} ,
					new Recommendation(){ Name = "Recommendations"} ,
					new Recommendation(){ Name = "Recommendations"} ,
			};
			}
		}

		[Preserve(AllMembers = true)]
		public class RecommendationsViewModel2 : RecommendationsBaseViewModel
		{
			public string AccountName => $"Recommendations 2";
			public RecommendationsViewModel2()
			{
				Recommendations = new List<Recommendation>()
			{
					new Recommendation(){ Name = "This should have changed"} ,
					new Recommendation(){ Name = "Recommendations 2"} ,
					new Recommendation(){ Name = "Recommendations 2", IsBusy = true } ,
			};
			}
		}

		[Preserve(AllMembers = true)]
		public class Recommendation : ViewModelBase
		{
			string _name;
			public string Name
			{
				get { return _name; }
				set
				{
					if (_name == value)
						return;
					_name = value;
					OnPropertyChanged();
				}
			}
		}

	}

	[Preserve(AllMembers = true)]
	public class RecommendationsView : ContentView
	{
		public RecommendationsView()
		{
			Grid grd = new Grid();
			var lst = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var swittch = new SwitchCell();
					swittch.SetBinding(SwitchCell.TextProperty, new Binding("Name"));
					swittch.SetBinding(SwitchCell.OnProperty, new Binding("IsBusy"));
					return swittch;
				})

			};

			lst.SetBinding(ListView.ItemsSourceProperty, new Binding("Recommendations"));
			grd.Children.Add(lst);
			Content = grd;
		}

		// This work around exists because of this issue
		// https://github.com/xamarin/Xamarin.Forms/issues/3602
		object context = null;
		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			if (BindingContext == null)
				Device.BeginInvokeOnMainThread(() => BindingContext = context);
			else
				context = BindingContext;
		}
	}

	[Preserve(AllMembers = true)]
	public class AccountDetailsDataTemplateSelector : DataTemplateSelector
	{
		public Lazy<DataTemplate> RecommendationsViewDataTemplate { get; }
		public Lazy<ViewCell> RecommendationsView { get; }

		public Lazy<DataTemplate> RecommendationsViewDataTemplate2 { get; }
		public Lazy<ViewCell> RecommendationsView2 { get; }

		public AccountDetailsDataTemplateSelector()
		{
			RecommendationsView = new Lazy<ViewCell>(() => new ViewCell() { View = new RecommendationsView() });
			RecommendationsViewDataTemplate = new Lazy<DataTemplate>(() => new DataTemplate(() => RecommendationsView.Value));


			RecommendationsView2 = new Lazy<ViewCell>(() => new ViewCell() { View = new RecommendationsView() });
			RecommendationsViewDataTemplate2 = new Lazy<DataTemplate>(() => new DataTemplate(() => RecommendationsView2.Value));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item == null)
			{
				return null;
			}

			if (item is RecommendationsViewModel)
			{
				RecommendationsView.Value.BindingContext = item;
				return RecommendationsViewDataTemplate.Value;
			}

			if (item is RecommendationsViewModel2)
			{
				RecommendationsView2.Value.BindingContext = item;
				return RecommendationsViewDataTemplate2.Value;
			}

			throw new ArgumentException("Invalid ViewModel Type");
		}
	}
}
