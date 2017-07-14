using Cassandra;
using Cassandra.Mapping;

namespace Platform.Utils.Cassandra
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using global::Cassandra.Data.Linq;

    public class CassandraDataProvider
    {
        private readonly Cluster cluster;

        public ISession Session { get; }

        public Mapper Mapper => new Mapper(Session);

        protected readonly FieldInfo DefinitionsField = typeof(Mappings).GetField("Definitions",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public CassandraDataProvider(string connectionString, string keyspace, IList<Mappings> mappings)
        {
            var cl = Cluster.Builder()
                 .AddContactPoint(connectionString)
                 .Build();

            this.cluster = cl;

            Session = cl.Connect();

            var keyspaceName = keyspace.ToLower();
            Session.CreateKeyspaceIfNotExists(keyspaceName);
            Session.ChangeKeyspace(keyspaceName);

            if (mappings != null && mappings.Count > 0)
            {
                MappingConfiguration.Global.Define(mappings.ToArray());

                if (this.DefinitionsField != null)
                {
                    foreach (var map in mappings)
                    {
                        var definitions = this.DefinitionsField.GetValue(map) as KeyedCollection<Type, ITypeDefinition>;
                        if (definitions != null)
                        {
                            foreach (var definition in definitions)
                            {
                                Func<Table<object>> f = () => new Table<object>(this.Session);
                                var s = f();

                                var tableType = typeof(Table<>).MakeGenericType(definition.PocoType);
                                var ctorInfo = tableType.GetConstructor(new[] { typeof(ISession) });
                                if (ctorInfo != null)
                                {
                                    //var ctorGeneric = typeof(Func<>).MakeGenericType(tableType);
                                    var newExpression = Expression.New(ctorInfo, Expression.Constant(this.Session));
                                    var callExpression = Expression.Call(newExpression, "CreateIfNotExists", null);
                                    //var lamda = Expression.Lambda(ctorGeneric, callExpression).Compile();
                                    var lamda = Expression.Lambda(typeof(Action), callExpression).Compile();
                                    var res = lamda.DynamicInvoke();
                                }
                            }
                        }
                    }
                }
            }

        }


        public void Close()
        {
            this.cluster.Shutdown();
            Session.Dispose();
        }
    }
}
