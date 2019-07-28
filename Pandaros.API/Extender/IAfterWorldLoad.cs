namespace Pandaros.API.Extender
{
    public interface IAfterWorldLoadExtender : IPandarosExtention
    {
        void AfterWorldLoad();
    }
}
