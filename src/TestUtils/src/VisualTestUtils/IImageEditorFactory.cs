namespace VisualTestUtils
{
    public interface IImageEditorFactory
    {
        /// <summary>
        /// Create an image editor, to update the specified image snapshot.
        /// The original image snapshot is unchanged. After making edits,
        /// call IImageEditor.GetImageSnapshot to get a new image snapshot
        /// with the updates.
        /// </summary>
        /// <param name="imageSnapshot">image snapshot to manipulate</param>
        /// <returns>image editor, that can be used to make image updates</returns>
        IImageEditor CreateImageEditor(ImageSnapshot imageSnapshot);
    }
}
