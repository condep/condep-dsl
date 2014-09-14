using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    internal interface IManageSequence<in T> : IValidate
    {
        void Add(T operation, bool addFirst = false);
    }
}