using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest.Queries;
using NUnit.Framework;
#endif


namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2951, "On Android, button background is not updated when color changes ")]
	public partial class Issue2951 : TestContentPage
	{
		public Issue2951 ()
		{
			#if APP
			InitializeComponent ();
			#endif
		}

		protected override void Init ()
		{
			BindingContext = new MyViewModel ();
		}

		[Preserve (AllMembers = true)]
		public class MyViewModel
		{
			public ObservableCollection<MyItemViewModel> Items { get; private set; }

			public Command<MyItemViewModel> ButtonTapped { get; private set; }

			public MyViewModel ()
			{
				ButtonTapped = new Command<MyItemViewModel> (OnItemTapped);

				Items = new ObservableCollection<MyItemViewModel> ();

				Items.Add (new MyItemViewModel { Name = "A", IsStarted = false });
				Items.Add (new MyItemViewModel { Name = "B", IsStarted = false });
				Items.Add (new MyItemViewModel { Name = "C", IsStarted = false });
			}

			void OnItemTapped (MyItemViewModel model)
			{
				if (model.IsStarted) {
					Items.Remove (model);
				} else {
					model.IsStarted = true;
				}
			}
		}

		[Preserve (AllMembers = true)]
		public class MyItemViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string _name;

			public string Name {
				get { return _name; } 
				set {
					_name = value;
					OnPropertyChanged ("Name");
				}
			}

			bool _isStarted;

			public bool IsStarted {
				get { return _isStarted; } 
				set {
					_isStarted = value;
					OnPropertyChanged ("IsStarted");
				}
			}

			void OnPropertyChanged (string propertyName)
			{
				if (PropertyChanged != null) {
					PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
				}
			}
		}
	
		#if UITEST
		[Test]
		public void Issue2951Test ()
		{
			var bt = RunningApp.WaitForElement (c => c.Marked ("btnChangeStatus"));
			var buttons = RunningApp.Query (c => c.Marked ("btnChangeStatus"));
			Assert.That (buttons.Length, Is.EqualTo (3));
			RunningApp.Tap(c => c.Marked ("btnChangeStatus").Index(1));
			buttons = RunningApp.Query (c => c.Marked ("btnChangeStatus"));
			var text = buttons [1].Text ?? buttons [1].Label;
			Assert.That (text, Is.EqualTo ("B"));
			RunningApp.Tap(c => c.Marked ("btnChangeStatus").Index(1));
			buttons = RunningApp.Query (c => c.Marked ("btnChangeStatus"));
			Assert.That (buttons.Length, Is.EqualTo (2));
			//TODO: we should check the color of the button
			//var buttonTextColor = GetProperty<Color> ("btnChangeStatus", Button.BackgroundColorProperty);
			//Assert.AreEqual (Color.Pink, buttonTextColor);
		}

	
		#endif
	}
}

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	public class ButtonExtensions
	{
#pragma warning disable 618
		public static readonly BindableProperty IsPrimaryProperty = BindableProperty.CreateAttached<ButtonExtensions, bool>(
#pragma warning restore 618
																		bindable => GetIsPrimary(bindable),
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
