using System;
using System.Collections.Generic;

namespace FakeAutofac
{
    public class Resolver
    {
        public Type RealType { get; set; }
        //用该委托来获取构造函数的参数实例
        public Func<Type, object> GetParameterInstance { get; set; } 
       
        public object GetInstance()
        {
            //取得Type的构造函数, 这里为了简便，默认使用最后一个，实际的Autofac默认会使用参数最长的一个
            var constructors = RealType.GetConstructors();
            var paramsInfos = constructors[constructors.Length - 1].GetParameters();
            //准备构造函数的参数
            var @params = new List<object>();
            foreach (var parameterInfo in paramsInfos)
            {
                //根据类型，取的参数的实例
                var tempPara = GetParameterInstance(parameterInfo.ParameterType);
                @params.Add(tempPara);
            }

            //调用构造函数构造对象
            return constructors[0].Invoke(@params.ToArray());
        }
    }
}
