namespace UITest.Core
{
    public class CommandResponse
    {
        public static readonly CommandResponse FailedEmptyResponse = new(null, CommandResponseResult.Failed);
        public static readonly CommandResponse SuccessEmptyResponse = new(null, CommandResponseResult.Success);

        public CommandResponse(object? val, CommandResponseResult result)
        {
            Value = val;
            Result = result;
        }

        public CommandResponseResult Result { get; private set; }

        public object? Value { get; private set; }
    }
}