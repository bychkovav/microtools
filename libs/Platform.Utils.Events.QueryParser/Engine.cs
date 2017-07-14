namespace Platform.Utils.Events.QueryParser
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using Domain.Objects;
    using Domain.Syntax;
    using Listeners;

    public class Engine
    {
        public SingleQuery Parse(string query)
        {
            var input = new AntlrInputStream(query);
            var lexer = new QueryLanguageLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QueryLanguageParser(tokens);

            IParseTree tree = parser.query();

            var walker = new ParseTreeWalker();
            var listener = new QueryLanguageListener();

            walker.Walk(listener, tree);
            return listener.RootQuery;
        }

        public List<string> GetQueries(string query)
        {
            var input = new AntlrInputStream(query);
            var lexer = new QueryLanguageLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new QueryLanguageParser(tokens);

            IParseTree tree = parser.queryChain();

            var walker = new ParseTreeWalker();
            var listener = new QueryLanguageSepartionListener();

            walker.Walk(listener, tree);
            return listener.QueryList;
        }

        public string PreprocessScript(string script)
        {
            var result = Regex.Replace(script, @"<query>(.*?)</query>", (Match match) =>
            {
                var stringQuery = match.Groups[1].Value;
                return $"JsonLinqExecutor.GetExecutor.Run(\"{stringQuery.Replace("\"", "'")}\", e.Data, (ExpandoObject)Services)";
            }, RegexOptions.Singleline);
//            result = Regex.Replace(result, @"<json>(.*?)</json>", (Match match) => RenderQuery(match.Groups[1].Value.ToSingleQuery()), RegexOptions.Singleline);

            return result;
        }

    }
}