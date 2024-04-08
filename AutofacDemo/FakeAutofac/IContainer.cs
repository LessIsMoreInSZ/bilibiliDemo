namespace FakeAutofac
{
    public interface IContainer
    {
        T Resolve<T>() where  T: class;
    }
}
