namespace Xamarin.Forms
{
	public class OnPlatform<T>
	{
		public T Android { get; set; }
		public T iOS { get; set; }
		public T WinPhone { get; set; }

		public T Tizen { get; set; }

		public static implicit operator T(OnPlatform<T> onPlatform)
		{
			return Device.OnPlatform(iOS: onPlatform.iOS, Android: onPlatform.Android, WinPhone: onPlatform.WinPhone, Tizen: onPlatform.Tizen);
		}
	}
}