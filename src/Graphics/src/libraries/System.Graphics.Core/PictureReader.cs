namespace System.Graphics
{
    public interface IPictureReader
    {
        IPicture Read(byte[] data);
    }
}