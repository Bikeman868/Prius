using System;
using System.Data.SqlClient;

namespace Prius.Contracts.Interfaces.External
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
