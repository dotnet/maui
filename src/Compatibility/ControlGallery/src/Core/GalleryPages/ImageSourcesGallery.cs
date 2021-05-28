using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class ImageSourcesGallery : NavigationPage
	{
		public ImageSourcesGallery()
			: base(new RootPage())
		{
		}

		static Picker CreateImageSourcePicker(string title, Action<Func<ImageSource>> onSelected)
		{
			var items = new[]
			{
				new ImageSourcePickerItem
				{
					Text = "<none>",
					Getter = () => null
				},
				new ImageSourcePickerItem
				{
					Text = "App Resource",
					Getter = () => ImageSource.FromFile("bank.png")
				},
				new ImageSourcePickerItem
				{
					Text = "Embedded",
					Getter = () => ImageSource.FromResource("Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.crimson.jpg", typeof(App))
				},
				new ImageSourcePickerItem
				{
					Text = "Stream",
					Getter = () => ImageSource.FromStream(() => typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.Maui.Controls.Compatibility.ControlGallery.coffee.png"))
				},
				new ImageSourcePickerItem
				{
					Text = "URI",
					Getter = () => new UriImageSource
					{
						Uri = new Uri("https://beehive.blob.core.windows.net/staticimages/FeatureImages/MutantLizard01.png"),
						CachingEnabled = false
					}
				},
				new ImageSourcePickerItem
				{
					Text = "Font Glyph",
					Getter = () =>
					{
						var fontFamily = "";
						switch (Device.RuntimePlatform)
						{
							case Device.iOS:
								fontFamily = "Ionicons";
								break;
							case Device.UWP:
								fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
								break;
							case Device.Android:
							default:
								fontFamily = "fonts/ionicons.ttf#";
								break;
						}
						return new FontImageSource
						{
							Color = Colors.Black,
							FontFamily = fontFamily,
							Glyph = "\uf233",
							Size = 24,
						};
					}
				},
			};

			var picker = new Picker
			{
				Title = title,
				ItemsSource = items,
				ItemDisplayBinding = new Binding("Text"),
			};

			picker.SelectedIndexChanged += (sender, e) =>
			{
				var item = (ImageSourcePickerItem)picker.SelectedItem;
				var text = item.Text;
				onSelected?.Invoke(item.Getter);
			};

			return picker;
		}

		class ImageSourcePickerItem
		{
			public string Text { get; set; }

			public Func<ImageSource> Getter { get; set; }
		}

		new class RootPage : ContentPage
		{
			public RootPage()
			{
				Title = "Image Source Tests";

				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Padding = 20,
						Spacing = 10,
						Children =
						{
							CreateImageSourcePicker("Change Title Icon", getter => NavigationPage.SetTitleIconImageSource(this, getter())),
							new Button
							{
								Text = "Page & Toolbar",
								Command = new Command(() => Navigation.PushAsync(new PagePropertiesPage()))
							},
							new Button
							{
								Text = "ListView & Context Actions",
								Command = new Command(() => Navigation.PushAsync(new ListViewContextActionsPage()))
							},
							new Button
							{
								Text = "Image View",
								Command = new Command(() => Navigation.PushAsync(new ImageViewPage()))
							},
							new Button
							{
								Text = "Buttons",
								Command = new Command(() => Navigation.PushAsync(new ButtonsPage()))
							},
							new Button
							{
								Text = "Slider",
								Command = new Command(() => Navigation.PushAsync(new SliderPage()))
							},
						}
					}
				};
			}
		}

		class PagePropertiesPage : TabbedPage
		{
			public PagePropertiesPage()
			{
				Title = "Page & Toolbar";

				Children.Add(new TabPage { Title = "Tab 1" });
				Children.Add(new TabPage { Title = "Tab 2" });
				Children.Add(new TabPage { Title = "Tab 3" });
			}

			class TabPage : ContentPage
			{
				ToolbarItem _toolbarItem;

				public TabPage()
				{
					ToolbarItems.Add(_toolbarItem = new ToolbarItem("MENU", null, delegate
					{
					}));

					Content = new ScrollView
					{
						Content = new StackLayout
						{
							Padding = 20,
							Spacing = 10,
							Children =
							{
								CreateImageSourcePicker("Change Tab Icon", getter => IconImageSource = getter()),
								CreateImageSourcePicker("Change Toolbar Icon", getter => _toolbarItem.IconImageSource = getter()),
								CreateImageSourcePicker("Change Background", getter => BackgroundImageSource = getter()),
							}
						}
					};
				}
			}
		}

		class ListViewContextActionsPage : ContentPage
		{
			ListView _listView;
			string[] _items = new[] { "one", "two", "three", "four", "five" };

			public ListViewContextActionsPage()
			{
				Title = "ListView & Context Actions";

				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Padding = 20,
						Spacing = 10,
						Children =
						{
							new Label
							{
								Text = "Select the item source from the picker and then view the context menu of each item.",
								LineBreakMode = LineBreakMode.WordWrap,
							},
							CreateImageSourcePicker("Select Icon Source", getter =>
							{
								_listView.ItemsSource = null;
								_listView.ItemsSource = CreateDataItems(getter);
							}),
							(_listView = new ListView
							{
								Margin = new Thickness(-20, 0, -20, -20),
								ItemsSource = CreateDataItems(),
								ItemTemplate = new DataTemplate(() =>
								{
									var menuItem = new MenuItem();
									menuItem.SetBinding(MenuItem.TextProperty, new Binding(nameof(ListItem.Text)));
									menuItem.SetBinding(MenuItem.IconImageSourceProperty, new Binding(nameof(ListItem.ContextImage)));

									var cell = new ImageCell();
									cell.ContextActions.Add(menuItem);
									cell.SetBinding(ImageCell.TextProperty, new Binding(nameof(ListItem.Text)));
									cell.SetBinding(ImageCell.ImageSourceProperty, new Binding(nameof(ListItem.Image)));

									return cell;
								}),
							})
						}
					}
				};
			}

			IEnumerable<ListItem> CreateDataItems(Func<ImageSource> getter = null)
			{
				return _items.Select(i => new ListItem
				{
					Text = i,
					Image = getter?.Invoke(),
					ContextImage = getter?.Invoke(),
				});
			}

			class ListItem
			{
				public string Text { get; set; }

				public ImageSource Image { get; set; }

				public ImageSource ContextImage { get; set; }
			}
		}

		class ImageViewPage : ContentPage
		{
			Image _image = null;
			Image _imageAutosize = null;
			ActivityIndicator _loading = null;

			public ImageViewPage()
			{
				Title = "Image View";

				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Padding = 20,
						Spacing = 10,
						Children =
						{
							CreateImageSourcePicker("Select Image Source", getter =>
							{
								_image.Source = getter();
								_imageAutosize.Source = getter();
							}),
							new StackLayout
							{
								Children =
								{
									new Grid
									{
										Children =
										{
											(_image = new Image
											{
												WidthRequest = 200,
												HeightRequest = 200,
												Source = "bank.png"
											}),
											(_loading = new ActivityIndicator
											{
												WidthRequest = 100,
												HeightRequest = 100
											}),
										}
									},
									(_imageAutosize = new Image
									{
										Source = "bank.png"
									}),
								}
							}
						}
					}
				};

				_loading.SetBinding(ActivityIndicator.IsRunningProperty, new Binding(Image.IsLoadingProperty.PropertyName));
				_loading.SetBinding(ActivityIndicator.IsVisibleProperty, new Binding(Image.IsLoadingProperty.PropertyName));
				_loading.BindingContext = _image;
			}
		}

		class ButtonsPage : ContentPage
		{
			Button _buttonWithImageAndText;
			Button _buttonWithPositionedImageAndText;
			Button _buttonWithImage;
			ImageButton _imageButton;

			public ButtonsPage()
			{
				Title = "Buttons";

				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Padding = 20,
						Spacing = 10,
						Children =
						{
							CreateImageSourcePicker("Select Image Source", getter =>
							{
								_buttonWithImageAndText.ImageSource = getter();
								_buttonWithPositionedImageAndText.ImageSource = getter();
								_buttonWithImage.ImageSource = getter();
								_imageButton.Source = getter();
							}),
							new Label
							{
								Text = "The default Button type.",
								LineBreakMode = LineBreakMode.WordWrap,
							},
							(_buttonWithImageAndText = new Button
							{
								Text = "Image & Text",
								ImageSource = "bank.png"
							}),
							(_buttonWithPositionedImageAndText = new Button
							{
								Text = "Image Above & Text",
								ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 5),
								ImageSource = "bank.png"
							}),
							(_buttonWithImage = new Button
							{
								ImageSource = "bank.png"
							}),
							new Button
							{
								Text = "Just Text",
								ImageSource = null
							},
							new Label
							{
								Text = "The ImageButton type.",
								LineBreakMode = LineBreakMode.WordWrap,
							},
							(_imageButton = new ImageButton
							{
								Padding = 10,
								Source = "bank.png",
							}),
						}
					}
				};
			}
		}

		class SliderPage : ContentPage
		{
			Slider _slider = null;

			public SliderPage()
			{
				Title = "Slider";

				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Padding = 20,
						Spacing = 10,
						Children =
						{
							CreateImageSourcePicker("Select Image Source", getter => _slider.ThumbImageSource = getter()),
							(_slider = new Slider
							{
								Minimum = 0,
								Maximum = 1,
								Value = 0.5,
								HeightRequest = 50
							}),
						}
					}
				};
			}
		}
	}
}
