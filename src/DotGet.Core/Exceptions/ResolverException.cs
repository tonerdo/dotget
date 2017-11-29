using System;

namespace DotGet.Core.Exceptions
{
    public class ResolverException : System.Exception
    {
        public ResolverException() { }
        public ResolverException(string message) : base(message) { }
        public ResolverException(string message, System.Exception inner) : base(message, inner) { }
        protected ResolverException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}