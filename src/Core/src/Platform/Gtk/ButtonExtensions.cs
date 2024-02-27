using Gtk;

namespace Microsoft.Maui
{

	public static class ButtonExtensions
	{

		public static void UpdateText(this Button platformView, ITextButton button)
		{
			// need to attach Attributes after setting text again, so get it ...
			var attrs = (platformView.Child as Label)?.Attributes;

			// cause maybe a new label is created on assiging text:
			platformView.Label = button.Text;

			if (platformView.Child is Label lbl)
			{
				// and set it again on the new label:
				lbl.Attributes = attrs;
			}
		}

		[MissingMapper]
		public static void UpdateLineBreakMode(this Button platformView, ITextButton button)
		{
			
		}

	}

}