//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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