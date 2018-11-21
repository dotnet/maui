using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;
#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Editor)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1733, "Autoresizable Editor")]
	public class Issue1733 : TestContentPage
	{
		public const string editorHeightShrinkWithPressureId = "editorHeightShrinkWithPressureId";
		public const string editorHeightGrowId = "editorHeightGrowId";
		public const string editorWidthGrow1Id = "editorWidthGrow1Id";
		public const string editorWidthGrow2Id = "editorWidthGrow2Id";
		public const string btnChangeFontToDefault = "Change the Font to Default";
		public const string btnChangeFontToLarger = "Change the Font to Larger";
		public const string btnChangeToHasText = "Change to Has Text";
		public const string btnChangeToNoText = "Change to Has No Text";
		public const string btnChangeSizeOption = "Change the Size Option";

		protected override void Init()
		{
			StackLayout container = new StackLayout()
			{
				BackgroundColor = Color.Purple
			};

			StackLayout layout = new StackLayout()
			{
				BackgroundColor = Color.Pink,
				HeightRequest = 200
			};

			var editor = new Editor()
			{
				BackgroundColor = Color.Green,
				MinimumHeightRequest = 10,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorHeightShrinkWithPressureId
			};

			var editorHeightGrow = new Editor()
			{
				BackgroundColor = Color.Green,
				MinimumHeightRequest = 200,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorHeightGrowId
			};


			layout.Children.Add(editor);
			layout.Children.Add(editorHeightGrow);

			StackLayout layoutHorizontal = new StackLayout()
			{
				BackgroundColor = Color.Yellow,
				Orientation = StackOrientation.Horizontal
			};

			var editorWidthGrow1 = new Editor()
			{
				BackgroundColor = Color.Green,
				MinimumWidthRequest = 10,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorWidthGrow1Id
			};

			var editorWidthGrow2 = new Editor()
			{
				BackgroundColor = Color.Green,
				MinimumWidthRequest = 200,
				AutoSize = EditorAutoSizeOption.TextChanges,
				AutomationId = editorWidthGrow2Id,
				ClassId = editorWidthGrow2Id
			};


			layoutHorizontal.Children.Add(editorWidthGrow1);
			layoutHorizontal.Children.Add(editorWidthGrow2);

			container.Children.Add(layout);
			container.Children.Add(layoutHorizontal);


			List<Editor> editors = new List<Editor>()
			{
				editor, editorHeightGrow, editorWidthGrow1, editorWidthGrow2
			};

			Button buttonChangeFont = new Button()
			{
				Text = btnChangeFontToLarger
			};


			Button buttonChangeText = new Button()
			{
				Text = btnChangeToHasText
			};

			Button buttonChangeSizeOption = new Button()
			{
				Text = btnChangeSizeOption
			};

			double fontSizeInitial = editor.FontSize;
			buttonChangeFont.Clicked += (x, y) =>
			{
				editors.ForEach(e =>
				{
					if (e.FontSize == fontSizeInitial)
					{
						e.FontSize = 40;
						Device.BeginInvokeOnMainThread(() => buttonChangeFont.Text = btnChangeFontToDefault);
					}
					else
					{
						e.FontSize = fontSizeInitial;
						Device.BeginInvokeOnMainThread(() => buttonChangeFont.Text = btnChangeFontToLarger);
					}
				});
			};

			buttonChangeText.Clicked += (_, __) =>
			{
				editors.ForEach(e =>
				{
					if (String.IsNullOrWhiteSpace(e.Text))
					{
						e.Text = String.Join(" ", Enumerable.Range(0, 100).Select(x => "f").ToArray());
						Device.BeginInvokeOnMainThread(() => buttonChangeText.Text = btnChangeToNoText);
					}
					else
					{
						e.Text = String.Empty;
						Device.BeginInvokeOnMainThread(() => buttonChangeText.Text = btnChangeToHasText);
					}
				});
			};

			buttonChangeSizeOption.Clicked += (_, __) =>
			{
				editors.ForEach(e =>
				{
					EditorAutoSizeOption option = EditorAutoSizeOption.TextChanges;
					if (e.AutoSize == option)
						option = EditorAutoSizeOption.Disabled;

					e.AutoSize = option;
				});
			};

			StackLayout commands = new StackLayout();
			commands.Children.Add(buttonChangeFont);
			commands.Children.Add(buttonChangeText);
			commands.Children.Add(buttonChangeSizeOption);


			Label sizeOption = new Label();
			sizeOption.SetBinding(Label.TextProperty, new Binding(nameof(Editor.AutoSize), source: editor));

			container.Children.Add(sizeOption);



			editors.ForEach(e =>
			{
				StackLayout commandLayout = new StackLayout() { Orientation = StackOrientation.Horizontal };
				commands.Children.Add(commandLayout);

				Label automationLabelId = new Label();
				automationLabelId.SetBinding(Label.TextProperty, new Binding(nameof(Editor.AutomationId), source: e));

				Label width = new Label();
				width.SetBinding(Label.TextProperty, new Binding(nameof(Editor.Width), source: e));
				width.AutomationId = e.AutomationId + "_width";

				Label height = new Label();
				height.SetBinding(Label.TextProperty, new Binding(nameof(Editor.Height), source: e));
				height.AutomationId = e.AutomationId + "_height";

				commandLayout.Children.Add(automationLabelId);
				commandLayout.Children.Add(width);
				commandLayout.Children.Add(height);
			});

			StackLayout content = new StackLayout();

			content.Children.Add(new ScrollView() { Content = container });
			content.Children.Add(commands);

			Content = content;
		}

#if UITEST
		Dictionary<string, Size> results = null;

		[Test]
		public void Issue1733Test()
		{
			string[] editors = new string[] { editorHeightShrinkWithPressureId, editorHeightGrowId, editorWidthGrow1Id, editorWidthGrow2Id };
			RunningApp.WaitForElement(q => q.Marked(editorHeightShrinkWithPressureId));

			results = new Dictionary<string, Size>();

			foreach (var editor in editors)
			{
				results.Add(editor, GetDimensions(editor));
			}

			RunningApp.Tap(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeToNoText);
			TestGrowth(false);
			RunningApp.Tap(btnChangeFontToLarger);
			RunningApp.WaitForElement(btnChangeFontToDefault);
			TestGrowth(true);


			// Reset back to being empty and make sure everything sets back to original size
			RunningApp.Tap(btnChangeFontToDefault);
			RunningApp.Tap(btnChangeToNoText);
			RunningApp.WaitForElement(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeFontToLarger);

			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				Assert.AreEqual(allTheSame.Width, results[editor].Width, editor);
				Assert.AreEqual(allTheSame.Height, results[editor].Height, editor);
			}


			// this sets it back to not auto size and we click everything again to see if it grows
			RunningApp.Tap(btnChangeSizeOption);
			RunningApp.Tap(btnChangeFontToLarger);
			RunningApp.Tap(btnChangeToHasText);
			RunningApp.WaitForElement(btnChangeFontToDefault);
			RunningApp.WaitForElement(btnChangeToNoText);
			foreach (var editor in editors)
			{
				var allTheSame = GetDimensions(editor);
				Assert.AreEqual(allTheSame.Width, results[editor].Width, editor);
				Assert.AreEqual(allTheSame.Height, results[editor].Height, editor);
			}


		}

		void TestGrowth(bool heightPressureShrink)
		{
			var testSizes = GetDimensions(editorHeightShrinkWithPressureId);
			Assert.AreEqual(testSizes.Width, results[editorHeightShrinkWithPressureId].Width, editorHeightShrinkWithPressureId);

			if (heightPressureShrink)
				Assert.Less(testSizes.Height, results[editorHeightShrinkWithPressureId].Height, editorHeightShrinkWithPressureId);
			else
				Assert.Greater(testSizes.Height, results[editorHeightShrinkWithPressureId].Height, editorHeightShrinkWithPressureId);

			testSizes = GetDimensions(editorHeightGrowId);
			Assert.AreEqual(testSizes.Width, results[editorHeightGrowId].Width, editorHeightGrowId);
			Assert.Greater(testSizes.Height, results[editorHeightGrowId].Height, editorHeightGrowId);

			var grow1 = GetDimensions(editorWidthGrow1Id);
			Assert.Greater(grow1.Width, results[editorWidthGrow1Id].Width, editorWidthGrow1Id);
			Assert.Greater(grow1.Height, results[editorWidthGrow1Id].Height, editorWidthGrow1Id);

			var grow2 = GetDimensions(editorWidthGrow2Id);
			Assert.Greater(grow2.Width, results[editorWidthGrow2Id].Width, editorWidthGrow2Id);
			Assert.Greater(grow2.Height, results[editorWidthGrow2Id].Height, editorWidthGrow2Id);

			//grow 1 has a lower minimum width request so it's width should be smaller than grow 2
			Assert.Greater(grow2.Width, grow1.Width, "grow2.Width > grow1.Width");
		}

		Size GetDimensions(string editorName)
		{
			RunningApp.WaitForElement($"{editorName}_height");
			RunningApp.WaitForElement($"{editorName}_width");

			var height = RunningApp.Query(x => x.Marked($"{editorName}_height")).FirstOrDefault()?.Text;
			var width = RunningApp.Query(x => x.Marked($"{editorName}_width")).FirstOrDefault()?.Text;

			if (height == null)
			{
				throw new ArgumentException($"{editorName}_height not found");
			}
			if (width == null)
			{
				throw new ArgumentException($"{editorName}_width not found");
			}
			return new Size(Convert.ToDouble(width), Convert.ToDouble(height));
		}

#endif
	}
}
