using ConDep.Dsl.Config;

namespace ConDep.Dsl
{
    public interface IProvideSecrets
    {
        DeploymentUserConfig GetDeploymentUser();
    }
}
