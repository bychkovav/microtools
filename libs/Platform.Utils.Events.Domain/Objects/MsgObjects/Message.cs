using System;
using System.Collections.Generic;

namespace Platform.Utils.Events.Domain.Objects.MsgObjects
{
    using Enums;

    public class Message : MsgBase
    {
        public Message() { }

        public Message(Message mes) : base(mes)
        {
            ReplyBy = mes.ReplyBy;
            InReplyTo = mes.InReplyTo;
            ReplyTo = mes.ReplyTo;
            To = new List<string>(mes.To);
            Protocol = mes.Protocol;
        }

        public DateTime? ReplyBy { get; set; }

        public Guid? InReplyTo { get; set; }

        public IList<string> To { get; set; }

        public string ReplyTo { get; set; }

        public Protocol Protocol { get; set; }
    }
}
