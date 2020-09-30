using System.ComponentModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8644, "WPF Entry crashes when IsPassword=true", PlatformAffected.WPF)]
	public class Issue8644 : TestContentPage
	{
		public class BinCon : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;
			string _title;
			public string Title
			{
				get => _title;
				set
				{
					_title = value?.Length > 4 ? value.Substring(0, 4) : value;
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
				}
			}

		}
		protected override void Init()
		{
			var bc = new BinCon();
			var e1 = new Entry
			{
				BindingContext = bc,
				Margin = new Thickness(50),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				IsPassword = true,
			};
			e1.SetBinding(Entry.TextProperty, nameof(BinCon.Title));

			// Label just to show current Entry text, not needed for test
			var lbl = new Label { BindingContext = bc };
			lbl.SetBinding(Label.TextProperty, nameof(BinCon.Title));
			var stack = new StackLayout
			{

				Children = {
					new Label { Text = "Type more than 4 symbols" },
					e1,
					lbl,
				}
			};

			Content = stack;
		}
	}
}