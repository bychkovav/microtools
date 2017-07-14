using System;

namespace Platform.Utils.Domain.Objects.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when entity canot be found.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        /// <summary>
        ///     Gets/Sets requested entity id.
        /// </summary>
        public object EntityId { get; private set; }
        /// <summary>
        ///     Gets/Sets requested entity type
        /// </summary>
        public Type EntityType { get; private set; }

        public EntityNotFoundException(){}

        public EntityNotFoundException(Type type, object id)
            : this(string.Format("Entity '{0}' with Id '{1}' is not found.", type, id))
        {
            EntityId = id;
            EntityType = type;
        }

        public EntityNotFoundException(string message):base(message){}

        public EntityNotFoundException(string message,Exception innerException):base(message,innerException){}
    }
}
