namespace Platform.Utils.Events.QueryParser.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Domain.Syntax;
    using Events.Domain.Enums;
    using Extensions.Fluent;
    using Json;

    public static class ParserHelper
    {

        #region Syntax mappings

        private static Dictionary<CriteriaAppendType, int> CriteriaAppendTypeToToken = new Dictionary<CriteriaAppendType, int>
        {
            [CriteriaAppendType.And] = QueryLanguageParser.And,
            [CriteriaAppendType.Or] = QueryLanguageParser.Or,
        };

        private static Dictionary<PivotType, int> PivotTypeToToken = new Dictionary<PivotType, int>
        {
            [PivotType.Transaction] = -1,
            [PivotType.Attributes] = QueryLanguageParser.Ae,
            [PivotType.Value] = QueryLanguageParser.Vx,
            [PivotType.MasterData] = QueryLanguageParser.Md,
            [PivotType.Task] = QueryLanguageParser.Ts,
            [PivotType.HelperData] = QueryLanguageParser.Hd,
            [PivotType.BusinessProcess] = QueryLanguageParser.Bp,
            [PivotType.Activity] = QueryLanguageParser.Ac,
            [PivotType.ValueAttributes] = QueryLanguageParser.Vae,
            [PivotType.ValueHelperData] = QueryLanguageParser.Vhd,
            [PivotType.ValueMasterData] = QueryLanguageParser.Vmd,
            [PivotType.ValueValue] = QueryLanguageParser.Vvx,
        };

        private static Dictionary<QueryMethodType, int> QueryMethodTypeToToken = new Dictionary<QueryMethodType, int>
        {
            [QueryMethodType.Set] = QueryLanguageParser.Set,
            [QueryMethodType.Add] = QueryLanguageParser.Add,
            [QueryMethodType.Get] = QueryLanguageParser.Get,
            [QueryMethodType.Delete] = QueryLanguageParser.Delete,
            [QueryMethodType.ToMd] = QueryLanguageParser.ToMD,
            [QueryMethodType.ToT] = QueryLanguageParser.ToT,
            [QueryMethodType.ToLocal] = QueryLanguageParser.ToLocal,
            [QueryMethodType.Take] = QueryLanguageParser.Take,
            [QueryMethodType.Skip] = QueryLanguageParser.Skip,
            [QueryMethodType.OrderBy] = QueryLanguageParser.OrderBy,
            [QueryMethodType.GetValue] = QueryLanguageParser.GetValue,
        };

        private static Dictionary<CriteriaComparator, int> CriteriaComparatorToToken = new Dictionary<CriteriaComparator, int>
        {
            [CriteriaComparator.Eq] = QueryLanguageParser.Eq,
            [CriteriaComparator.Gt] = QueryLanguageParser.Gt,
            [CriteriaComparator.Ge] = QueryLanguageParser.Ge,
            [CriteriaComparator.Lt] = QueryLanguageParser.Lt,
            [CriteriaComparator.Le] = QueryLanguageParser.Le,
            [CriteriaComparator.In] = QueryLanguageParser.In,
            [CriteriaComparator.NotEq] = QueryLanguageParser.NotEq,
            [CriteriaComparator.Between] = QueryLanguageParser.Between,
            [CriteriaComparator.Like] = QueryLanguageParser.Like,
            [CriteriaComparator.BeginsWith] = QueryLanguageParser.BeginsWith,
            [CriteriaComparator.EndsWith] = QueryLanguageParser.EndsWith,
        };
        
        #endregion
        
        #region Mappings

        /// <summary> Pivot expressions matching </summary>
        private static List<PivotDefinition> PivotDefinitions = new List<PivotDefinition>
        {
            new PivotDefinition { Type = PivotType.MasterData,
                Form = PivotTypeForm.Collection,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName,
                SecondaryProperty = ObjectHelper.CapacityPropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.MasterIdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.Transaction,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByMainPivotValue | PivotStorageFeatures.FilterByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.MasterIdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.Attributes,
                Form = PivotTypeForm.Collection,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName, SecondaryProperty = ObjectHelper.CodePropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.SourceIdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.Task,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                MainProperty = ObjectHelper.TaskCodePropName, SecondaryProperty = ObjectHelper.CodePropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.MasterIdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.Value,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.HelperData,
                Form = PivotTypeForm.Collection,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName, SecondaryProperty = ObjectHelper.CapacityPropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.BusinessProcess,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ObjectHelper.CreatedInfoPropName, ObjectHelper.UpdatedInfoPropName, ObjectHelper.ActivityDependencyPropName, ObjectHelper.ActivitySequencePropName, },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.Activity,
                Form = PivotTypeForm.Collection,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue | PivotStorageFeatures.FilterByMainPivotValue,
                MainProperty = ObjectHelper.ObjectCodePropName,
                SecondaryProperty = ObjectHelper.CodePropName,
                CommonProperties = new List<string> {ObjectHelper.IdPropName, ObjectHelper.DeletePropName, ObjectHelper.VersionPropName, ParserHelper.GetPivotName(PivotType.Activity), },
                IsIntent = true
            },

            new PivotDefinition { Type = PivotType.ValueValue,
                Form = PivotTypeForm.Property,
            },

            new PivotDefinition { Type = PivotType.ValueAttributes,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue | PivotStorageFeatures.StoreBySecondPivotValue,
            },

            new PivotDefinition { Type = PivotType.ValueHelperData,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue | PivotStorageFeatures.StoreBySecondPivotValue,
            },

            new PivotDefinition { Type = PivotType.ValueMasterData,
                Form = PivotTypeForm.Property,
                StorageFeatures = PivotStorageFeatures.StoreByPivotName | PivotStorageFeatures.StoreByMainPivotValue | PivotStorageFeatures.StoreBySecondPivotValue,
            },
        };

        /// <summary> Query methods types maching </summary>
        private static readonly Dictionary<QueryMethodType, string> QueryMethodMapping = new Dictionary<QueryMethodType, string>
        {
            { QueryMethodType.Set, "Set"},
            { QueryMethodType.Add, "Add"},
            { QueryMethodType.Get, "Get"},
            { QueryMethodType.Delete, "Delete"},
            { QueryMethodType.ToT, "ToT"},
            { QueryMethodType.ToMd, "ToMd"},
            { QueryMethodType.ToLocal, "ToLocal"},
            { QueryMethodType.Take, "Take"},
            { QueryMethodType.Skip, "Skip"},
            { QueryMethodType.OrderBy, "OrderBy"},
        };

        public static Dictionary<string, PivotType> StringToPivotType = new Dictionary<string, PivotType>
        {
//            [] = PivotType.Transaction,
            [GetPivotName(PivotType.Attributes)] = PivotType.Attributes,
            [GetPivotName(PivotType.Task)] = PivotType.Task,
            [GetPivotName(PivotType.HelperData)] = PivotType.HelperData,
            [GetPivotName(PivotType.MasterData)] = PivotType.MasterData,
            [GetPivotName(PivotType.Value)] = PivotType.Value,
            [GetPivotName(PivotType.Activity)] = PivotType.Activity,
            [GetPivotName(PivotType.BusinessProcess)] = PivotType.BusinessProcess,
            [GetPivotName(PivotType.ValueValue)] = PivotType.ValueValue,
            [GetPivotName(PivotType.ValueAttributes)] = PivotType.ValueAttributes,
            [GetPivotName(PivotType.ValueHelperData)] = PivotType.ValueHelperData,
            [GetPivotName(PivotType.ValueMasterData)] = PivotType.ValueMasterData,
        };

        public static Dictionary<TreeNodeType, PivotType> TreeNodeTypeToPivot = new Dictionary<TreeNodeType, PivotType>
        {
            [TreeNodeType.T] = PivotType.Transaction,
            [TreeNodeType.Ae] = PivotType.Attributes,
            [TreeNodeType.Ts] = PivotType.Task,
            [TreeNodeType.Hd] = PivotType.HelperData,
            [TreeNodeType.Md] = PivotType.MasterData,
            [TreeNodeType.Vx] = PivotType.Value,
            [TreeNodeType.Ac] = PivotType.Activity,
            [TreeNodeType.Bp] = PivotType.BusinessProcess,
            [TreeNodeType.Vvx] = PivotType.ValueValue,
            [TreeNodeType.Vae] = PivotType.ValueAttributes,
            [TreeNodeType.Vhd] = PivotType.ValueHelperData,
            [TreeNodeType.Vmd] = PivotType.ValueMasterData,
        };

        [Obsolete("Use GetFluentDelegate", true)]
        public static Dictionary<PivotType, Func<SingleQuery, string, Action<QueryNode>[], SingleQuery>>
            PivotTypesToFluentDelegates =
                new Dictionary<PivotType, Func<SingleQuery, string, Action<QueryNode>[], SingleQuery>>
                {
                    [PivotType.Transaction] = FluentExtensions.AddProperty,
                    [PivotType.Attributes] = FluentExtensions.AddCollection,
                    [PivotType.Task] = FluentExtensions.AddProperty,
                    [PivotType.HelperData] = FluentExtensions.AddCollection,
                    [PivotType.MasterData] = FluentExtensions.AddCollection,
                    [PivotType.Value] = FluentExtensions.AddProperty,
                    [PivotType.Activity] = FluentExtensions.AddProperty,
                    [PivotType.BusinessProcess] = FluentExtensions.AddProperty,
                    [PivotType.ValueValue] = FluentExtensions.AddProperty,
                    [PivotType.ValueAttributes] = FluentExtensions.AddProperty,
                    [PivotType.ValueMasterData] = FluentExtensions.AddProperty,
                    [PivotType.ValueHelperData] = FluentExtensions.AddProperty,
                };

        #endregion

        public static Func<SingleQuery, string, Action<QueryNode>[], SingleQuery> GetFluentDelegate(PivotType pivotType)
        {
            var isCollection = GetPivot(pivotType).IsCollection;
            var fluentDelegate = isCollection
                ? (Func<SingleQuery, string, Action<QueryNode>[], SingleQuery>) FluentExtensions.AddCollection
                : (Func<SingleQuery, string, Action<QueryNode>[], SingleQuery>) FluentExtensions.AddProperty;

            return fluentDelegate;
        }

        public static string GetMethod(QueryMethodType id) => QueryMethodMapping.ContainsKey(id) ? QueryMethodMapping[id] : null;
        public static PivotDefinition GetPivot(PivotType id)
        {
            return PivotDefinitions.FirstOrDefault(x => x.Type == id);
        }

        /// <summary> Convert typeValue into C# type </summary>
        /// <param name="context"> Parsed typeValue </param>
        /// <returns> Returns C# type value </returns>
        public static object GetValue(this QueryLanguageParser.TypeValueContext context)
        {
            if (context == null)
                return null;

            dynamic resultValue = null;

            var singleQuoteString = context.stringType()?.SingleQuoteString()?.GetText()?.Trim('\'');
            var doubleQuoteString = context.stringType()?.DoubleQuoteString()?.GetText()?.Trim('"');
            var guid = context.guidType()?.Guid()?.GetText();
/*
            var guid = context.guidType()?.Guid()?.GetText() ??
                       context.guidType()?.SingleQuoteGuid()?.GetText()?.Trim('\'') ??
                       context.guidType()?.DoubleQuoteGuid()?.GetText()?.Trim('"');
*/

            var integer = context.intType()?.GetText();
            var hex = context.hexType()?.GetText();
            var @float = context.floatType()?.GetText();
            var boolean = context.booleanType()?.GetText();
            var dateTime = context.dateTimeType()?.GetText();

            var array = context.array();

            if (array != null)
                resultValue = array.typeValue().Select(GetValue).ToList();
            else if (guid != null)
                resultValue = Guid.Parse(guid);
            else
            {
                var typeCode = TypeCode.Empty;

                var value = singleQuoteString ??
                            doubleQuoteString ?? guid ?? integer ?? hex ?? @float ?? boolean ?? dateTime;

                if (singleQuoteString != null)
                    typeCode = TypeCode.String;
                else if (doubleQuoteString != null)
                    typeCode = TypeCode.String;
                else if (integer != null)
                    typeCode = TypeCode.Int64;
                else if (hex != null)
                    typeCode = TypeCode.Int64;
                else if (@float != null)
                    typeCode = TypeCode.Decimal;
                else if (boolean != null)
                    typeCode = TypeCode.Boolean;
                else if (dateTime != null)
                    typeCode = TypeCode.DateTime;

                resultValue = Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
            }

            return resultValue;
        }

        public static PivotData GetPivotData(string nodeName)
        {
            var pivotName = nodeName.Split('.').First();
            var pivotValue = nodeName.Split('.').Last();
            var pivotType = ParserHelper.GetPivotTypeByName(pivotName);

//            if (!pivotType.HasValue)
//                return null;

            var pivotData = new PivotData
            {
                PivotDefinition = ParserHelper.GetPivot(pivotType ?? PivotType.Transaction),
                MainValue = pivotValue,
            };

            return pivotData;
        }

        public static string GetTokenName(int idx)
        {
            return QueryLanguageParser.tokenNames[idx].Trim('\'');
        }

        public static string GetPivotName(PivotType pivotType)
        {
            return GetTokenName(GetToken(pivotType));
        }

        public static PivotType? GetPivotTypeByName(string pivotName)
        {
            return StringToPivotType.ContainsKey(pivotName.ToLower()) ? StringToPivotType[pivotName.ToLower()] : (PivotType?) null;
        }

        [Obsolete("Use GetFluentDelegate", true)]
        public static Func<SingleQuery, string, Action<QueryNode>[], SingleQuery> GetFluentDelegateByPivotType(PivotType pivotType)
        {
            return PivotTypesToFluentDelegates[pivotType];
        }

        /// <summary> Get query language token id by PivotType </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetToken(PivotType type)
        {
            return PivotTypeToToken[type];
        }

        /// <summary> Get PivotType by query language token id </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static PivotType GetPivotType(int idx)
        {
            var type = PivotTypeToToken.Where(x => x.Value == idx).Select(x => (PivotType?)x.Key).SingleOrDefault();

            if (!type.HasValue)
                throw new KeyNotFoundException($"No PivotType associated to syntax token '{idx}'");

            return type.Value;
        }

        /// <summary> Get query language token id by CriteriaAppendType </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetToken(CriteriaAppendType type)
        {
            return CriteriaAppendTypeToToken[type];
        }

        /// <summary> Get CriteriaAppendType by query language token id </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static CriteriaAppendType GetCriteriaAppendType(int idx)
        {
            var type = CriteriaAppendTypeToToken.Where(x => x.Value == idx).Select(x => (CriteriaAppendType?)x.Key).SingleOrDefault();

            if (!type.HasValue)
                throw new KeyNotFoundException($"No CriteriaAppendType associated to syntax token '{idx}'");

            return type.Value;
        }

        /// <summary> Get query language token id by QueryMethodType </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetToken(QueryMethodType type)
        {
            return QueryMethodTypeToToken[type];
        }

        /// <summary> Get QueryMethodType by query language token id </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static QueryMethodType GetQueryMethodType(int idx)
        {
            var type = QueryMethodTypeToToken.Where(x => x.Value == idx).Select(x => (QueryMethodType?)x.Key).SingleOrDefault();

            if (!type.HasValue)
                throw new KeyNotFoundException($"No QueryMethodType associated to syntax token '{idx}'");

            return type.Value;
        }

        /// <summary> Get query language token id by CriteriaComparator </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetToken(CriteriaComparator type)
        {
            return CriteriaComparatorToToken[type];
        }

        /// <summary> Get CriteriaComparator by query language token id </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static CriteriaComparator GetCriteriaComparator(int idx)
        {
            var type = CriteriaComparatorToToken.Where(x => x.Value == idx).Select(x => (CriteriaComparator?)x.Key).SingleOrDefault();

            if (!type.HasValue)
                throw new KeyNotFoundException($"No CriteriaComparator associated to syntax token '{idx}'");

            return type.Value;
        }
    }
}
