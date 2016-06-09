using System;
using System.Collections.Generic;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Mocks.Helper
{
    public interface IMockedStoredProcedure
    {
        IEnumerable<IMockedResultSet> Query(ICommand command);
        long NonQuery(ICommand command);
        T Scalar<T>(ICommand command);
    }
}
