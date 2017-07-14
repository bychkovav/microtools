using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Tests
{
    using Owin;

    class Program
    {
        static void Main(string[] args)
        {
            ServiceContainer.Run(() => new TestService());
        }
    }

}
