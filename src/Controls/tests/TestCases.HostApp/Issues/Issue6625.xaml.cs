namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 6625, "Changing image source on Android causes flicker between images", PlatformAffected.Android)]
	public partial class Issue6625 : ContentPage
	{
		readonly ImageSource[] imageSources =
		[
			"https://64.media.tumblr.com/14cb5aa197e5d9ca6479d955f68344f0/cb28a32e384437f2-07/s540x810/005508be5849eff8fda5b0ebda23f9fcbede164e.jpg",
			"https://64.media.tumblr.com/d66b1709639c23315d26c0a4e977c399/1fb3e31c5e63625d-81/s540x810/2e9a893ec8c1f65a4b9fb8f1264b4fb76914ced8.jpg",
			null,
			"https://64.media.tumblr.com/e7db300b8248c0a7f4c66884032c9414/8f89498ebe784d1c-4e/s540x810/90835c041547bf2936ee249c5c5e1b4eddd589a3.jpg",
			"https://64.media.tumblr.com/fd901060692d2c9ca6f05ddc58e28e5d/2db73c9c5730d87c-e2/s500x750/092c9dc3cff5150acf24bf22a9e61b9719d9ccd6.jpg",
			"https://64.media.tumblr.com/c011aa547a45ac6fe47c46beeb290596/eafc76ded66f16f9-dc/s500x750/5ed923ed086296a2bd41cab3106079eb7addc2d0.jpg",
			"https://this.is.a.broken.url",
			new FontImageSource { FontFamily = "FA", Glyph = "\uf111", Size = 200, Color = Colors.Blue },
			new FontImageSource { FontFamily = "FA", Glyph = "\uf192", Size = 200, Color = Colors.Blue },
			new FontImageSource { FontFamily = "FA", Glyph = "\uf111", Size = 200, Color = Colors.Blue },
			new FontImageSource { FontFamily = "FA", Glyph = "\uf192", Size = 200, Color = Colors.Blue }
		];

		int imageNo = -1;

		public Issue6625()
		{
			InitializeComponent();
			Container.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(NextImage) });
			NextImage();
		}

		void NextImage()
		{
			imageNo = (imageNo + 1) % imageSources.Length;

			ImageSource imageSource = imageSources[imageNo];
			Label.Text = imageSource switch
			{
				UriImageSource uri => uri.Uri.ToString(),
				FontImageSource font => $"Glyph: {Convert.ToBase64String(font.Glyph.Select(c => (byte)c).ToArray())}",
				_ => imageSource?.GetType().Name ?? "null"
			};
			Image.Source = imageSource;
		}
	}
}