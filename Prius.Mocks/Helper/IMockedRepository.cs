using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Mocks.Helper
{
    public interface IMockedRepository
    {
        string Name { get; }
        IMockedStoredProcedure GetProcedure(string name);
    }
}
