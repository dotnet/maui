using System;
using System.ComponentModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Represents a pin on the <see cref="Map"/> control.
	/// </summary>
	public partial class Pin : Element
	{
		/// <summary>Bindable property for <see cref="Type"/>.</summary>
		public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(PinType), typeof(Pin), default(PinType));

		/// <summary>Bindable property for <see cref="Location"/>.</summary>
		public static readonly BindableProperty LocationProperty = BindableProperty.Create(nameof(Location), typeof(Location), typeof(Pin), default(Location));

		/// <summary>Bindable property for <see cref="Address"/>.</summary>
		public static readonly BindableProperty AddressProperty = BindableProperty.Create(nameof(Address), typeof(string), typeof(Pin), default(string));

		/// <summary>Bindable property for <see cref="Label"/>.</summary>
		public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));

		/// <summary>Bindable property for <see cref="ImageSource"/>.</summary>
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(Pin), default(ImageSource));

		private object? _markerId;

		/// <inheritdoc />
		public string Address
		{
			get { return (string)GetValue(AddressProperty); }
			set { SetValue(AddressProperty, value); }
		}

		/// <inheritdoc />
		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		/// <inheritdoc />
		public Location Location
		{
			get { return (Location)GetValue(LocationProperty); }
			set { SetValue(LocationProperty, value); }
		}

		/// <summary>
		/// Gets or sets the kind of pin. The default value is <see cref="PinType.Generic"/>.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>Depending on the platform, and versions of the platform, this might change the visual representation. This varies between the different platforms.</remarks>
		public PinType Type
		{
			get { return (PinType)GetValue(TypeProperty); }
			set { SetValue(TypeProperty, value); }
		}

		/// <summary>
		/// Gets or sets the custom image source for this pin's icon.
		/// When set, this image will be used instead of the default platform pin icon.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>
		/// Supported image sources include file-based images, embedded resources, URIs, and streams.
		/// The image will be scaled by the underlying platform to a platform-defined size (32x32 points on iOS, 64x64 pixels on Android).
		/// Provide images that look clear when scaled to these sizes.
		/// </remarks>
		public ImageSource? ImageSource
		{
			get { return (ImageSource?)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the platform counterpart of this pin element.
		/// </summary>
		/// <remarks>This should typically not be set by the developer. Doing so might result in unpredictable behavior.</remarks>
		public object? MarkerId
		{
			get => _markerId;
			set
			{
				_markerId = value;
			}
		}

		/// <summary>
		/// Raised when this marker is clicked.
		/// </summary>
		public event EventHandler<PinClickedEventArgs>? MarkerClicked;

		/// <summary>
		/// Raised when the info window for this marker is clicked.
		/// </summary>
		public event EventHandler<PinClickedEventArgs>? InfoWindowClicked;

		/// <inheritdoc cref="object.Equals(object)"/>
		public override bool Equals(object? obj)
		{
			if (obj is null)
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((Pin)obj);
		}

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode()
		{
			unchecked
			{
#if NETSTANDARD2_0
				int hashCode = Label?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ Location.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)Type;
				hashCode = (hashCode * 397) ^ (Address?.GetHashCode() ?? 0);
#else
				int hashCode = Label?.GetHashCode(StringComparison.Ordinal) ?? 0;
				hashCode = (hashCode * 397) ^ Location.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)Type;
				hashCode = (hashCode * 397) ^ (Address?.GetHashCode(StringComparison.Ordinal) ?? 0);
#endif
				return hashCode;
			}
		}

		/// <summary>
		/// Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(Pin? left, Pin? right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(Pin? left, Pin? right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		/// Triggers the click/tap event for a pin.
		/// </summary>
		/// <returns><see langword="true"/> if the info window associated to this pin will hide after this event has occurred, otherwise <see langword="false"/></returns>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool SendMarkerClick()
		{
			var args = new PinClickedEventArgs();
			MarkerClicked?.Invoke(this, args);
			return args.HideInfoWindow;
		}

		/// <summary>
		/// Triggers the click/tap event for a info window of a pin.
		/// </summary>
		/// <returns><see langword="true"/> if the info window associated to this pin will hide after this event has occurred, otherwise <see langword="false"/></returns>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool SendInfoWindowClick()
		{
			var args = new PinClickedEventArgs();
			InfoWindowClicked?.Invoke(this, args);
			return args.HideInfoWindow;
		}

		bool Equals(Pin other)
		{
			return string.Equals(Label, other.Label, StringComparison.Ordinal) &&
				Equals(Location, other.Location) && Type == other.Type &&
				string.Equals(Address, other.Address, StringComparison.Ordinal);
		}
	}
}
