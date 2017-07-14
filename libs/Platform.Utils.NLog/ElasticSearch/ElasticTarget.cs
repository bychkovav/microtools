﻿namespace Platform.Utils.NLog.ElasticSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Elasticsearch.Net;
    using Elasticsearch.Net.Connection;
    using Elasticsearch.Net.ConnectionPool;
    using Elasticsearch.Net.Serialization;
    using global::NLog.Common;
    using global::NLog.Config;
    using global::NLog.Layouts;
    using global::NLog.Targets;

    [Target("ElasticSearch")]
    public class ElasticSearchTarget : TargetWithLayout
    {
        #region [Fields]

        private IElasticsearchClient client;

        private List<string> excludedProperties = new List<string>(new[] { "CallerMemberName", "CallerFilePath", "CallerLineNumber", "MachineName", "ThreadId" });

        #endregion

        public string Uri { get; set; }

        public Layout Index { get; set; }

        public bool IncludeAllProperties { get; set; }

        [RequiredParameter]
        public Layout DocumentType { get; set; }

        [ArrayParameter(typeof(ElasticField), "field")]
        public IList<ElasticField> Fields { get; private set; }

        public IElasticsearchSerializer ElasticsearchSerializer { get; set; }

        public string ExcludedProperties { get; set; }

        public ElasticSearchTarget()
        {
            Uri = "http://localhost:9200";
            DocumentType = "logevent";
            Index = "logstash-${date:format=yyyy.MM.dd}";
            Fields = new List<ElasticField>();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            var uri = Uri;
            var nodes = uri.Split(',').Select(url => new Uri(url));
            var connectionPool = new StaticConnectionPool(nodes);
            var config = new ConnectionConfiguration(connectionPool);
            config.SetPingTimeout(5000);
            this.client = new ElasticsearchClient(config, serializer: ElasticsearchSerializer);

            if (!String.IsNullOrEmpty(ExcludedProperties))
                this.excludedProperties = new List<string>(ExcludedProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write(new[] { logEvent });
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            SendBatch(logEvents);
        }

        private void SendBatch(IEnumerable<AsyncLogEventInfo> events)
        {
            var logEvents = events.Select(e => e.LogEvent);
            var payload = new List<object>();

            foreach (var logEvent in logEvents)
            {
                var document = new Dictionary<string, object>();
                document.Add("@timestamp", logEvent.TimeStamp);
                document.Add("level", logEvent.Level.Name);
                if (logEvent.Exception != null)
                    document.Add("exception", logEvent.Exception.ToString());
                document.Add("message", Layout.Render(logEvent));
                foreach (var field in Fields)
                {
                    var renderedField = field.Layout.Render(logEvent);
                    if (!string.IsNullOrWhiteSpace(renderedField))
                        document[field.Name] = renderedField.ToSystemType(field.LayoutType);
                }

                if (IncludeAllProperties)
                {
                    foreach (var p in logEvent.Properties.Where(p => !this.excludedProperties.Contains(p.Key)))
                    {
                        if (document.ContainsKey(p.Key.ToString()))
                            continue;

                        document[p.Key.ToString()] = p.Value;
                    }
                }

                var index = Index.Render(logEvent).ToLowerInvariant();
                var type = DocumentType.Render(logEvent);

                payload.Add(new { index = new { _index = index, _type = type } });
                payload.Add(document);
            }


            try
            {
                var result = this.client.Bulk<byte[]>(payload);
                if (!result.Success)
                    InternalLogger.Error("Failed to send log messages to ElasticSearch: status={0} message=\"{1}\"", result.HttpStatusCode, result.OriginalException.Message);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error while sending log messages to ElasticSearch: message=\"{0}\"", ex.Message);
            }
        }
    }
}
