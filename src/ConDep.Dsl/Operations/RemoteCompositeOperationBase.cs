using System.Collections.Generic;
using System.IO;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Operations
{
    public abstract class RemoteCompositeOperationBase
    {
        public abstract string Name { get; }
        public abstract bool IsValid(Notification notification);
    }
}