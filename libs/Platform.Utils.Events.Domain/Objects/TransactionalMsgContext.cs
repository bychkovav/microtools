namespace Platform.Utils.Events.Domain.Objects
{
    using System;
    using System.Collections.Generic;
    using QueryParser.Domain.Objects;

    // TODO: Maybe not the best place to store things that are connectced to transactional here. But for now it will be that way.
    public class TransactionalMsgContext : MsgContext
    {
        public TransactionalMsgContext() { }

        public TransactionalMsgContext(TransactionalMsgContext msgContext) : base(msgContext)
        {
            TransactionId = this.TransactionId;
            QueryCommands = this.QueryCommands;
        }

        public Guid TransactionId { get; set; }

        public IList<SingleQuery> QueryCommands { get; set; }

        public override object Clone()
        {
            return new TransactionalMsgContext(this);
        }
    }
}
