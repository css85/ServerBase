using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Integration.Tests.Base;
using Shared;
using Shared.CsvData;

namespace Integration.Tests.Tests
{
    public partial class UserTestBase
    {
        private static int _storeUniqueIndex = 910000;

        private int GetStoreUniqueIndex()
        {
            return Interlocked.Increment(ref _storeUniqueIndex);
        }


        private void SetData(object data, string propertyName, object value)
        {
            data.GetType().GetProperty(propertyName)!.SetValue(data, value);
        }
    }
}