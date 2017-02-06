using System;
using System.Collections.Generic;
using System.IO;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Procedures
{
    public class NativeExecutionContext
    {
        public IList<IParameter> Parameters;
        public TextWriter MessageOutput;
        public string DataShapeName;
        public Action<IDataReader> CloseAction;
        public Action<IDataReader> ErrorAction;
    }
}
