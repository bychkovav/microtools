using System;
using System.Linq;

namespace Platform.Utils.Kafka
{
    using System.Collections.Generic;
    using System.Configuration;
    using Configuration;
    using KafkaNet;
    using KafkaNet.Model;
    using KafkaNet.Protocol;

    public class KafkaBootstrapper
    {
        private static volatile KafkaBootstrapper instance;

        private static readonly object SyncRoot = new object();

        private static volatile bool isInitialized;

        private readonly IDictionary<string, Producer> producers = new Dictionary<string, Producer>();
        private readonly IDictionary<string, Consumer> consumers = new Dictionary<string, Consumer>();
        private readonly IDictionary<string, List<Uri>> clusters = new Dictionary<string, List<Uri>>();

        public Producer DefaultClient
        {
            get
            {
                if (!isInitialized)
                {
                    throw new InvalidOperationException("KafkaBootstrapper should be initialize before accessing DefaultClient property. Call Init first.");
                }
                return this.producers.FirstOrDefault().Value;
            }
        }

        private KafkaBootstrapper()
        {
        }

        public static KafkaBootstrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new KafkaBootstrapper();
                            instance.Init();
                        }

                    }
                }

                return instance;
            }
        }

        private void Init()
        {
            if (!isInitialized)
            {
                lock (SyncRoot)
                {
                    if (!isInitialized)
                    {
                        foreach (var cluster in KafkaConfigurationSection.Current.Clusters.Cast<ClusterElement>())
                        {
                            var brokers =
                                cluster.Brokers.Cast<BrokerElement>().Select(x => new Uri(x.Url)).ToList();
                            this.clusters.Add(cluster.Name, brokers);
                        }

                        foreach (var consumer in KafkaConfigurationSection.Current.Consumers.Cast<ConsumerElement>())
                        {
                            if (!this.clusters.ContainsKey(consumer.Cluster))
                            {
                                throw new ConfigurationErrorsException($"Error creating consumer {consumer.Cluster} : cluster {consumer.Cluster} is not defined");
                            }
                            GetConsumer(consumer.Topic, consumer.Cluster);
                        }

                        foreach (var producer in KafkaConfigurationSection.Current.Producer.Cast<ProducerElement>())
                        {
                            if (!this.clusters.ContainsKey(producer.Cluster))
                            {
                                throw new ConfigurationErrorsException(
                                    $"Error creating producer {producer.Cluster} : cluster {producer.Cluster} is not defined");
                            }
                            GetProducer(producer.Cluster, producer.BatchDelayTimeMs);
                        }

                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an existing Consumer with the specified topic and cluster, if Consumer does not exist - creates a new one. 
        /// </summary>
        /// <param name="topic">Topic to consume</param>
        /// <param name="cluster">Cluster to work with</param>
        public Consumer GetConsumer(string topic, string cluster = null)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }

            var clusterInfo = GetClusterInfo(cluster);
            if (this.consumers.ContainsKey(topic))
            {
                return this.consumers[topic];
            }

            var consumerRouter =
                new BrokerRouter(new KafkaOptions(clusterInfo.Value.ToArray()));

            var consumer = new Consumer(new ConsumerOptions(topic, consumerRouter));
            var offsets = consumer.GetTopicOffsetAsync(topic).GetAwaiter().GetResult();
            consumer.SetOffsetPosition(
                offsets.Where(x => x.Offsets.Any())
                    .Select(x => new OffsetPosition(x.PartitionId, x.Offsets.First()))
                    .ToArray());
            this.consumers.Add(topic, consumer);
            return consumer;
        }

        /// <summary>
        /// Returns an existing Producer for the cluster, if Producer does not exist - creates a new one.
        /// </summary>
        /// <param name="batchDelayMs"></param>
        /// <param name="cluster">Cluster to work with</param>
        public Producer GetProducer(string cluster = null, int batchDelayMs = 1000)
        {
            var clusterInfo = GetClusterInfo(cluster);
            if (this.producers.ContainsKey(clusterInfo.Key))
            {
                return this.producers[clusterInfo.Key];
            }

            var producerRouter = new BrokerRouter(new KafkaOptions(clusterInfo.Value.ToArray()));

            var producer = new Producer(producerRouter)
            {
                BatchDelayTime = TimeSpan.FromMilliseconds(batchDelayMs)
            };

            this.producers.Add(clusterInfo.Key, producer);
            return producer;
        }


        private KeyValuePair<string, List<Uri>> GetClusterInfo(string cluster)
        {
            if (!this.clusters.Any())
            {
                throw new InvalidOperationException("No clusters were configured.");
            }
            if (string.IsNullOrEmpty(cluster) || !this.clusters.ContainsKey(cluster))
            {
                return this.clusters.First();
            }
            return this.clusters.First(x => x.Key == cluster);
        }

    }
}
