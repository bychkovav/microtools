namespace Platform.Utils.Events.Domain.Objects.ExecutionContext
{
    using System;

    public class ExecutionContext
    {
        public int PrincipalType { get; set; }

        public CoreIdentities CoreIdentities { get; set; }

        public Guid LocalMasterId { get; set; }

        public static ExecutionContext Empty()
        {
            return new ExecutionContext()
            {
                CoreIdentities = new CoreIdentities()
            };
        }
    }
}
