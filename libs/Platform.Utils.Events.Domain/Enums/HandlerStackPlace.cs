namespace Platform.Utils.Events.Domain.Enums
{
    using System;

    [Flags]
    public enum HandlerStackPlace
    {
        //Stage
        InfraInit = 1 << 12,
        LogicProcess = 1 << 11,
        InfraDispose = 1 << 10,

        //Phase
        Init = 1 << 9,
        Process = 1 << 8,
        Dispose = 1 << 7,

        //Flow
        Execute = 1 << 6,
        Codition = 1 << 5,

        //Slot
        Override = 1 << 4,
        CatchAll = 1 << 3,
        Extend = 1 << 2,
        Kill = 1 << 1
    }
}
