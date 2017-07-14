namespace Platform.Utils.NLog.Kafka
{
    using global::NLog.Common;
    using global::NLog.Targets;
    using KafkaNet.Protocol;
    using Utils.Kafka;

    [Target("Kafka")]
    public class KafkaTarget : TargetWithLayout
    {
        protected override async void Write(AsyncLogEventInfo logEvent)
        {
            var formattedMessage = this.Layout.Render(logEvent.LogEvent);

            await
                KafkaBootstrapper.Instance.DefaultClient.SendMessageAsync(logEvent.LogEvent.LoggerName,
                    new[] { new Message(formattedMessage) });
        }
    }
}
