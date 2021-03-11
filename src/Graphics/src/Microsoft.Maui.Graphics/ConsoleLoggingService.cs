using System.IO;

namespace Microsoft.Maui.Graphics
{
    public class ConsoleLoggingService : ILoggingService
    {
        public virtual void Log(LogType type, string message)
        {
            WriteToConsole(type, message);
        }

        public virtual void Log(LogType type, string message, Exception exception)
        {
            WriteToConsole(type, message + "\n" + GetExceptionDetails(exception));
        }

        protected virtual void WriteToConsole(LogType type, string message)
        {
            Console.WriteLine("[{0}] {1}", type, message);
        }

        protected virtual string GetExceptionDetails(Exception exception)
        {
            var writer = new StringWriter();

            writer.Write("Exception: ");
            writer.WriteLine(exception.GetType());
            writer.Write("Message: ");
            writer.WriteLine(exception.Message);
            if (exception.InnerException != null)
            {
                writer.Write("InnerException: ");
                writer.WriteLine(exception.InnerException);
            }

            writer.Write("StackTrace: ");
            writer.WriteLine(exception.StackTrace);

            return writer.ToString();
        }
    }
}