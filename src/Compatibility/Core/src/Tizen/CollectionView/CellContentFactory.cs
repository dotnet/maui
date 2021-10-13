using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class SectionCell : TextCell { }

	public class CellContentFactory
	{
		public static View CreateContent(object data, BindableObject container = null)
		{
			if (data is ImageCell imageCell)
			{
				return new CellContentView(imageCell, container)
				{
					Content = CreateContent(imageCell)
				};
			}
			else if (data is SectionCell sectionCell)
			{
				return new CellContentView(sectionCell, container, false, false)
				{
					Content = CreateContent(sectionCell)
				};
			}
			else if (data is EntryCell entryCell)
			{
				return new CellContentView(entryCell, container)
				{
					Content = CreateContent(entryCell)
				};
			}
			else if (data is TextCell textCell)
			{
				return new CellContentView(textCell, container)
				{
					Content = CreateContent(textCell)
				};
			}
			else if (data is ViewCell viewCell)
			{
				return new CellContentView(viewCell, container, false)
				{
					Content = CreateContent(viewCell)
				};
			}
			else if (data is SwitchCell switchCell)
			{
				return new CellContentView(switchCell, container)
				{
					Content = CreateContent(switchCell)
				};
			}
			else
			{
				return new StackLayout
				{
					BackgroundColor = Colors.Red
				};
			}
		}

		static View CreateContent(SectionCell sectionCell)
		{
			var text = new Label
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				FontAttributes = FontAttributes.Bold,
				Margin = new Thickness(10, 0),
				FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
			};

			text.SetBinding(Label.TextProperty, new Binding("Text", source: sectionCell));
			text.SetBinding(Label.TextColorProperty, new Binding("TextColor", source: sectionCell));

			var layout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.FromArgb("#e3f2fd"),
				Padding = 5,
				Children =
				{
					text,
				}
			};
			return layout;
		}

		static View CreateContent(TextCell textcell)
		{
			var text = new Label();
			text.SetBinding(Label.TextProperty, new Binding("Text", source: textcell));
			text.SetBinding(Label.TextColorProperty, new Binding("TextColor", source: textcell));
			text.FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label));

			var detail = new Label();
			detail.SetBinding(Label.TextProperty, new Binding("Detail", source: textcell));
			detail.SetBinding(Label.TextColorProperty, new Binding("DetailColor", source: textcell));
			detail.FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) / 2;
			detail.Margin = new Thickness(10, 0, 0, 0);

			var layout = new StackLayout
			{
				Spacing = 0,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness(10, 5),
				Children =
				{
					text,
					detail,
				}
			};
			return layout;
		}

		static View CreateContent(ImageCell imageCell)
		{
			var textcell = CreateContent((TextCell)imageCell);
			textcell.HorizontalOptions = LayoutOptions.FillAndExpand;
			var layout = new Grid
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				ColumnDefinitions =
				{
					new ColumnDefinition
					{
						Width = new GridLength(2, GridUnitType.Star)
					},
					new ColumnDefinition
					{
						Width = new GridLength(8, GridUnitType.Star)
					},
				}
			};
			var img = new Image
			{
				HorizontalOptions = LayoutOptions.Start,
				Aspect = Aspect.AspectFit,
			};
			img.SetBinding(Image.SourceProperty, new Binding("ImageSource", source: imageCell));
			layout.Children.Add(img, 0, 0);
			layout.Children.Add(textcell, 1, 0);
			return layout;
		}

		static View CreateContent(EntryCell entryCell)
		{
			var entry = new Entry();
			entry.SetBinding(Entry.TextProperty, new Binding("Text", BindingMode.TwoWay, source: entryCell));
			entry.SetBinding(Entry.PlaceholderProperty, new Binding("Placeholder", source: entryCell));
			entry.SetBinding(InputView.KeyboardProperty, new Binding("Keyboard", source: entryCell));
			entry.SetBinding(Entry.HorizontalTextAlignmentProperty, new Binding("HorizontalTextAlignment", source: entryCell));
			entry.SetBinding(Entry.VerticalTextAlignmentProperty, new Binding("VerticalTextAlignment", source: entryCell));

			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("Label", source: entryCell));
			label.SetBinding(Label.TextColorProperty, new Binding("LabelColor", source: entryCell));
			label.FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)) / 2;
			label.Margin = new Thickness(20, 0, 0, 0);
			var layout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					label,
					entry,
				}
			};

			return layout;
		}

		static View CreateContent(SwitchCell switchCell)
		{
			var text = new Label
			{
				HorizontalOptions = LayoutOptions.Start
			};
			text.SetBinding(Label.TextProperty, new Binding("Text", source: switchCell));

			var sw = new Switch
			{
				HorizontalOptions = LayoutOptions.End
			};
			sw.SetBinding(Switch.IsToggledProperty, new Binding("On", BindingMode.TwoWay, source: switchCell));
			sw.SetBinding(Switch.OnColorProperty, new Binding("OnColor", source: switchCell));

			var layout = new StackLayout
			{
				Padding = new Thickness(10, 5),
				Spacing = 0,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Horizontal,
				Children =
				{
					text,
					sw
				}
			};
			return layout;
		}

		static View CreateContent(ViewCell viewCell)
		{
			return viewCell.View;
		}

		class CellContentView : ContentView
		{
			BindableObject _target;
			public CellContentView(BindableObject target, BindableObject container = null, bool hasVisualState = true, bool hasSeparator = true)
			{
				var separator = new BoxView
				{
					Margin = new Thickness(10, 0),
					Color = Color.FromRgb(120, 120, 120),
					HeightRequest = 1,
				};

				var content = new ContentPresenter
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand
				};


				if (container is ListView listview)
				{
					if (listview.SeparatorVisibility == SeparatorVisibility.None)
					{
						separator.IsVisible = false;
					}
					if (!listview.SeparatorColor.IsDefault())
					{
						separator.Color = listview.SeparatorColor;
					}
					if (listview.RowHeight > 0 && !listview.HasUnevenRows)
					{
						content.HeightRequest = listview.RowHeight;
					}
				}

				if (separator.IsVisible && !hasSeparator)
				{
					separator.IsVisible = false;
				}


				ControlTemplate = new ControlTemplate(() =>
				{
					var layout = new StackLayout
					{
						Spacing = 0,
						VerticalOptions = LayoutOptions.FillAndExpand,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Children =
						{
							content,
							separator
						}
					};
					return layout;
				});
				_target = target;
				if (hasVisualState)
					SetupVisualState();
			}

			void SetupVisualState()
			{
				VisualStateGroup stateGroup = new VisualStateGroup();
				var selected = new VisualState
				{
					Name = VisualStateManager.CommonStates.Selected,
					TargetType = typeof(View),
					Setters =
					{
						new Setter
						{
							Property = View.BackgroundColorProperty,
							Value = Color.FromArgb("#f9fbe7"),
						}
					}
				};

				var normal = new VisualState
				{
					Name = VisualStateManager.CommonStates.Normal,
					TargetType = typeof(View),
					Setters =
					{
						new Setter
						{
							Property = View.BackgroundColorProperty,
							Value = Colors.Transparent
						}
					}
				};
				var focused = new VisualState
				{
					Name = VisualStateManager.CommonStates.Focused,
					TargetType = typeof(View),
					Setters =
					{
						new Setter
						{
							Property = View.BackgroundColorProperty,
							Value = Color.FromArgb("#eeeeeeee")
						}
					}
				};

				var unfocused = new VisualState
				{
					Name = VisualStateManager.CommonStates.Normal,
					TargetType = typeof(View),
					Setters =
					{
						new Setter
						{
							Property = View.BackgroundColorProperty,
							Value = Colors.Transparent
						}
					}
				};
				stateGroup.States.Add(focused);
				stateGroup.States.Add(selected);
				stateGroup.States.Add(unfocused);
				VisualStateManager.GetVisualStateGroups(this).Add(stateGroup);
			}

			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();
				_target.BindingContext = BindingContext;
			}
		}
	}
}
