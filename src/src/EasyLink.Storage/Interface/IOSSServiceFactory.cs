namespace EasyLink.Storage
{
    public interface IOSSServiceFactory
    {
        IOSSService Create();

        IOSSService Create(string name);
    }
}
