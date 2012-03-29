using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository
{
    public interface IObjectContext<out T> : IDisposable
    {
        //===============================================================
        T Value { get; }
        //===============================================================
        void SaveChanges();
        //===============================================================
    }
}
