namespace Platform.Utils.Events.Contracts
{
    public class DebugEventMessage
    {
        public string DebugId { get; set; }

        public string ServiceIdentity { get; set; }

        public bool Success { get; set; }

        public string FailedHandler { get; set; }

        public string ErrorDetails { get; set; }
    }
}
