namespace Microsoft.Maui.Graphics
{
    public delegate void DrawingCommand(ICanvas canvas);

    public class StandardPicture : IPicture
    {
        private readonly DrawingCommand[] _commands;

        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public string Hash { get; set; }

        public StandardPicture(float x, float y, float width, float height, DrawingCommand[] commands, string hash = null)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _commands = commands;

            Hash = hash;
        }

        public void Draw(ICanvas canvas)
        {
            if (_commands != null)
                foreach (var command in _commands)
                    command.Invoke(canvas);
        }
    }
}
