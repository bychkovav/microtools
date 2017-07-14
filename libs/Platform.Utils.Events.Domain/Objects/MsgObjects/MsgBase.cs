namespace Platform.Utils.Events.Domain.Objects.MsgObjects
{
    using System;
    using Enums;
    using Newtonsoft.Json.Linq;

    public class MsgBase : ICloneable
    {
        public MsgBase()
        {
            
        }

        public MsgBase(MsgBase msg)
        {
            Content = msg.Content;
            Id = msg.Id;
            EventAction = msg.EventAction;
            MsgType = msg.MsgType;
            EventPhase = msg.EventPhase;
            EventPath = msg.EventPath;
            Structure = msg.Structure;
            Level = msg.Level;
            Category = msg.Category;
            ContextId = msg.ContextId;
            ModelName = msg.ModelName;
            WorkspaceId = msg.WorkspaceId;
            SourceId = msg.SourceId;
        }

        public JToken Content { get; set; }

        public Guid Id { get; set; }

        public string EventAction { get; set; }

        public MsgType MsgType { get; set; }

        [Obsolete("The logic should be changed")]
        public virtual string EventCode
        {
            get
            {
                switch (MsgType)
                {
                    case MsgType.Command:
                        return !string.IsNullOrEmpty(EventPhase)
                            ? $"{EventAction}_{EventPath}_{EventPhase}"
                            : $"{EventAction}_{EventPath}";
                    case MsgType.Event:
                        return !string.IsNullOrEmpty(EventPhase)
                            ? $"{EventPath}_{EventAction}_{EventPhase}"
                            : $"{EventPath}_{EventAction}";
                    default:
                        throw new NotSupportedException($"{MsgType} is not supported");
                }


            }
        }

        public string EventPhase { get; set; }

        public string EventPath { get; set; }

        public MsgStructure Structure { get; set; }

        public string Level { get; set; }

        public string Category { get; set; }

        public Guid ContextId { get; set; }

        public string ModelName { get; set; }

        public string WorkspaceId { get; set; }

        public Guid SourceId { get; set; }

        public virtual object Clone()
        {
            return new MsgBase(this);
        }
    }
}
