#nullable disable
namespace Microsoft.Maui.Controls
{

	/// <summary>
	/// Defines a contract for objects that can be attached to a <see cref="BindableObject"/>.
	/// This interface is typically implemented by behaviors or effects that need to interact with
	/// the lifecycle or properties of a visual element in the UI.
	/// </summary>
	public interface IAttachedObject
	{

		/// <summary>
		/// Attaches the object to the specified <see cref="BindableObject"/>.
		/// This method is called when the object is added to the visual tree.
		/// </summary>
		/// <param name="bindable">The <see cref="BindableObject"/> to attach to.</param>
		void AttachTo(BindableObject bindable);

		/// <summary>
		/// Detaches the object from the specified <see cref="BindableObject"/>.
		/// This method is called when the object is removed from the visual tree.
		/// </summary>
		/// <param name="bindable">The <see cref="BindableObject"/> to detach from.</param>
		void DetachFrom(BindableObject bindable);
	}
}