using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1567, "NRE using TapGestureRecognizer on cell with HasUnevenRows", PlatformAffected.iOS, issueTestNumber: 1)]
	public class GitHub1567 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		ICommand SomeCommand;
		ObservableCollection<LocalIem> LocalList { get; set; } = new ObservableCollection<LocalIem>();

		protected override async void Init()
		{
			// Initialize ui here instead of ctor
			var btn = new Button
			{
				AutomationId = "btnFillData",
				Text = "FILL DATA",
				Command = new Command(async () => { await FillData(); })
			};
			var lst = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				Header = btn,
				HasUnevenRows = true,
				RowHeight = -1,
				SeparatorVisibility = SeparatorVisibility.None,
				ItemsSource = LocalList,
				ItemTemplate = new DataTemplate(typeof(CustomCell))
			};

			Content = lst;
			this.BindingContext = this;
			SomeCommand = new Command(SomeCommandAction);
			await FillData();
		}

        [Preserve(AllMembers = true)]
		class CustomCell : ViewCell
		{
			public CustomCell()
			{
				var lbl = new Label { FontSize = 14 };
				lbl.SetBinding(Label.TextProperty, "Value1");
				var grd = new Grid();
				var boxView = new BoxView
				{
					BackgroundColor = Color.Transparent,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand
				};
				var gesture = new TapGestureRecognizer();
				gesture.SetBinding(TapGestureRecognizer.CommandProperty, "SomeCommand");
				boxView.GestureRecognizers.Add(gesture);
				grd.Children.Add(lbl);
				grd.Children.Add(boxView);
				View = grd;
			}
		}

		void SomeCommandAction(object obj)
		{		
		}
		
		async Task FillData()
		{
			await Task.Factory.StartNew(async () =>
			{
				await Task.Delay(100);
				LocalList.Clear();
				for (int i = 0; i < 100; i++)
				{
					LocalList.Add(new LocalIem()
					{
						Value1 = DateTime.UtcNow.Ticks.ToString(),
					});
				}
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

        [Preserve(AllMembers = true)]
		class LocalIem
		{
			public string Value1 { get; set; }
		}

#if UITEST
		[Test]
		public void GitHub1567Test()
		{
			RunningApp.WaitForElement(q => q.Marked("btnFillData"));
			RunningApp.Tap(q => q.Marked("btnFillData"));
		}
#endif
	}
}