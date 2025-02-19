using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8167, "[Bug] XF 4.3 UWP Crash - Element not found", PlatformAffected.UWP)]
	public class Issue8167 : TestContentPage
	{
		const string Run = "Update Text";
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = $"Tap the button marked {Run}. If the Label below reads {Success} then the test has passed."
			};

			layout.Children.Add(instructions);

			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding(nameof(_8167ViewModel.Text)));

			layout.Children.Add(label);

			var presenter = new ContentPresenter();
			presenter.SetBinding(ContentPresenter.WidthRequestProperty, new Binding(nameof(_8167ViewModel.Width)));

			layout.Children.Add(presenter);

			var model = new _8167ViewModel();

			var button = new Button() { Text = Run };

			button.Clicked += (obj, args) =>
			{
				model.UpdateText();
				model.UpdateWidth();
			};

			layout.Children.Add(button);

			Content = layout;

			BindingContext = model;
		}

		[Preserve(AllMembers = true)]
		public class _8167ViewModel : INotifyPropertyChanged
		{
			string _text;
			double _width;

			public _8167ViewModel()
			{
				_text = "Starting value";
			}

			public void UpdateText()
			{
				Task.Run(() => { Text = Success; });
			}

			public void UpdateWidth()
			{
				Task.Run(() => { Width = 200; });
			}

			public string Text
			{
				get => _text;
				set
				{
					_text = value;
					RaisePropertyChanged(nameof(Text));
				}
			}

			public double Width
			{
				get => _width;
				set
				{
					_width = value;
					RaisePropertyChanged(nameof(Width));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			void RaisePropertyChanged(string propertyName)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void ThreadpoolBindingUpdateShouldNotCrash()
		{
			RunningApp.WaitForElement(Run);
			RunningApp.Tap(Run);
			RunningApp.WaitForElement(Success);
		}
#endif
	}


}
