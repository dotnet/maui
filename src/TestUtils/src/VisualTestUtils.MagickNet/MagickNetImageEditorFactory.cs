namespace VisualTestUtils.MagickNet
{
    public class MagickNetImageEditorFactory : IImageEditorFactory
    {
        public IImageEditor CreateImageEditor(ImageSnapshot imageSnapshot) =>
            new MagickNetImageEditor(imageSnapshot);
    }
}
