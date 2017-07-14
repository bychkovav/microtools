using Platform.Utils.Events.Domain.Enums;

namespace Platform.Utils.Events.Domain.Definitions
{
    public class EventPhaseDefinition
    {
        public PhaseAction Phase { get; set; }

        public PhaseAction? PreviousPhase { get; set; }

        public string[] PhaseName { get; set; }

        public MsgType InMsgType { get; set; }

        public MsgType OutMsgType { get; set; }

        public string InEventNameTemplate { get; set; }

        public bool InEventPastTenseAction { get; set; }

        public bool? InEventPastTensePhase { get; set; }

        public string OutEventNameTemplate { get; set; }

        public bool OutEventPastTenseAction { get; set; }

        public bool? OutEventPastTensePhase { get; set; }

        public string HandlerNameTemplate { get; set; }

        public bool HandlerPastTenseAction { get; set; }

        public bool? HandlerPastTensePhase { get; set; }

        public string HandlerStackNameTemplate { get; set; }

        public bool HandlerStackPastTenseAction { get; set; }

        public bool? HandlerStackPastTensePhase { get; set; }
    }
}