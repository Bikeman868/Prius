using Prius.Contracts.Interfaces.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UsersTestApp.Integration
{
    internal class TraceWriter: ITraceWriter
    {
        private readonly string _id;
        private readonly string _repository;
        private string _cluster;
        private string _database;
        private string _procedure;

        public TraceWriter(string repository)
        {
            _id = Guid.NewGuid().ToString("n").Substring(0, 6);
            _repository = repository;
        }

        public void SetCluster(string clusterName)
        {
            _cluster = clusterName;
        }

        public void SetDatabase(string databaseName)
        {
            _database = databaseName;
        }

        public void SetProcedure(string storedProcedureName)
        {
            _procedure = storedProcedureName;
        }

        public void WriteLine(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var line = _id + " " + _repository;

            if (!string.IsNullOrEmpty(_cluster))
                line += " -> " + _cluster;

            if (!string.IsNullOrEmpty(_database))
                line += " -> " + _database;

            if (!string.IsNullOrEmpty(_procedure))
                line += " -> " + _procedure;

            Console.WriteLine(line + " : " + message);
        }
    }
}
