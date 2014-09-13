using ConDep.Dsl.Validation;

namespace ConDep.Dsl.Sequence
{
    public interface IManageSequence<in T> : IValidate
    {
        void Add(T operation, bool addFirst = false);
    }
}