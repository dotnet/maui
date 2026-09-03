#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <summary>
	/// The Contacts API lets a user pick a contact and retrieve information about it.
	/// </summary>
	public interface IContacts
	{
		/// <summary>
		/// Opens the operating system's default UI for picking a contact from the device.
		/// </summary>
		/// <returns>A single contact, or <see langword="null"/> if the user cancelled the operation.</returns>
		Task<Contact?> PickContactAsync();

		/// <summary>
		/// Gets a collection of all the contacts on the device.
		/// </summary>
		/// <param name="cancellationToken">A token that can be used for cancelling the operation.</param>
		/// <returns>A collection of contacts on the device.</returns>
		Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// The Contacts API lets a user pick a contact and retrieve information about it.
	/// </summary>
	public static class Contacts
	{
		/// <summary>
		/// Opens the operating system's default UI for picking a contact from the device.
		/// </summary>
		/// <returns>A single contact, or <see langword="null"/> if the user cancelled the operation.</returns>
		public static Task<Contact?> PickContactAsync() =>
			Default.PickContactAsync();

		/// <summary>
		/// Gets a collection of all the contacts on the device.
		/// </summary>
		/// <param name="cancellationToken">A token that can be used for cancelling the operation.</param>
		/// <returns>A collection of contacts on the device.</returns>
		public static Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default) =>
			Default.GetAllAsync(cancellationToken);

		static IContacts? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IContacts Default =>
			defaultImplementation ??= new ContactsImplementation();

		internal static void SetDefault(IContacts? implementation) =>
			defaultImplementation = implementation;
	}
}
