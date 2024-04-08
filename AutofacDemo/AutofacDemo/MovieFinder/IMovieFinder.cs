using System.Collections.Generic;

namespace AutofacDemo.MovieFinder
{
    public interface IMovieFinder
    {
        List<Movie> FindAll();
    }
}
