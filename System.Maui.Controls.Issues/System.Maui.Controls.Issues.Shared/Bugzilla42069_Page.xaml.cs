using System;
using System.Diagnostics;
using System.Threading;

namespace Xamarin.Forms.Controls.Issues
{
	public partial class Bugzilla42069_Page : ContentPage
	{
		public const string DestructorMessage = ">>>>>>>>>> Bugzilla42069_Page destructor <<<<<<<<<<";

		public Bugzilla42069_Page()
		{
			#if APP
			InitializeComponent();
			
			ImageWhichChanges = ImageSource.FromFile("oasissmall.jpg") as FileImageSource;

			ChangingImage.SetBinding(Image.SourceProperty, nameof(ImageWhichChanges));

			Button.Clicked += (sender, args) => Navigation.PopAsync(false);

			Button2.Clicked += (sender, args) =>
			{
				ImageWhichChanges.File = ImageWhichChanges.File == "bank.png" ? "oasissmall.jpg" : "bank.png";
			};

			BindingContext = this;
			#endif
		}

		~Bugzilla42069_Page()
		{
			Debug.WriteLine(DestructorMessage);
		}

		public FileImageSource ImageWhichChanges { get; set; }
	}
}