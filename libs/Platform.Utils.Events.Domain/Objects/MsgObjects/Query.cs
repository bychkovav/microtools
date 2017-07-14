using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Events.Domain.Objects.MsgObjects
{
    using Enums;

    public class Query: MsgBase
    {
        public Query() { }

        public Query(Query q) : base(q)
        {
            ReplyBy = q.ReplyBy;
            InReplyTo = q.InReplyTo;
            ReplyTo = q.ReplyTo;
            To = new List<string>(q.To);
            Protocol = q.Protocol;
        }

        public string ReplyTo { get; set; }

        public Guid? InReplyTo { get; set; }

        public DateTime? ReplyBy { get; set; }

        public IList<string> To { get; set; }

        public Protocol Protocol { get; set; }
    }
}
