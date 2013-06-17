using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryException : Exception
    {
        //===============================================================
        public RepositoryException(String errorMessage, Exception innerException = null)
            :base(errorMessage, innerException)
        {}
        //===============================================================
    }
}
