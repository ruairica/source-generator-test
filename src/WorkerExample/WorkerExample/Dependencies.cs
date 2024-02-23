namespace WorkerExample;

public interface IService1
{
    bool Method1();  
}

public class Service1: IService1
{
    private readonly IService2 _service2;

    public Service1(IService2 service2)
    {
        _service2 = service2;
    }

    public bool Method1() => _service2.Method2();
}


public interface IService2
{
    bool Method2();
}
public class Service2(IRepository1 repository1, IRepository2 repository2, IEnumerable<IMultipleImplementation> repos) : IService2
{
    public bool Method2()
    {
        var x = repository1.Method1() && repository2.Method1() && repos.All(x => x.Method1());
        return x;
    }
}

public interface  IRepository1
{
    bool Method1();
}

public class Repository1: IRepository1
{
    public bool Method1() => true;
}

public interface IRepository2
{
    bool Method1();
}

public class Repository2 : IRepository2
{
    public bool Method1() => true;
}

public interface IMultipleImplementation
{
    bool Method1();
}


public class Impl1 : IMultipleImplementation
{
    public bool Method1() => true;
}

public class Impl2 : IMultipleImplementation
{
    public bool Method1() => false;
}