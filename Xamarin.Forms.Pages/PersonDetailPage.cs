namespace Xamarin.Forms.Pages
{
	public class PersonDetailPage : DataPage
	{
		public static readonly BindableProperty DisplayNameProperty = BindableProperty.Create(nameof(DisplayName), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty PhoneNumberProperty = BindableProperty.Create(nameof(PhoneNumber), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(ImageSource), typeof(PersonDetailPage), default(ImageSource));

		public static readonly BindableProperty EmailProperty = BindableProperty.Create(nameof(Email), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty AddressProperty = BindableProperty.Create(nameof(Address), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty EmployerProperty = BindableProperty.Create(nameof(Employer), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty TwitterProperty = BindableProperty.Create(nameof(Twitter), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty FacebookProperty = BindableProperty.Create(nameof(Facebook), typeof(string), typeof(PersonDetailPage), default(string));

		public static readonly BindableProperty WebsiteProperty = BindableProperty.Create(nameof(Website), typeof(string), typeof(PersonDetailPage), default(string));

		public PersonDetailPage()
		{
			SetBinding(DisplayNameProperty, new DataSourceBinding(nameof(DisplayName)));
			SetBinding(PhoneNumberProperty, new DataSourceBinding(nameof(PhoneNumber)));
			SetBinding(ImageProperty, new DataSourceBinding(nameof(Image)));
			SetBinding(EmailProperty, new DataSourceBinding(nameof(Email)));
			SetBinding(AddressProperty, new DataSourceBinding(nameof(Address)));
			SetBinding(EmployerProperty, new DataSourceBinding(nameof(Employer)));
			SetBinding(TwitterProperty, new DataSourceBinding(nameof(Twitter)));
			SetBinding(FacebookProperty, new DataSourceBinding(nameof(Facebook)));
			SetBinding(WebsiteProperty, new DataSourceBinding(nameof(Website)));
		}

		public string Address
		{
			get { return (string)GetValue(AddressProperty); }
			set { SetValue(AddressProperty, value); }
		}

		public string DisplayName
		{
			get { return (string)GetValue(DisplayNameProperty); }
			set { SetValue(DisplayNameProperty, value); }
		}

		public string Email
		{
			get { return (string)GetValue(EmailProperty); }
			set { SetValue(EmailProperty, value); }
		}

		public string Employer
		{
			get { return (string)GetValue(EmployerProperty); }
			set { SetValue(EmployerProperty, value); }
		}

		public string Facebook
		{
			get { return (string)GetValue(FacebookProperty); }
			set { SetValue(FacebookProperty, value); }
		}

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public string PhoneNumber
		{
			get { return (string)GetValue(PhoneNumberProperty); }
			set { SetValue(PhoneNumberProperty, value); }
		}

		public string Twitter
		{
			get { return (string)GetValue(TwitterProperty); }
			set { SetValue(TwitterProperty, value); }
		}

		public string Website
		{
			get { return (string)GetValue(WebsiteProperty); }
			set { SetValue(WebsiteProperty, value); }
		}
	}
}