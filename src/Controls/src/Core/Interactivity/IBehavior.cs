#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{

	/// <summary>
	/// Represents a behavior that can be attached to a <see cref="BindableObject"/>.
	/// </summary>
	public interface IBehavior : INotifyPropertyChanged, IAttachedObject
	{
	}

	/// <summary>
	/// Represents a behavior that can be attached to a specific type of <see cref="BindableObject"/>.
	/// This generic interface allows behaviors to be strongly typed to the target element.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="BindableObject"/> the behavior can attach to.</typeparam>
	public interface IBehavior<T> : IBehavior where T : BindableObject
	{
	}
}