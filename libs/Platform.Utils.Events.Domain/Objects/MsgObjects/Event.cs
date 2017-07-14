using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Events.Domain.Objects.MsgObjects
{
    public class Event : MsgBase
    {
        public Event() { }

        public Event(Event ev) : base(ev)
        {
            ParentId = ev.ParentId;
        }

        public Guid ParentId { get; set; }
    }
}
