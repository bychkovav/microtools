namespace Platform.Utils.Events.QueryGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using NLog;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Extensions;
    using Platform.Utils.Events.QueryParser.Helpers;
    using Platform.Utils.Events.QueryParser.Builders.QueryLanguage;
    using Platform.Utils.Events.QueryParser.Domain.Enums;
    using Platform.Utils.Events.QueryParser.Domain.Objects;
    using Platform.Utils.Events.QueryParser.Extensions;
    using Platform.Utils.Events.QueryParser.Extensions.Fluent;
    using Platform.Utils.Json;

    public class QueryGenerator
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly QueryLanguageBuilder queryLanguageBuilder;

        public QueryGenerator()
        {
            this.queryLanguageBuilder = new QueryLanguageBuilder();
        }

        public List<SingleQuery> GeneratePatchQuery(SingleQuery path,
            JToken value,
            ModelElementObjectBase classDescription,
            IList<ModelElementObjectBase> allClassDescriptions)
        {
            return FillSetQuery(path, value, classDescription, allClassDescriptions, false);
        }

        public List<SingleQuery> GeneratePostQuery(SingleQuery path,
            JToken value,
            ModelElementObjectBase classDescription,
            IList<ModelElementObjectBase> allClassDescriptions)
        {
            return FillSetQuery(path, value, classDescription, allClassDescriptions, true);
        }


        private List<SingleQuery> FillSetQuery(SingleQuery path, JToken value, ModelElementObjectBase classDescription,
            IList<ModelElementObjectBase> allClassDescriptions, bool forceSet)
        {
            var children = classDescription.Children.ToList();

            var result = new List<SingleQuery>();

            path = path.MakeCopy();
            if (forceSet)
            {
                path.NodesList.First.Value.Type = QueryNodeType.Property;
            }

            var set = path.MakeCopy().MethodSet();

            var hasVx = false;

            if (value == null)
                throw new InvalidOperationException("Value can not be null");

            if (value.Type != JTokenType.Object)
            {
                throw new InvalidOperationException($"Value for being setted on {path} path should be object, but was {value.Type}");
            }

            foreach (var propertyDescription in children)
            {
                var propertyPivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[propertyDescription.ModelElementType]);
                var propertyToken = value[propertyDescription.DisplayText];

                if (propertyToken != null)
                {
                    if (propertyPivot.IsIntent)
                    {
                        throw new InvalidOperationException($"Intents must be edited independently. But you've sent {path}.{propertyDescription.DisplayText}");
                    }
                }

                //todo: initiate?
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Vx))
            {
                var propertyToken = value[propertyDescription.DisplayText];
                if (propertyToken != null)
                {
                    if (forceSet && propertyToken.Type == JTokenType.Null)
                        continue;

                    var vxTypeDescription = allClassDescriptions.FirstOrDefault(x => x.ModelDefinitionId == propertyDescription.LinkedModelDefinitionId);
                    hasVx = true;



                    if (vxTypeDescription != null)
                    {
                        var set2 = path.MakeCopy().AddProperty(ParserHelper.GetPivotName(PivotType.Value))
                            .SetPivot(PivotType.Value, propertyDescription.Code);

                        result.AddRange(FillSetQuery(set2, propertyToken, vxTypeDescription, allClassDescriptions, true));
                    }
                    else
                    {
                        if (propertyToken.Type != JTokenType.Null && propertyDescription.DataType != null)
                        {
                            switch (propertyDescription.DataType)
                            {
                                case DataType.Boolean:
                                    if (propertyToken.Type != JTokenType.Boolean)
                                    {
                                        throw new InvalidOperationException($"Value for the property {propertyDescription.DisplayText} should be bool, but was {value.Type}");
                                    }
                                    break;
                                case DataType.Number:
                                    if ((propertyToken.Type == JTokenType.Float) || propertyToken.Type == JTokenType.Integer)
                                    {
                                        throw new InvalidOperationException($"Value for the property {propertyDescription.DisplayText} should be bool, but was {value.Type}");
                                    }
                                    break;

                            }
                        }

                        set.AddArgument(
                            x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery()
                                .AddProperty("vx")
                                .SetPivot(PivotType.Value, propertyDescription.Code),
                            x => x.ArgumentValueConstant = propertyToken.ToPrimitiveObject());
                    }
                }
            }

            var pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[classDescription.ModelElementType]);

            foreach (var property in pivot.GetAllCommonProperties())
            {
                var propertyToken = value[property];
                if (propertyToken != null)
                {
                    if (forceSet && propertyToken.Type == JTokenType.Null)
                        continue;

                    hasVx = true;

                    set.AddArgument(
                        x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery().AddProperty(property),
                        x => x.ArgumentValueConstant = propertyToken.ToPrimitiveObject());
                }
            }

            if (hasVx || forceSet)
            {
                result.Add(set);
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Md))
            {
                var token = value[propertyDescription.DisplayText];

                if (token != null)
                {
                    if (token.Type != JTokenType.Array)
                    {
                        throw new InvalidOperationException($"Value for being setted on {path} path should be an array, but was {value.Type}");
                    }

                    var arrayPropertyToken = (JArray)token;

                    foreach (var propertyToken in arrayPropertyToken)
                    {
                        if (propertyToken.Type != JTokenType.Object
                            || propertyToken[ObjectHelper.CapacityPropName] == null
                            || propertyToken[ObjectHelper.CapacityPropName].Type != JTokenType.String
                            || propertyToken[ObjectHelper.IdPropName] == null
                            || propertyToken[ObjectHelper.IdPropName].Type != JTokenType.String)
                        {
                            throw new InvalidOperationException($"Value for md being setted at {path} path should be an array, " +
                                                                $"with objects containing id and capacity string properties. " +
                                                                $"But was {arrayPropertyToken}");
                        }

                        var add = path.MakeCopy()
                            .MethodAdd()
                            .SetPivot(PivotType.MasterData, propertyDescription.Code, (string)propertyToken["capacity"])
                            .AddArgument(
                                x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery().AddProperty("id"),
                                x => x.ArgumentValueConstant = (string)propertyToken["id"]);

                        result.Add(add);
                    }
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Ae))
            {
                var aeTypeDescription = allClassDescriptions.FindByElementId(propertyDescription.Id.ToString());
                var token = value[propertyDescription.DisplayText];

                if (token != null)
                {
                    if (token.Type != JTokenType.Array)
                    {
                        throw new InvalidOperationException(
                            $"Value for being setted on {path} path should be an array, but was {value.Type}");
                    }

                    var arrayPropertyToken = (JArray)token;

                    foreach (var propertyToken in arrayPropertyToken)
                    {
                        if (propertyToken.Type != JTokenType.Object
                            || propertyToken[ObjectHelper.CodePropName] == null
                            || propertyToken[ObjectHelper.CodePropName].Type != JTokenType.String)
                        {
                            throw new InvalidOperationException($"Value for ae being setted at {path} path should be an array, " +
                                                                $"with objects containing code string property and other details. " +
                                                                $"But was {arrayPropertyToken}");
                        }


                        var add = path.MakeCopy()
                            .MethodAdd()
                            .SetPivot(PivotType.Attributes, propertyDescription.Code, (string)propertyToken["code"]);

                        aeTypeDescription = new ModelElementObjectBase(aeTypeDescription);
                        aeTypeDescription.ModelElementType = TreeNodeType.Ae;

                        FillAddQuery(add, propertyToken, aeTypeDescription, allClassDescriptions);

                        result.Add(add);
                    }
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Ts))
            {
                var tsTypeDescription = allClassDescriptions.FindByElementId(propertyDescription.Id.ToString());

                var token = value[propertyDescription.DisplayText];

                var arrayPropertyToken = token as JArray;

                if (arrayPropertyToken != null)
                {
                    foreach (var propertyToken in arrayPropertyToken)
                    {
                        var add = path.MakeCopy()
                            .MethodAdd()
                            .SetPivot(PivotType.Task, propertyDescription.Code, (string)propertyToken["code"]);

                        FillAddQuery(add, propertyToken, tsTypeDescription, allClassDescriptions);

                        result.Add(add);
                    }
                }
            }

            foreach (var singleQuery in result)
            {
                var query = this.queryLanguageBuilder.RenderQuery(singleQuery);

                this.logger.Debug($"Generated query: {query}");
            }

            return result;
        }

        public SingleQuery GeneratePutQuery(SingleQuery path,
            JToken value,
            ModelElementObjectBase classDescription,
            IList<ModelElementObjectBase> allClassDescriptions)
        {
            var add = path.MakeCopy();

            FillAddQuery(add, value, classDescription, allClassDescriptions);

            var query = this.queryLanguageBuilder.RenderQuery(add);
            this.logger.Debug($"Generated query: {query}");

            return add;
        }

        private void FillAddQuery(SingleQuery path,
            JToken value,
            ModelElementObjectBase classDescription,
            IList<ModelElementObjectBase> allClassDescriptions)
        {
            var children = classDescription.Children.ToList();

            var pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[classDescription.ModelElementType]);

            foreach (var property in pivot.GetAllCommonProperties())
            {
                var propertyToken = value[property];
                if (propertyToken != null)
                {
                    if (propertyToken.Type == JTokenType.Null)
                        continue;

                    path.AddArgument(
                        x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery().AddProperty(property),
                        x => x.ArgumentValueConstant = propertyToken.ToPrimitiveObject());
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Vx))
            {
                var propertyToken = value[propertyDescription.DisplayText];
                if (propertyToken != null && propertyToken.Type != JTokenType.Null)
                {
                    //todo: composite vx

                    var vxTypeDescription = allClassDescriptions.FirstOrDefault(x => x.ModelDefinitionId == propertyDescription.LinkedModelDefinitionId);

                    if (vxTypeDescription != null)
                    {
                        throw new InvalidOperationException("Vx must not be a complex object for addition");
                    }

                    path.AddArgument(
                        x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery()
                            .AddProperty("vx")
                            .SetPivot(PivotType.Value, propertyDescription.Code),
                        x => x.ArgumentValueConstant = propertyToken.ToPrimitiveObject());
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Md))
            {
                var arrayPropertyToken = value[propertyDescription.DisplayText] as JArray;

                if (arrayPropertyToken != null)
                {
                    foreach (var propertyToken in arrayPropertyToken)
                    {
                        var add = SingleQuery.InitiateQuery()
                            .AddCollection("md")
                            .SetPivot(PivotType.MasterData, propertyDescription.Code, (string)propertyToken["capacity"])
                            .MethodAdd()
                            .AddArgument(
                                x => x.ArgumentSubjectQuery = SingleQuery.InitiateQuery().AddProperty("id"),
                                x => x.ArgumentValueConstant = (string)propertyToken["id"]);

                        path.AddArgument(x => x.ArgumentValueQuery = add);
                    }
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Ae))
            {
                var aeTypeDescription = allClassDescriptions.FirstOrDefault(x => x.ModelDefinitionId == propertyDescription.LinkedViewModelId);
                var arrayPropertyToken = value[propertyDescription.DisplayText] as JArray;

                if (aeTypeDescription != null && arrayPropertyToken != null)
                {
                    foreach (var propertyToken in arrayPropertyToken)
                    {
                        var add = SingleQuery.InitiateQuery()
                            .AddCollection("ae")
                            .SetPivot(PivotType.Attributes, propertyDescription.Code)
                            .MethodAdd();

                        aeTypeDescription = new ModelElementObjectBase(aeTypeDescription);
                        aeTypeDescription.ModelElementType = TreeNodeType.Ae;

                        FillAddQuery(add, propertyToken, aeTypeDescription, allClassDescriptions);

                        path.AddArgument(x => x.ArgumentValueQuery = add);
                    }
                }
            }

            foreach (var propertyDescription in children.Where(x => x.ModelElementType == TreeNodeType.Ts))
            {
                var eeTypeDescription = allClassDescriptions.FirstOrDefault(x => x.ModelDefinitionId == propertyDescription.LinkedModelDefinitionId);
                var propertyToken = value[propertyDescription.DisplayText];

                if (eeTypeDescription != null && propertyToken != null)
                {
                    var add = path.MakeCopy()
                        .MethodAdd()
                        .SetPivot(PivotType.Task, propertyDescription.Code, (string)propertyToken["code"]);

                    FillSetQuery(add, propertyToken, eeTypeDescription, allClassDescriptions, false);

                    path.AddArgument(x => x.ArgumentValueQuery = add);
                }
            }
        }
    }
}