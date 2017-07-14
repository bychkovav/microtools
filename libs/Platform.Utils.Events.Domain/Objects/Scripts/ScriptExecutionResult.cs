namespace Platform.Utils.Events.Domain.Objects.Scripts
{
    using System.Collections.Generic;

    public class ScriptExecutionResult
    {
        public ScriptExecutionResult()
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }
        public bool Success { get; set; }
    }
}