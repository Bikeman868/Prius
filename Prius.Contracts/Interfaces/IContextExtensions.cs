using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces
{
    public static class IContextExtensions
    {
        public static IDataReader ExecuteReader(this IContext context, ICommand command)
        {
            return context.EndExecuteReader(context.BeginExecuteReader(command));
        }

        public static IDataEnumerator<T> ExecuteEnumerable<T>(this IContext context, ICommand command, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class
        {
            return context.EndExecuteEnumerable<T>(context.BeginExecuteEnumerable(command), dataSetName, dataContractFactory);
        }

        public static long ExecuteNonQuery(this IContext context, ICommand command)
        {
            return context.EndExecuteNonQuery(context.BeginExecuteNonQuery(command));
        }

        public static T ExecuteScalar<T>(this IContext context, ICommand command)
        {
            return context.EndExecuteScalar<T>(context.BeginExecuteScalar(command));
        }
    }
}
