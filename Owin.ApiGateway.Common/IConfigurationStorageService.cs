namespace Owin.ApiGateway.Common
{
    public interface IConfigurationStorageService
    {
        string Read();

        void Write(string configurationString);
    }
}
