using System;
using System.Collections.Generic;

namespace ClientManager.Business
{
    public class BusinessValidationException : Exception
    {
        public IList<string> Errors { get; private set; }

        public BusinessValidationException(string error)
            : this(new List<string> { error })
        {
        }

        public BusinessValidationException(IList<string> errors)
            : base(string.Join(Environment.NewLine, errors))
        {
            Errors = errors;
        }
    }
}
