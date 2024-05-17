using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest.Queries;
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2951, "On Android, button background is not updated when color changes ")]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue2951 : TestContentPage
	{
		public Issue2951()
		{
#if APP
			InitializeComponent();
#endif
		}

		async void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
		{
			if (e.ItemIndex == 2)
			{
				await Task.Delay(10);
#if APP
				lblReady.Text = "Ready";
#endif
			}
		}

		protected override void Init()
		{
			BindingContext = new MyViewModel();
		}

		[Preserve(AllMembers = true)]
		public class MyViewModel
		{
			public ObservableCollection<MyItemViewModel> Items { get; private set; }

			public Command<MyItemViewModel> ButtonTapped { get; private set; }

			public MyViewModel()
			{
				ButtonTapped = new Command<MyItemViewModel>(OnItemTapped);

				Items = new ObservableCollection<MyItemViewModel>();

				Items.Add(new MyItemViewModel { Name = "A", IsStarted = false });
				Items.Add(new MyItemViewModel { Name = "B", IsStarted = false });
				Items.Add(new MyItemViewModel { Name = "C", IsStarted = false });
			}

			void OnItemTapped(MyItemViewModel model)
			{
				if (model.IsStarted)
				{
					Items.Remove(model);
				}
				else
				{
					model.IsStarted = true;
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class MyItemViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string _name;

			public string Name
			{
				get { return _name; }
				set
				{
					_name = value;
					OnPropertyChanged("Name");
				}
			}

			bool _isStarted;

			public bool IsStarted
			{
				get { return _isStarted; }
				set
				{
					_isStarted = value;
					OnPropertyChanged("IsStarted");
				}
			}

			void OnPropertyChanged(string propertyName)
			{
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue2951Test()
		{
			RunningApp.WaitForElement("Ready");
			var bt = RunningApp.WaitForElement(c => c.Marked("btnChangeStatus"));

			var buttons = RunningApp.QueryUntilPresent(() =>
			 {
				 var results = RunningApp.Query("btnChangeStatus");
				 if (results.Length == 3)
					 return results;

				 return null;
			 });

			Assert.That(buttons.Length, Is.EqualTo(3));
			RunningApp.Tap(c => c.Marked("btnChangeStatus").Index(1));

			buttons = RunningApp.QueryUntilPresent(() =>
			 {
				 var results = RunningApp.Query("btnChangeStatus");
				 if ((results[1].Text ?? results[1].Label) == "B")
					 return results;

				 return null;
			 });

			var text = buttons[1].Text ?? buttons[1].Label;
			Assert.That(text, Is.EqualTo("B"));
			RunningApp.Tap(c => c.Marked("btnChangeStatus").Index(1));

			buttons = RunningApp.QueryUntilPresent(() =>
			 {
				 var results = RunningApp.Query("btnChangeStatus");
				 if (results.Length == 2)
					 return results;

				 return null;
			 });

			Assert.That(buttons.Length, Is.EqualTo(2));
			//TODO: we should check the color of the button
			//var buttonTextColor = GetProperty<Color> ("btnChangeStatus", Button.BackgroundColorProperty);
			//Assert.AreEqual (Color.Pink, buttonTextColor);
		}


#endif
	}
}

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	public class ButtonExtensions
	{
		public static readonly BindableProperty IsPrimaryProperty = BindableProperty.CreateAttached(
																		"IsPrimary",
																		typeof(bool),
																		typeof(ButtonExtensions),
																		false,
																		BindingMode.TwoWay,
																		null,
																		null,
																		null,
																		null);

		public static bool GetIsPrimary(BindableObject bo)
		{
			return (bool)bo.GetValue(IsPrimaryProperty);
		}

		public static void SetIsPrimary(BindableObject bo, bool value)
		{
			bo.SetValue(IsPrimaryProperty, value);
		}
	}
}
