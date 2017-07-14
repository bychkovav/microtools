namespace Platform.Utils.Json
{
    using System;
    using Newtonsoft.Json.Linq;

    public static class ObjectHelper
    {
        #region [Ids]

        public const string VersionPropName = "version";

        public const string MasterIdPropName = "masterId";

        public const string SourceIdPropName = "sourceId";

        public const string IdPropName = "id";

        #endregion

        #region [Infra properties]

        public const string ObjectCodePropName = "objectCode";

        public const string ActivitySequencePropName = "activitySequence";

        public const string ActivityDependencyPropName = "activityDependency";

        public const string CodePropName = "code";

        public const string TaskCodePropName = "taskCode";

        public const string CapacityPropName = "capacity";

        public const string DeletePropName = "delete";

        public const string StatusPropName = "status";

        public const string CreatedInfoPropName = "createdInfo";

        public const string UpdatedInfoPropName = "updatedInfo";

        public const string DatePropName = "date";

        public const string AuthorPropName = "author";

        #endregion

        #region [Operations]

        public const string OperationPropName = "$operation";

        public const string DeleteOperation = "delete";

        public const string SetOperation = "set";

        public const string AddOperation = "add";

        public const string InitiateOperation = "initiate";

        #endregion

        public static double GetVersion(JToken obj)
        {
            return GetDouble(obj, VersionPropName);
        }

        public static Guid GetMasterId(JToken obj)
        {
            return GetId(obj, MasterIdPropName);
        }

        public static Guid GetId(JToken obj)
        {
            return GetId(obj, IdPropName);
        }

        public static JToken GetInnerObject(JToken wrapperObj)
        {
            return wrapperObj.First.First;
        }

        private static Guid GetId(JToken wrapperObj, string idPropName)
        {
            var obj = GetInnerObject(wrapperObj);
            var idToken = obj.SelectToken(idPropName);

            var idVal = idToken?.ToString();
            if (!string.IsNullOrEmpty(idVal))
            {
                return Guid.Parse(idVal);
            }

            return Guid.Empty;
        }

        private static double GetDouble(JToken wrapperObj, string propName)
        {
            var obj = GetInnerObject(wrapperObj);
            var doubleToken = obj.SelectToken(propName);

            var doubleVal = doubleToken?.ToString();
            if (!string.IsNullOrEmpty(doubleVal))
            {
                return double.Parse(doubleVal);
            }

            return 0;
        }
    }
}
