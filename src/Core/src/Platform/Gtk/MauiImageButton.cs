namespace Microsoft.Maui.Platform;

public class MauiImageButton : Gtk.Button
{
	public ImageView ImageView
	{
		get
		{
			if (Image is ImageView image)
				return image;
			Image = image = new ImageView();
			return image;
		}
		set
		{
			Image = value;
		}
	}
}