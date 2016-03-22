using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	public sealed class CarouselViewGallaryPage : ContentPage // or TestMasterDetailPage, etc ...
	{
		public abstract class Item
		{
			static int s_id = 0;

			int id;

			internal Item()
			{
				id = s_id++;
			}

			public int Id => id;
			public string TypeName => GetType ().Name;
		}
		public sealed class Foo : Item { }
		public sealed class Bar : Item { }
		public sealed class Baz : Item { }
		public sealed class Poo : Item { }
		public sealed class Moo : Item { }

		[Preserve (AllMembers = true)]
		public sealed class ItemView : ContentView
		{
			public static readonly BindableProperty TextColorProperty = BindableProperty.Create (
				propertyName: nameof(TextColor),
				returnType: typeof (Color),
				declaringType: typeof (ItemView),
				defaultValue: Color.White,
				defaultBindingMode: BindingMode.TwoWay
			);
			public static readonly BindableProperty ContextProperty = BindableProperty.Create (
				propertyName: nameof(Context),
				returnType: typeof (CarouselView),
				declaringType: typeof (ItemView),
				defaultBindingMode: BindingMode.TwoWay
			);

			public ItemView ()
			{

				var change = CreateButton("Change", (items, index) => items[index] = new Moo ());

				var removeBar = new StackLayout {
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = {
						CreateButton ("- Left", (items, index) => items.RemoveAt (index - 1)),
						CreateButton ("Remove", (items, index) => items.RemoveAt (index)),
						CreateButton ("- Right", (items, index) => items.RemoveAt (index + 1)),
					}
				};

				var addBar = new StackLayout {
					Orientation = StackOrientation.Horizontal,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = {
						CreateButton ("+ Left", (items, index) => items.Insert (index, new Moo ())),
						CreateButton ("+ Right", (items, index) => {
							if (index == items.Count - 1)
								items.Add (new Moo ());
							else
								items.Insert (index + 1, new Moo ());
						}),
					}
				};

				var typeNameLabel = new Label () { StyleId = "typename" };
				typeNameLabel.SetBinding (Label.TextProperty, nameof(Item.TypeName));

				var idLabel = new Label () { StyleId = "id", TextColor = Color.White };
				idLabel.SetBinding (Label.TextProperty, nameof(Item.Id));

				Content = new StackLayout {
					Children = {
						typeNameLabel,
						idLabel,
						change,
						removeBar,
						addBar,
					}
				};

				PropertyChanged += (s, e) => {
					if (e.PropertyName == "TextColor")
						typeNameLabel.TextColor = TextColor;
				};
			}

			Button CreateButton(string text, Action<IList<Item>, int> clicked)
			{
				var button = new Button ();
				button.Text = text;
				button.Clicked += (s, e) => {
					var items = (IList<Item>)Context.ItemsSource;
					var index = items.IndexOf (BindingContext);
					clicked (items, index);
				};
				return button;
			}

			public CarouselView Context
			{
				get { return (CarouselView)GetValue (ContextProperty); }
				set { SetValue (ContextProperty, value); }
			}
			public Color TextColor
			{
				get { return (Color)GetValue(TextColorProperty); }
				set { SetValue(TextColorProperty, value); }
			}
		}
		public sealed class MyDataTemplateSelector : DataTemplateSelector
		{
			Dictionary<Type, Color> m_colorByType = new Dictionary<Type, Color> ();
			Dictionary<Type, DataTemplate> m_dataTemplateByType = new Dictionary<Type, DataTemplate> ();

			public MyDataTemplateSelector()
			{
				m_colorByType[typeof (Foo)] = Color.Green;
				m_colorByType[typeof (Bar)] = Color.Red;
			}

			protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
			{
				return OnSelectTemplate (item.GetType (), container);
			}

			DataTemplate OnSelectTemplate (Type itemType, BindableObject container)
			{
				DataTemplate dataTemplate;
				if (!m_dataTemplateByType.TryGetValue(itemType, out dataTemplate))
					m_dataTemplateByType[itemType] = dataTemplate = CreateTemplate(itemType, container);
				return dataTemplate;
			}

			DataTemplate CreateTemplate(Type itemType, BindableObject container)
			{
				var dataTemplate = new DataTemplate (typeof (ItemView));

				Color color;
				if (!m_colorByType.TryGetValue (itemType, out color)) {
					color = Color.Pink;
					dataTemplate.SetValue (BackgroundColorProperty, Color.Black);
				} else {
					dataTemplate.SetValue (BackgroundColorProperty, Color.Blue);
				}

				dataTemplate.SetValue (ItemView.TextColorProperty, color);
				dataTemplate.SetValue (ItemView.ContextProperty, container);
				return dataTemplate;
			}
		}

		static readonly MyDataTemplateSelector Selector = new MyDataTemplateSelector ();

		static readonly IList<Item> Items = new ObservableCollection<Item> () {
			new Baz(),
			new Poo(),
			new Foo(),
			new Bar(),
		};

		Button CreateButton(string text, Action onClicked = null)
		{
			var button = new Button {
				Text = text
			};

			if (onClicked != null)
				button.Clicked += (s, e) => onClicked();

			return button;
		}

		public CarouselViewGallaryPage ()
		{
			BackgroundColor = Color.Blue;

			var logLabel = new Label () { TextColor = Color.White };
			var selectedItemLabel = new Label () { TextColor = Color.White, Text = "0" };
			var selectedPositionLabel = new Label () { TextColor = Color.White, Text = "@0" };
			//var appearingLabel = new Label () { TextColor = Color.White };
			//var disappearingLabel = new Label () { TextColor = Color.White };

			var carouselView = new CarouselView {
				BackgroundColor = Color.Purple,
				ItemsSource = Items,
				ItemTemplate = Selector,
				Position = 1
			};

			bool capture = false;
			carouselView.ItemSelected += (s, o) => {
				var item = (Item)o.SelectedItem;
				selectedItemLabel.Text = $"{item.Id}";
				if (capture)
					logLabel.Text += $"({item.Id}) ";
			};
			carouselView.PositionSelected += (s, o) => {
				var position = (int)o.SelectedPosition;
				selectedPositionLabel.Text = $"@{position}=={carouselView.Position}";
				if (capture)
					logLabel.Text += $"(@{position}) ";
			};
			//carouselView.ItemDisappearing += (s, o) => {
			//	var item = (Item)o.Item;
			//	var id = item.Id;
			//	disappearingLabel.Text = $"-{id}";
			//	if (capture)
			//		logLabel.Text += $"(-{id}) ";
			//};
			//carouselView.ItemAppearing += (s, o) => {
			//	var item = (Item)o.Item;
			//	var id = item.Id;
			//	appearingLabel.Text = $"+{id}";
			//	if (capture)
			//		logLabel.Text += $"(+{id}) ";
			//};

			var moveBar = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					CreateButton ("<<", () => carouselView.Position = 0),
					CreateButton ("<", () => { try { carouselView.Position--; } catch { } }),
					CreateButton (">", () => { try { carouselView.Position++; } catch { } }),
					CreateButton (">>", () => carouselView.Position = Items.Count - 1)
				}
			};

			var statusBar = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Children = {
					selectedItemLabel,
					selectedPositionLabel,
					//disappearingLabel,
					//appearingLabel,
				}
			};

			var logBar = new StackLayout {
				Orientation = StackOrientation.Horizontal,
				Children = {
					CreateButton ("Clear", () => logLabel.Text = ""),
					CreateButton ("On/Off", () => capture = !capture ),
					logLabel,
				}
			};

			Content = new StackLayout {
				Children = {
					carouselView,
					moveBar,
					statusBar,
					logBar
				}
			};
		}

#if UITEST
		//[Test]
		//public void CarouselViewTest ()
		//{
		//	var app = RunningApp;
		//	app.Screenshot ("I am at Issue 1");
		//	app.WaitForElement (q => q.Marked ("Remove"));

		//	app.Screenshot ("I see the Label");
		//	app.SwipeRight ();
		//	app.SwipeLeft ();
		//}
#endif
	}
}
