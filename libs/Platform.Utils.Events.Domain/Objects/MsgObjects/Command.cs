using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Events.Domain.Objects.MsgObjects
{
    using Enums;

    public class Command : MsgBase
    {
        public Command() { }

        public Command(Command cmd) : base(cmd)
        {
            ParentId = cmd.ParentId;
            ReplyBy = cmd.ReplyBy;
            InReplyTo = cmd.InReplyTo;
            ReplyTo = cmd.ReplyTo;
            To = new List<string>(cmd.To);
            Protocol = cmd.Protocol;
        }

        public Guid ParentId { get; set; }

        public DateTime? ReplyBy { get; set; }

        public Guid? InReplyTo { get; set; }

        public string ReplyTo { get; set; }

        public IList<string> To { get; set; }

        public Protocol Protocol { get; set; }
    }
}
