namespace Platform.Utils.Events.ScriptEngine
{
    public class ScriptDefinition
    {
        public object InputData { get; set; }

        public string Script { get; set; }

        public string ScriptUniqueId { get; set; }

        public ApplicationProxyBase Proxy { get; set; }
    }
}
