namespace Platform.Utils.Events.Domain.Objects
{
    using System.Collections.Generic;
    using MsgObjects;

    public class Payload
    {
        public IList<MsgBase> Msgs { get; set; }
    }
}
