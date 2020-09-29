namespace Xamarin.Forms
{
	public enum RelativeBindingSourceMode
	{
		// 0 reserved for possible future implementation of PreviousData 

		TemplatedParent = 1,
		Self = 2,
		FindAncestor = 3,
		FindAncestorBindingContext = 4,
	}
}