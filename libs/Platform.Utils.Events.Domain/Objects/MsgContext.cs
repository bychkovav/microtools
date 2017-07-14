namespace Platform.Utils.Events.Domain.Objects
{
    using System;
    using MsgObjects;
    using Newtonsoft.Json.Linq;

    public class MsgContext : ICloneable
    {
        public MsgContext()
        { }

        public MsgContext(MsgContext msgContext)
        {
            Msg = (MsgBase)msgContext.Msg.Clone();
            this.Data = msgContext.Data;
        }

        public MsgBase Msg { get; set; }

        public JToken Data { get; set; }

        public virtual object Clone()
        {
            return new MsgContext(this);
        }
    }
}
