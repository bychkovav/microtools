namespace Platform.Utils.Events.ScriptEngine.Helpers
{
    public class ScriptHelper
    {
        /// <summary> Produces Delegate Call script executable string </summary>
        /// <param name="delegateName">Delegate name to call</param>
        /// <param name="variableNames">Variables to pass to delegate</param>
        /// <returns></returns>
        public static string GetDelegateCall(string delegateName, params string[] variableNames)
        {
            var scriptLine = $"Services.{delegateName}({string.Join(", ", variableNames)})";

            return scriptLine;
        }


        /// <summary> Produces Fire Event script executable string </summary>
        /// <param name="phaseName"> Event phase name </param>
        /// <param name="variableName"> Variable to pass to event </param>
        /// <param name="elementName"> Event modelElement code </param>
        /// <param name="actionName"> Event action name </param>
        /// <returns></returns>
        public static string GetFireEvent(string elementName, string actionName, string phaseName, string variableName)
        {
            var scriptLine = $"Services.FireEvent(\"{elementName}\", \"{actionName}\", \"{phaseName}\", {variableName})";

            return scriptLine;
        }


        /// <summary> Produces Fire Command script executable string </summary>
        /// <param name="phaseName"> Command phase name </param>
        /// <param name="variableName"> Variable to pass to Command </param>
        /// <param name="elementName"> Command modelElement code </param>
        /// <param name="actionName"> Command action name </param>
        /// <returns></returns>
        public static string GetInvokeCommand(string elementName, string actionName, string phaseName, string variableName)
        {
            var scriptLine = $"Services.InvokeCommand(\"{elementName}\", \"{actionName}\", \"{phaseName}\", {variableName})";

            return scriptLine;
        }
    }
}
