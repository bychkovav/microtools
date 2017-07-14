namespace Platform.Utils.Nhibernate.Conventions
{
    using System.Data;
    using FluentNHibernate.Conventions;
    using FluentNHibernate.Conventions.Instances;

    public class TableNameConvention : IJoinedSubclassConvention, IHasManyToManyConvention, IClassConvention
    {
        public void Apply(IJoinedSubclassInstance instance)
        {
            instance.Table(string.Format("{0}", instance.Type.Name.Replace("Entity", "")));
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.Table(string.Format("{0}", instance.TableName.Replace("Entity", "")));
        }

        public void Apply(IClassInstance instance)
        {
            instance.Table(string.Format("{0}", instance.TableName.Replace("Entity", "")));
        }
    }
}
