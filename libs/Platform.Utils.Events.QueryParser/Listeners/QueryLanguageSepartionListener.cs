namespace Platform.Utils.Events.QueryParser.Listeners
{
    using System.Collections.Generic;
    using Domain.Syntax;

    internal class QueryLanguageSepartionListener : QueryLanguageBaseListener
    {
        public List<string> QueryList = new List<string>();

        public override void EnterQueryChainItem(QueryLanguageParser.QueryChainItemContext context)
        {
            this.QueryList.Add(context.query().GetText());
            base.EnterQueryChainItem(context);
        }
    }
}