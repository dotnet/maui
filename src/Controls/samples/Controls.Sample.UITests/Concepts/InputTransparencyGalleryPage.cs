using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class InputTransparencyGalleryPage : CoreGalleryBasePage
	{
		protected override void Build()
		{
			// Basic test with view defaults, should be clickable
			Add(Test.InputTransparency.Default, new Button { Text = "Click Me!" })
				.With(t => t.View.Clicked += (s, e) => t.ReportSuccessEvent());

			// Test when InputTransparent is explicitly set to False, should be clickable
			Add(Test.InputTransparency.IsFalse, new Button { Text = "Click Me!", InputTransparent = false })
				.With(t => t.View.Clicked += (s, e) => t.ReportSuccessEvent());

			// Test when InputTransparent is explicitly set to True, should NOT be clickable
			// and we emulate this by putting another button underneath that should be clickable
			Add(Test.InputTransparency.IsTrue, () =>
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

			// Test when there is an InputTransparent layout over the button, should be clickable
			Add(Test.InputTransparency.TransLayoutOverlay, () =>
			{
				var button = new Button { Text = "Click Me!" };
				var grid = new Grid
				{
					new Grid { button },
					new Grid
					{
						InputTransparent = true,
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

			// Test when there is an InputTransparent layout over the button, should NOT be clickable
			// but the button IN the layout should be clickable because it is not cascading
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

			// Tests for a nested layout (root grid, nested grid, button) with some variations to ensure
			// that all combinations are correctly clickable
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
#if ANDROID
					// TODO: Android is broken with everything passing through
					// https://github.com/dotnet/maui/issues/10252
					bottom.Clicked += (s, e) => t.ViewContainer.ReportSuccessEvent();
					top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
#else
					bottom.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
					top.Clicked += (s, e) => t.ViewContainer.ReportFailEvent();
#endif
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
