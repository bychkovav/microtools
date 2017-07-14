using System.Collections.Generic;
using Platform.Utils.Events.Domain.Enums;

namespace Platform.Utils.Events.Domain.Definitions
{
    public static class EventHandlerDefinitions
    {
        #region Definitions

        public static Dictionary<EventAction, string[]> StandardActionsNames { get; } = new Dictionary
            <EventAction, string[]>
            {
                {EventAction.Initiate, new[] {"Initiate", "Initiated"}},
                {EventAction.Update, new[] {"Update", "Updated"}},
                {EventAction.Add, new[] {"Add", "Added"}},
                {EventAction.Set, new[] {"Set", "Set"}},
                {EventAction.Delete, new[] {"Delete", "Deleted"}},
                {EventAction.Get, new[] {"Fetch", "Fetched"}},
                {EventAction.Query, new[] {"Query", "Queried"}}
            };

        public static Dictionary<string, EventAction> StandardActionsStrings { get; } = new Dictionary
       <string, EventAction>
            {
                {"Initiate",EventAction.Initiate},
                {"Update",EventAction.Update},
                {"Add",EventAction.Add},
                {"Set",EventAction.Set},
                {"Delete",EventAction.Delete},
                {"Fetch",EventAction.Get},
                {"Query",EventAction.Query}
            };

        public static List<PhaseAction> PhaseActionStack = new List<PhaseAction>
        {
            PhaseAction.PreProcess,
            PhaseAction.Process,
            PhaseAction.PostProcess,
            PhaseAction.SetResult
        };

        /// <summary> 
        /// Templates to generate events and handlers for every action phase 
        /// </summary>
        public static Dictionary<PhaseAction, EventPhaseDefinition> PhaseActionTemplates { get; } =
            new Dictionary<PhaseAction, EventPhaseDefinition>
            {
                {
                    PhaseAction.PreProcess, new EventPhaseDefinition
                    {
                        Phase = PhaseAction.PreProcess,
                        PhaseName = new [] {"PreProcess", "PreProcessed"},
                        InMsgType = MsgType.Command,
                        OutMsgType = MsgType.Command,
                        HandlerStackNameTemplate = "#ACTION#_#CODE#_PreProcess",
                        HandlerNameTemplate = "PreProcess_#ACTION#_#CODE#",
                        InEventNameTemplate = "#ACTION#_#CODE#_Invoked",
                        OutEventNameTemplate = "#ACTION#_#CODE#",

                        HandlerStackPastTenseAction = false,
                        HandlerPastTenseAction = false,
                        InEventPastTenseAction = false,
                        OutEventPastTenseAction = false,

                        HandlerStackPastTensePhase = false,
                        HandlerPastTensePhase = false,
                        InEventPastTensePhase = true,
                        // OutEventPastTensePhase = false,
                    }
                },
                {
                    PhaseAction.Process, new EventPhaseDefinition
                    {
                        Phase = PhaseAction.Process,
                        PhaseName = new [] { "Process", "Processed" },
                        InMsgType = MsgType.Command,
                        OutMsgType = MsgType.Event,
                        HandlerStackNameTemplate = "#ACTION#_#CODE#_Process",
                        HandlerNameTemplate = "Process_#ACTION#_#CODE#",
                        InEventNameTemplate = "#ACTION#_#CODE#",
                        OutEventNameTemplate = "#CODE#_#ACTIONED#",

                        HandlerStackPastTenseAction = false,
                        HandlerPastTenseAction = false,
                        InEventPastTenseAction = false,
                        OutEventPastTenseAction = true,

                        HandlerStackPastTensePhase = false,
                        HandlerPastTensePhase = false,
                        // InEventPastTensePhase = true,
                        // OutEventPastTensePhase = false,
                    }
                },
                {
                    PhaseAction.PostProcess, new EventPhaseDefinition
                    {
                        Phase = PhaseAction.PostProcess,
                        PreviousPhase = PhaseAction.Process,
                        PhaseName = new [] { "PostProcess", "PostProcessed" },
                        InMsgType = MsgType.Event,
                        OutMsgType = MsgType.Event,
                        HandlerStackNameTemplate = "#CODE#_#ACTIONED#_PostProcess",
                        HandlerNameTemplate = "PostProcess_#CODE#_#ACTIONED#",
                        InEventNameTemplate = "#CODE#_#ACTIONED#",
                        OutEventNameTemplate = "#CODE#_#ACTIONED#_PostProcessed",

                        HandlerStackPastTenseAction = true,
                        HandlerPastTenseAction = true,
                        InEventPastTenseAction = true,
                        OutEventPastTenseAction = true,

                        HandlerStackPastTensePhase = false,
                        HandlerPastTensePhase = false,
                        // InEventPastTensePhase = true,
                        OutEventPastTensePhase = true,
                    }
                },
                {
                    PhaseAction.SetResult, new EventPhaseDefinition
                    {
                        Phase = PhaseAction.SetResult,
                        PreviousPhase = PhaseAction.PostProcess,
                        PhaseName = new [] { "ApiSet", "ApiSet"},
                        InMsgType = MsgType.Event,
                        OutMsgType = MsgType.Event,
                        HandlerStackNameTemplate = "#CODE#_#ACTIONED#_ApiSet",
                        HandlerNameTemplate = "ApiSet_#CODE#_#ACTIONED#",
                        InEventNameTemplate = "#CODE#_#ACTIONED#_PostProcessed",
                        OutEventNameTemplate = "",

                        HandlerStackPastTenseAction = true,
                        HandlerPastTenseAction = true,
                        InEventPastTenseAction = true,
                        OutEventPastTenseAction = true,

                        HandlerStackPastTensePhase = false,
                        HandlerPastTensePhase = false,
                        InEventPastTensePhase = true,
                        // OutEventPastTensePhase = true,
                    }
                }
            };

        public static Dictionary<TreeNodeType, List<EventAction>> StandardActions { get; } =
            new Dictionary<TreeNodeType, List<EventAction>>
            {
                [TreeNodeType.T] = new List<EventAction>
                {
                    EventAction.Initiate,
                    EventAction.Get,
                    EventAction.Query,
                    EventAction.Delete
                },
                [TreeNodeType.Ae] = new List<EventAction>
                {
                    EventAction.Get,
                    EventAction.Set,
                    EventAction.Add,
                    EventAction.Delete
                },
                [TreeNodeType.Ts] = new List<EventAction>
                {
                    EventAction.Get,
                    EventAction.Set
                },
                [TreeNodeType.Md] = new List<EventAction>
                {
                    EventAction.Get,
                    EventAction.Add,
                    EventAction.Delete
                },
                [TreeNodeType.Vx] = new List<EventAction>
                {
                    EventAction.Set,
                    EventAction.Get
                },
                [TreeNodeType.Hd] = new List<EventAction>
                {
                    EventAction.Get,
                    EventAction.Add,
                    EventAction.Delete
                },
                [TreeNodeType.Bp] = new List<EventAction>
                {
                    EventAction.Set,
                    EventAction.Get
                },
                [TreeNodeType.Ac] = new List<EventAction>
                {
                    EventAction.Get,
                    EventAction.Set,
                    EventAction.Add,
                    EventAction.Delete
                },
                [TreeNodeType.Vae] = new List<EventAction>
                {
                },
                [TreeNodeType.Vhd] = new List<EventAction>
                {
                },
                [TreeNodeType.Vmd] = new List<EventAction>
                {
                },
                [TreeNodeType.Vvx] = new List<EventAction>
                {
                },
            };

        #endregion

        /// <summary> Generates event name for custom action </summary>
        /// <param name="modelElement"></param>
        /// <param name="eventDirection"></param>
        /// <param name="actionName"></param>
        /// <param name="phaseAction"></param>
        /// <returns></returns>
        public static string GetEventCode(string path, string[] actionName, PhaseAction phaseAction, EventDirection eventDirection)
        {
            var template = eventDirection == EventDirection.In
                ? EventHandlerDefinitions.PhaseActionTemplates[phaseAction].InEventNameTemplate
                : EventHandlerDefinitions.PhaseActionTemplates[phaseAction].OutEventNameTemplate;

            return GetEventOrHandlerName(path, actionName, template);
        }

        /// <summary> Generates event name for standard action </summary>
        /// <param name="modelElement"></param>
        /// <param name="eventDirection"></param>
        /// <param name="eventAction"></param>
        /// <param name="phaseAction"></param>
        /// <returns></returns>
        public static string GetEventCode(string path, EventAction eventAction, PhaseAction phaseAction, EventDirection eventDirection)
        {
            return GetEventCode(path, StandardActionsNames[eventAction], phaseAction, eventDirection);
        }

        public static string GetEventOrHandlerName(string path, string[] actionName, string template)
        {
            var eventCode = template.Replace("#CODE#", path).Replace("#ACTION#", actionName[0]).Replace("#ACTIONED#", actionName[1]);

            return eventCode;
        }
    }
}
