namespace Xamarin.Forms
{
	public class OnIdiom<T>
	{
		public T Phone { get; set; }

		public T Tablet { get; set; }
		
		public T Desktop { get; set; }

		public T TV { get; set; }

		public static implicit operator T(OnIdiom<T> onIdiom)
		{
			switch (Device.Idiom)
			{
				default:
				case TargetIdiom.Phone:
					return onIdiom.Phone;
				case TargetIdiom.Tablet:
					return onIdiom.Tablet;
				case TargetIdiom.Desktop:
					return onIdiom.Desktop;
				case TargetIdiom.TV:
					return onIdiom.TV;
			}
		}
	}
}
