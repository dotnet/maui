using System;

namespace VisualTestUtils
{
    public class VisualTestFailedException : Exception
    {
        public VisualTestFailedException(string message)
            : base(message)
        {
        }
    }
}
