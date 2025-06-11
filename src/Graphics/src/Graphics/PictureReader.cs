namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines an interface for reading picture data from a byte array.
	/// </summary>
	public interface IPictureReader
	{
		/// <summary>
		/// Reads picture data from a byte array.
		/// </summary>
		/// <param name="data">The picture data as a byte array.</param>
		/// <returns>An <see cref="IPicture"/> object representing the read picture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
		IPicture Read(byte[] data);
	}
}
