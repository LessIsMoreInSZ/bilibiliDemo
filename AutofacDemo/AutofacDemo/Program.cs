using System;
using AutofacDemo.MovieFinder;
using FakeAutofac;

namespace AutofacDemo
{
    internal class Program
    {
        private static IContainer _container;
        private static void Main(string[] args)
        {
            InitIoC();

            var lister = _container.Resolve<MPGMovieLister>();

            foreach (var movie in lister.GetMPG())
            {
                Console.WriteLine(movie.Name);
            }
            Console.Read();
        }


        private static void InitIoC()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ListMovieFinder>().AsImplementedInterfaces();
            builder.RegisterType<DBMovieFinder>().AsImplementedInterfaces();
            builder.RegisterType<MPGMovieLister>();
            _container = builder.Build();
        }
    }
}
