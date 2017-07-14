namespace Platform.Utils.NLog.ElasticSearch
{
    using System;
    using global::NLog.Config;
    using global::NLog.Layouts;

    [NLogConfigurationItem]
    public class ElasticField
    {
        public ElasticField()
        {
            LayoutType = typeof(string);
        }

        [RequiredParameter]
        public string Name { get; set; }

        [RequiredParameter]
        public Layout Layout { get; set; }

        public Type LayoutType { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, LayoutType: {1}, Layout: {2}", Name, LayoutType, Layout);
        }
    }
}
