using System;
using System.Collections.Generic;

namespace FakeAutofac
{
    public class ContainerBuilder
    {
        //用来保存注册的类型
        private readonly Dictionary<Type, Resolver> _typePool = new Dictionary<Type, Resolver>();
        private Type _currentKey;

        public ContainerBuilder RegisterType<T> () where T:class
        {
            _currentKey = typeof (T);
            var resolver = new Resolver { RealType = _currentKey };
            _typePool[_currentKey] = resolver;

            return this;
        }


        public ContainerBuilder AsImplementedInterfaces()
        {
            //以接口为key, 对应Resolver
            var interfaces = _currentKey.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                _typePool[@interface] = _typePool[_currentKey];
            }
            return this;
        }

        //创建Container
        public IContainer Build()
        {
            var container = new Container(_typePool);

            return container;
        }
    }
}
