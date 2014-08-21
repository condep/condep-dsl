using System;
using System.Collections.Generic;
using System.Linq;

namespace ConDep.Dsl.Validation
{
	public class Notification
	{
		private readonly List<ValidationError> _validationErrors = new List<ValidationError>();

		public bool HasErrors
		{
			get { return _validationErrors.Count > 0; }
		}

		public void AddError(ValidationError error)
		{
			_validationErrors.Add(error);
		}

	    public void Throw()
	    {
	        if(_validationErrors.Count > 0)
            {
                throw _validationErrors.Aggregate<ValidationError, Exception>(null, (current, error) => new ConDepInvalidSetupException(error.Message, current));
            }
	        throw new ConDepInvalidSetupException("Validation failed for unknown reason");
	    }
	}
}