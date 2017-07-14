namespace Platform.Utils.Nhibernate.Conventions
{
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    public class SafeDeleteConvention : IClassConvention, IHasManyConvention
    {
        public const string FilterName = "safeDelete";

        public void Apply(IClassInstance instance)
        {
            instance.ApplyFilter(FilterName);
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.ApplyFilter(FilterName);
        }
    }
}