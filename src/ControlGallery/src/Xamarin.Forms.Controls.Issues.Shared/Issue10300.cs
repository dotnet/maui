using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10300, "ObservableCollection.RemoveAt(index) with a valid index raises ArgementOutOfRangeException", PlatformAffected.iOS)]
	public class Issue10300 : TestContentPage // or TestFlyoutPage, etc ...
	{
		CarouselView carousel;

		public class ModalPage : ContentPage
		{
			public ModalPage()
			{
				var btn = new Button
				{
					Text = "Close",
					TextColor = Color.White,
					BackgroundColor = Color.Red,
					VerticalOptions = LayoutOptions.End
				};
				btn.Clicked += OnCloseClicked;
				Content = btn;
			}

			private void OnCloseClicked(object sender, EventArgs e)
			{
				this.Navigation.PopModalAsync();
				MessagingCenter.Instance.Send<Page>(this, "Delete");
			}
		}

		protected override void Init()
		{
			Items = new ObservableCollection<ModelIssue10300>(new[]
															{
																new ModelIssue10300("1", Color.Aqua),
																new ModelIssue10300("2", Color.BlueViolet),
																new ModelIssue10300("3", Color.Coral),
																new ModelIssue10300("4", Color.DarkGoldenrod),
																new ModelIssue10300("5", Color.Fuchsia),
																new ModelIssue10300("6", Color.Gold),
																new ModelIssue10300("7", Color.HotPink),
																new ModelIssue10300("8", Color.IndianRed),
																new ModelIssue10300("9", Color.Khaki),
															});
			carousel = new CarouselView();
			carousel.ItemTemplate = new DataTemplate(() =>
			{
				var l = new Grid();
				l.SetBinding(Grid.BackgroundColorProperty, new Binding("Color"));
				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, new Binding("Text"));
				l.Children.Add(label);
				return l;
			});
			carousel.CurrentItemChanged += Carousel_CurrentItemChanged;
			carousel.PositionChanged += Carousel_PositionChanged;

			Grid.SetColumnSpan(carousel, 2);


			carousel.SetBinding(CarouselView.ItemsSourceProperty, new Binding("Items"));
			carousel.BindingContext = this;

			var grd = new Grid
			{
				Margin = new Thickness(5)
			};
			grd.RowDefinitions.Add(new RowDefinition());
			grd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var btn = new Button
			{
				Text = "Delete",
				BackgroundColor = Color.Red,
				TextColor = Color.White
			};
			var btnAdd = new Button
			{
				Text = "Add",
				BackgroundColor = Color.Red,
				TextColor = Color.White
			};
			btn.Clicked += OnDeleteClicked;
			Grid.SetRow(btn, 1);
			Grid.SetColumn(btn, 0);

			btnAdd.Clicked += OnAddlicked;
			Grid.SetRow(btnAdd, 1);
			Grid.SetColumn(btnAdd, 1);

			grd.Children.Add(carousel);
			grd.Children.Add(btn);
			grd.Children.Add(btnAdd);
			Content = grd;
			MessagingCenter.Instance.Subscribe<Page>(this, "Delete", Callback);
		}

		void Carousel_PositionChanged(object sender, PositionChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"Position old {e.PreviousPosition} Position new {e.CurrentPosition}");
		}

		void Carousel_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"Current old {e.PreviousItem} Current new {e.CurrentItem}");
		}

		void Callback(Page page)
		{
			var index = Items.IndexOf(carousel.CurrentItem as ModelIssue10300);
			System.Diagnostics.Debug.WriteLine($"Delete {index}");
			Items.RemoveAt(index);
		}

		public ObservableCollection<ModelIssue10300> Items { get; set; }


		async void OnDeleteClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new ModalPage());
		}

		void OnAddlicked(object sender, EventArgs e)
		{
			Items.Insert(0, new ModelIssue10300("0", Color.PaleGreen));
		}

#if UITEST
		[Test]
		public void Issue10300Test() 
		{
			RunningApp.Tap("Add");
			RunningApp.Tap("Delete");
			RunningApp.WaitForElement("Close");
			RunningApp.Tap("Close");
			RunningApp.WaitForElement(q => q.Marked("2"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ModelIssue10300
	{
		public string Text { get; set; }

		public Color Color { get; set; }

		public ModelIssue10300(string text, Color color)
		{
			this.Text = text;
			this.Color = color;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}