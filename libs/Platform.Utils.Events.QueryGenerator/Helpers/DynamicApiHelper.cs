namespace Platform.Utils.Events.QueryGenerator.Helpers
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Mvc;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.QueryParser.Domain.Enums;

    public static class DynamicApiHelper
    {
        #region VerbsToActions

        private static Dictionary<PivotType, Dictionary<HttpVerbs, EventAction>> VerbsToActions =
            new Dictionary<PivotType, Dictionary<HttpVerbs, EventAction>>
            {
                [PivotType.Transaction] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Post] = EventAction.Initiate,
                    [HttpVerbs.Get] = EventAction.Get,
                    //[HttpVerbs.Get] =  EventAction.Query,
                    [HttpVerbs.Delete] = EventAction.Delete
                },
                [PivotType.Attributes] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Get] = EventAction.Get,
                    [HttpVerbs.Patch] = EventAction.Set,
                    [HttpVerbs.Put] = EventAction.Add,
                    [HttpVerbs.Delete] = EventAction.Delete
                },
                [PivotType.Task] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Get] = EventAction.Get,
                    [HttpVerbs.Patch] = EventAction.Set
                },
                [PivotType.MasterData] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Get] = EventAction.Get,
                    [HttpVerbs.Put] = EventAction.Add,
                    [HttpVerbs.Delete] = EventAction.Delete
                },
                [PivotType.Value] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Patch] = EventAction.Set,
                    [HttpVerbs.Get] = EventAction.Get
                },
                [PivotType.HelperData] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Get] = EventAction.Get,
                    [HttpVerbs.Put] = EventAction.Add,
                    [HttpVerbs.Delete] = EventAction.Delete
                },
                [PivotType.BusinessProcess] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Patch] = EventAction.Set,
                    [HttpVerbs.Get] = EventAction.Get
                },
                [PivotType.Activity] = new Dictionary<HttpVerbs, EventAction>
                {
                    [HttpVerbs.Get] = EventAction.Get,
                    [HttpVerbs.Patch] = EventAction.Set,
                    [HttpVerbs.Put] = EventAction.Add,
                    [HttpVerbs.Delete] = EventAction.Delete
                }
            };

        #endregion

        public static EventAction GetEventAction(HttpVerbs verb, PivotType pivotType, bool hasIdParameter)
        {
            if (hasIdParameter == false && pivotType == PivotType.Transaction && verb == HttpVerbs.Get)
            {
                return EventAction.Query;
            }

            return VerbsToActions[pivotType][verb];
        }

        public static Dictionary<HttpMethod, HttpVerbs> HttpMethodToVerb = new Dictionary<HttpMethod, HttpVerbs>
        {
            [HttpMethod.Get] = HttpVerbs.Get,
            [HttpMethod.Delete] = HttpVerbs.Delete,
            [HttpMethod.Post] = HttpVerbs.Post,
            [HttpMethod.Put] = HttpVerbs.Put,
            [new HttpMethod("PATCH")] = HttpVerbs.Patch
        };

        private static List<EventAction> ActionsRequiredOperationInEdm = new List<EventAction>
        {
            EventAction.Add, EventAction.Delete, EventAction.Initiate, EventAction.Set, EventAction.Update
        };

        public static bool MustHaveOperationInEdm(EventAction eventAction)
        {
            return ActionsRequiredOperationInEdm.Contains(eventAction);
        }

        public static List<PivotType> IzvratPropertiesPivots = new List<PivotType>
        {
            PivotType.ValueAttributes, PivotType.ValueHelperData, PivotType.ValueMasterData
        };
    }
}