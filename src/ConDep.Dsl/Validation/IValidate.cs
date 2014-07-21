using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Validation
{
	public interface IValidate
	{
		bool IsValid(Notification notification);
	}
}