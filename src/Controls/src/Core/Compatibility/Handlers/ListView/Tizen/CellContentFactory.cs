#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
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
				return new Controls.StackLayout
				{
					BackgroundColor = Colors.Red
				};
			}
		}

		static View CreateContent(SectionCell sectionCell)
		{
			var text = new Label
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				FontAttributes = FontAttributes.Bold,
				Margin = new Thickness(10, 0),
#pragma warning disable CS0612 // Type or member is obsolete
				FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
#pragma warning restore CS0612 // Type or member is obsolete
			};

			text.SetBinding(Label.TextProperty, new Binding("Text", source: sectionCell));
			text.SetBinding(Label.TextColorProperty, new Binding("TextColor", source: sectionCell));

			var layout = new Controls.StackLayout
			{
				HorizontalOptions = LayoutOptions.Fill,
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
#pragma warning disable CS0612 // Type or member is obsolete
			text.FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label));
#pragma warning restore CS0612 // Type or member is obsolete

			var detail = new Label();
			detail.SetBinding(Label.TextProperty, new Binding("Detail", source: textcell));
			detail.SetBinding(Label.TextColorProperty, new Binding("DetailColor", source: textcell));
#pragma warning disable CS0612 // Type or member is obsolete
			detail.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
#pragma warning restore CS0612 // Type or member is obsolete
			detail.Margin = new Thickness(10, 0, 0, 0);

			var layout = new Controls.StackLayout
			{
				Spacing = 0,
				HorizontalOptions = LayoutOptions.Fill,
				Margin = new Thickness(10, 5),
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
			textcell.HorizontalOptions = LayoutOptions.Fill;
			var layout = new Controls.Grid
			{
				HorizontalOptions = LayoutOptions.Fill,
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Auto),
					new ColumnDefinition(GridLength.Star)
				},
				Margin = new Thickness(10, 5),
			};
			var img = new Image
			{
				Aspect = Aspect.AspectFit,
			};
			img.SetBinding(Image.SourceProperty, new Binding("ImageSource", source: imageCell));
			layout.Add(img, 0, 0);
			layout.Add(textcell, 1, 0);
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
#pragma warning disable CS0612 // Type or member is obsolete
			label.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
#pragma warning restore CS0612 // Type or member is obsolete
			var layout = new Controls.StackLayout
			{
				Margin = new Thickness(10, 5),
				HorizontalOptions = LayoutOptions.Fill,
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
#pragma warning disable CS0618
			var text = new Label
			{
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalOptions = LayoutOptions.StartAndExpand,
			};
#pragma warning restore CS0618
			text.SetBinding(Label.TextProperty, new Binding("Text", source: switchCell));

			var sw = new Switch
			{
				HorizontalOptions = LayoutOptions.End
			};
			sw.SetBinding(Switch.IsToggledProperty, new Binding("On", BindingMode.TwoWay, source: switchCell));
			sw.SetBinding(Switch.OnColorProperty, new Binding("OnColor", source: switchCell));

			var layout = new Controls.StackLayout
			{
				Margin = new Thickness(10, 5),
				HorizontalOptions = LayoutOptions.Fill,
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
				BackgroundColor = Colors.Transparent;
				var separator = new BoxView
				{
					Margin = new Thickness(10, 0),
					Color = Color.FromRgb(120, 120, 120),
					HeightRequest = 1,
				};

				var content = new ContentPresenter
				{
					VerticalOptions = LayoutOptions.Fill,
					HorizontalOptions = LayoutOptions.Fill
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
					var layout = new Controls.StackLayout
					{
						Spacing = 0,
						VerticalOptions = LayoutOptions.Fill,
						HorizontalOptions = LayoutOptions.Fill,
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

				stateGroup.States.Add(normal);
				stateGroup.States.Add(focused);
				stateGroup.States.Add(selected);
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
