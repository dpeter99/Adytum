namespace ConfigurationManager.Installer.Repository;

public interface IRepositoryManager 
{
    Task<bool> EnableRepositoryAsync(RepositoryDescriptor repo);
}