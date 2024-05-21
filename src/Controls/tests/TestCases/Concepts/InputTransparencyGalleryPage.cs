using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample
{
	internal class InputTransparencyGalleryPage : CoreGalleryBasePage
	{
		protected override void Build()
		{
			// Single Control
			AddButtonNotSet();
			AddButton();
			AddTransButtonInputBlocked();

			// Button
			AddButtonOverlay();
			AddTransButtonOverlay();

			// Image
			AddOverlayTest<Image>(
				Test.InputTransparency.ImageOverlayInputBlocked,
				Test.InputTransparency.TransImageOverlay,
				image => image.Source = ImageSource.FromFile("small_dotnet_bot.png"));
			AddOverlayTest<Image>(
				Test.InputTransparency.ImageBackOverlayInputBlocked,
				Test.InputTransparency.TransImageBackOverlay,
				image =>
				{
					image.Source = ImageSource.FromFile("small_dotnet_bot.png");
					image.Background = Brush.Red;
				});

			// Label
			AddOverlayTest<Label>(
				Test.InputTransparency.LabelOverlayInputBlocked,
				Test.InputTransparency.TransLabelOverlay,
				label => label.Text = "Overlay Text");

			// ActivityIndicator
			AddOverlayTest<ActivityIndicator>(
				Test.InputTransparency.ActivityIndicatorOverlayInputBlocked,
				Test.InputTransparency.TransActivityIndicatorOverlay,
				ai => ai.IsRunning = true);

			// ProgressBar
			AddOverlayTest<ProgressBar>(
				Test.InputTransparency.ProgressBarOverlayInputBlocked,
				Test.InputTransparency.TransProgressBarOverlay,
				p => p.Progress = 0.75);

			// Layout
			AddOverlayTest<Grid>(
				Test.InputTransparency.LayoutOverlayInputBlocked,
				Test.InputTransparency.TransLayoutOverlay);
			AddTransLayoutOverlayWithButton();
			AddCascadeTransLayoutOverlay();
			AddCascadeTransLayoutOverlayWithButton();

			// ListView + CollectionView
			AddItemsViewItemLayoutOverlay();
			AddItemsViewItemTransLayoutOverlay();
			AddItemsViewItemButtonOverlay();
			AddItemsViewItemTransButtonOverlay();

			// The full matrix
			AddFullInputTransparencyMatrix();
		}

		// Basic test with view defaults, should be clickable
		void AddButtonNotSet() =>
			Add(Test.InputTransparency.ButtonNotSet, new Button { Text = "Click Me!" })
				.With(t => t.View.Clicked += (s, e) => t.ReportSuccessEvent());

		// Test when InputTransparent is explicitly set to False, should be clickable
		void AddButton() =>
			Add(Test.InputTransparency.Button, new Button { Text = "Click Me!", InputTransparent = false })
				.With(t => t.View.Clicked += (s, e) => t.ReportSuccessEvent());

		// Test when InputTransparent is explicitly set to True, should NOT be clickable
		void AddTransButtonInputBlocked() =>
			Add(Test.InputTransparency.TransButtonInputBlocked, new Button { Text = "Click Me!", InputTransparent = true })
				.With(t => t.View.Clicked += (s, e) => t.ReportFailEvent());

		// Test when InputTransparent is explicitly set to False, should be clickable
		void AddButtonOverlay() =>
			Add(Test.InputTransparency.ButtonOverlay, () =>
			{
				var bottom = new Button { Text = "Bottom Button" };
				var top = new Button { Text = "Click Me!", InputTransparent = false };
				var grid = new Grid { bottom, top };
				return (grid, new { Bottom = bottom, Top = top });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var bottom = t.Additional.Bottom;
				var top = Annotate(t.Additional.Top, v);
				bottom.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
				top.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
			});

		// Test when InputTransparent is explicitly set to True, should NOT be clickable
		// and we emulate this by putting another button underneath that should be clickable
		void AddTransButtonOverlay() =>
			Add(Test.InputTransparency.TransButtonOverlay, () =>
			{
				var bottom = new Button { Text = "Bottom Button" };
				var top = new Button { Text = "Click Me!", InputTransparent = true };
				var grid = new Grid { bottom, top };
				return (grid, new { Bottom = bottom, Top = top });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var bottom = t.Additional.Bottom;
				var top = Annotate(t.Additional.Top, v);
				bottom.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
				top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
			});

		// Test when there is an InputTransparent layout over the button, should NOT be clickable
		// but the button IN the layout should be clickable because it is not cascading
		void AddTransLayoutOverlayWithButton() =>
			Add(Test.InputTransparency.TransLayoutOverlayWithButton, () =>
			{
				var bottom = new Button { Text = "Bottom Button" };
				var top = new Button { Text = "Click Me!" };
				var grid = new Grid
				{
					new Grid { bottom },
					new Grid
					{
						InputTransparent = true,
						CascadeInputTransparent = false,
						Background = Brush.Red,
						Opacity = 0.5,
						Children = { top },
					}
				};
				return (grid, new { Bottom = bottom, Top = top });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var bottom = t.Additional.Bottom;
				var top = Annotate(t.Additional.Top, v);
				bottom.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
				top.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
			});

		// Test when there is an InputTransparent layout over the button, should be clickable
		void AddCascadeTransLayoutOverlay() =>
			Add(Test.InputTransparency.CascadeTransLayoutOverlay, () =>
			{
				var button = new Button { Text = "Click Me!" };
				var grid = new Grid
				{
					new Grid { button },
					new Grid
					{
						InputTransparent = true,
						CascadeInputTransparent = true,
						Background = Brush.Red,
						Opacity = 0.5,
					}
				};
				return (grid, new { Button = button });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var button = Annotate(t.Additional.Button, v);
				button.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
			});

		// Test when there is an InputTransparent layout over the button, should be clickable
		// and the button IN the layout should NOT be clickable because it is cascading
		void AddCascadeTransLayoutOverlayWithButton() =>
			Add(Test.InputTransparency.CascadeTransLayoutOverlayWithButton, () =>
			{
				var bottom = new Button { Text = "Bottom Button" };
				var top = new Button { Text = "Click Me!" };
				var grid = new Grid
				{
					new Grid { bottom },
					new Grid
					{
						InputTransparent = true,
						CascadeInputTransparent = true,
						Background = Brush.Red,
						Opacity = 0.5,
						Children = { top },
					}
				};
				return (grid, new { Bottom = bottom, Top = top });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var bottom = t.Additional.Bottom;
				var top = Annotate(t.Additional.Top, v);
				bottom.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
				top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
			});

		// CV test where a grid covers/blocks the button, so this causes the CV to select items
		void AddItemsViewItemLayoutOverlay()
		{
			Button CreateBottom() => new Button { Text = "Click Me!" };
			Grid CreateTop() => new Grid { };
			void Setup(ExpectedEventViewContainer<View> vc, ObservableCollection<Button> bottoms, ObservableCollection<Grid> tops)
			{
				var v = vc.View;
				if (v is CollectionView cv)
					cv.SelectionChanged += (s, e) => vc.ReportSuccessEvent();
				else if (v is ListView lv)
					lv.ItemSelected += (s, e) => vc.ReportSuccessEvent();

				bottoms.CollectionChanged += (s, e) =>
				{
					foreach (Button n in e.NewItems)
					{
						Annotate(n, v);
						n.Clicked += (s, e) => vc.ReportFailEvent();
					}
				};
			}

			AddCollectionView(Test.InputTransparency.CollectionViewItemLayoutOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));

			AddListView(Test.InputTransparency.ListViewItemLayoutOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));
		}

		// CV test where a transparent grid covers the button, so this allows button clicks
		void AddItemsViewItemTransLayoutOverlay()
		{
			Button CreateBottom() => new Button { Text = "Click Me!" };
			Grid CreateTop() => new Grid { InputTransparent = true };
			void Setup(ExpectedEventViewContainer<View> vc, ObservableCollection<Button> bottoms, ObservableCollection<Grid> tops)
			{
				var v = vc.View;
				if (v is CollectionView cv)
					cv.SelectionChanged += (s, e) => vc.ReportFailEvent();
				else if (v is ListView lv)
					lv.ItemSelected += (s, e) => vc.ReportFailEvent();

				bottoms.CollectionChanged += (s, e) =>
				{
					foreach (Button n in e.NewItems)
					{
						Annotate(n, v);
						n.Clicked += (s, e) => vc.ReportSuccessEvent();
					}
				};
			}

			AddCollectionView(Test.InputTransparency.CollectionViewItemTransLayoutOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));

			AddListView(Test.InputTransparency.ListViewItemTransLayoutOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));
		}

		// CV test where a button is the item so the CV is not selectable
		void AddItemsViewItemButtonOverlay()
		{
			Grid CreateBottom() => new Grid { };
			Button CreateTop() => new Button { Text = "Click Me!" };
			void Setup(ExpectedEventViewContainer<View> vc, ObservableCollection<Grid> bottoms, ObservableCollection<Button> tops)
			{
				var v = vc.View;
				if (v is CollectionView cv)
					cv.SelectionChanged += (s, e) => vc.ReportFailEvent();
				else if (v is ListView lv)
					lv.ItemSelected += (s, e) => vc.ReportFailEvent();

				tops.CollectionChanged += (s, e) =>
				{
					foreach (Button n in e.NewItems)
					{
						Annotate(n, v);
						n.Clicked += (s, e) => vc.ReportSuccessEvent();
					}
				};
			};

			AddCollectionView(Test.InputTransparency.CollectionViewItemButtonOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));

			AddListView(Test.InputTransparency.ListViewItemButtonOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));
		}

		// CV test where a transparent button is the item so the CV is selectable
		void AddItemsViewItemTransButtonOverlay()
		{
			Grid CreateBottom() => new Grid { };
			Button CreateTop() => new Button { Text = "Click Me!", InputTransparent = true };
			void Setup(ExpectedEventViewContainer<View> vc, ObservableCollection<Grid> bottoms, ObservableCollection<Button> tops)
			{
				var v = vc.View;
				if (v is CollectionView cv)
					cv.SelectionChanged += (s, e) => vc.ReportSuccessEvent();
				else if (v is ListView lv)
					lv.ItemSelected += (s, e) => vc.ReportSuccessEvent();

				tops.CollectionChanged += (s, e) =>
				{
					foreach (Button n in e.NewItems)
					{
						Annotate(n, v);
						n.Clicked += (s, e) => vc.ReportFailEvent();
					}
				};
			}

			AddCollectionView(Test.InputTransparency.CollectionViewItemTransButtonOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));

			AddListView(Test.InputTransparency.ListViewItemTransButtonOverlay, CreateBottom, CreateTop)
				.With(t => Setup(t.ViewContainer, t.Bottoms, t.Tops));
		}

		void AddOverlayTest<T>(Test.InputTransparency test, Test.InputTransparency transTest, Action<T> setup = null)
			where T : View, new()
		{
			// Test when there is an T over the button, should NOT be clickable
			Add(test, () =>
			{
				var button = new Button { Text = "Click Me!" };
				var view = new T { InputTransparent = false };
				setup?.Invoke(view);
				var grid = new Grid
				{
					new Grid { button },
					view
				};
				return (grid, new { Button = button });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var button = Annotate(t.Additional.Button, v);
				button.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
			});

			// Test when there is a transparent T over the button, should be clickable
			Add(transTest, () =>
			{
				var button = new Button { Text = "Click Me!" };
				var view = new T { InputTransparent = true };
				setup?.Invoke(view);
				var grid = new Grid
				{
					new Grid { button },
					view
				};
				return (grid, new { Button = button });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var button = Annotate(t.Additional.Button, v);
				button.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
			});
		}

		// Tests for a nested layout (root grid, nested grid, button) with some variations to ensure
		// that all combinations are correctly clickable
		void AddFullInputTransparencyMatrix()
		{
			foreach (var state in Test.InputTransparencyMatrix.States)
			{
				var (rt, rc, nt, nc, t) = state.Key;
				var (clickable, passthru) = state.Value;

				AddNesting(rt, rc, nt, nc, t, clickable, passthru);
			}
		}

		void AddNesting(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans, bool isClickable, bool isPassThru) =>
			Add(Test.InputTransparencyMatrix.GetKey(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, isClickable, isPassThru), () =>
			{
				var bottom = new Button { Text = "Bottom Button" };
				var top = new Button
				{
					InputTransparent = trans,
					Text = "Click Me!"
				};
				var grid = new Grid
				{
					new Grid { bottom },
					new Grid
					{
						InputTransparent = rootTrans,
						CascadeInputTransparent = rootCascade,
						Children =
						{
							new Grid
							{
								InputTransparent = nestedTrans,
								CascadeInputTransparent = nestedCascade,
								Children = { top }
							}
						},
					}
				};
				return (grid, new { Bottom = bottom, Top = top });
			})
			.With(t =>
			{
				var v = t.ViewContainer.View;
				var bottom = t.Additional.Bottom;
				var top = Annotate(t.Additional.Top, v);
				if (isClickable)
				{
					// if the button is clickable, then it should be clickable
					bottom.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
					top.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
				}
				else if (!isPassThru)
				{
					// if one of the parent layouts are NOT transparent, then
					// the tap should NOT go through to the bottom button
					bottom.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
					top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
				}
				else
				{
					// otherwise, the tap should go through
					bottom.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
					top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
				}
			});

		(ExpectedEventViewContainer<View> ViewContainer, T Additional) Add<T>(Test.InputTransparency test, Func<(View View, T Additional)> func) =>
			Add(test.ToString(), func);

		(ExpectedEventViewContainer<View> ViewContainer, T Additional) Add<T>(string test, Func<(View View, T Additional)> func)
		{
			var result = func();
			var vc = new ExpectedEventViewContainer<View>(test, result.View);
			Add(vc);
			return (vc, result.Additional);
		}

		ExpectedEventViewContainer<Button> Add(Test.InputTransparency test, Button button) =>
			Add(new ExpectedEventViewContainer<Button>(test, button));

		(ExpectedEventViewContainer<View> ViewContainer, ObservableCollection<TBottom> Bottoms, ObservableCollection<TTop> Tops)
			AddCollectionView<TBottom, TTop>(Test.InputTransparency test, Func<TBottom> createBottom, Func<TTop> createTop)
			where TBottom : View
			where TTop : View
		{
			// NOTE: the CV height needs to be 3x the item height because some
			// platforms tap the CV itself. This ensures the center of the CV is
			// over the second item.
			var added = Add(test.ToString(), () =>
			{
				var bottoms = new ObservableCollection<TBottom>();
				var tops = new ObservableCollection<TTop>();
				var cv = new CollectionView
				{
					SelectionMode = SelectionMode.Single,
					HeightRequest = 150, // 3x the item height
					ItemsSource = new[] { "First Item", "Second Item", "Third Item" },
					ItemTemplate = new DataTemplate(() =>
					{
						var bottom = createBottom();
						if (bottom is not null)
							bottoms.Add(bottom);

						var top = createTop();
						if (top is not null)
							tops.Add(top);

						var grid = new Grid
						{
							HeightRequest = 50,
							Children =
							{
								new Grid { bottom },
								top
							}
						};
						return grid;
					})
				};
				return (cv, new { Bottoms = bottoms, Tops = tops });
			});
			return (added.ViewContainer, added.Additional.Bottoms, added.Additional.Tops);
		}

		(ExpectedEventViewContainer<View> ViewContainer, ObservableCollection<TBottom> Bottoms, ObservableCollection<TTop> Tops)
			AddListView<TBottom, TTop>(Test.InputTransparency test, Func<TBottom> createBottom, Func<TTop> createTop)
			where TBottom : View
			where TTop : View
		{
			// NOTE: the LV height needs to be 3x the item height because some
			// platforms tap the LV itself. This ensures the center of the LV is
			// over the second item.
			var added = Add(test.ToString(), () =>
			{
				var bottoms = new ObservableCollection<TBottom>();
				var tops = new ObservableCollection<TTop>();
				var lv = new ListView
				{
					SelectionMode = ListViewSelectionMode.Single,
					HeightRequest = 150, // 3x the item height
					ItemsSource = new[] { "First Item", "Second Item", "Third Item" },
					ItemTemplate = new DataTemplate(() =>
					{
						var bottom = createBottom();
						if (bottom is not null)
							bottoms.Add(bottom);

						var top = createTop();
						if (top is not null)
							tops.Add(top);

						var grid = new Grid
						{
							HeightRequest = 50,
							Children =
							{
								new Grid { bottom },
								top
							}
						};
						return new ViewCell { View = grid };
					})
				};
				return (lv, new { Bottoms = bottoms, Tops = tops });
			});
			return (added.ViewContainer, added.Additional.Bottoms, added.Additional.Tops);
		}

		static T Annotate<T>(T view, View desired)
			where T : View
		{
#if WINDOWS
			// Windows does not have layouts in the automation tree
			// and some of the tests have the layout as the root
			view.AutomationId = desired.AutomationId;
#endif
			return view;
		}
	}
}
