using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
{
    /// <summary>
    /// Defines the mechanism for reporting errors
    /// </summary>
    public interface IErrorReporter : IDisposable
    {
        void ReportError(Exception e, string subject, params object[] otherInfo);
        void ReportError(Exception e, SqlCommand cmd, string subject, params object[] otherInfo);
    }
}
