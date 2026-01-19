using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests.Client
{
    public class MultipleTestUserContext : IAsyncDisposable, IEnumerable<TestUserContext>
    {
        private readonly TestUserContext[] _ucArray;

        public TestUserContext this[int i] => _ucArray[i];
        public int Length => _ucArray.Length;

        public MultipleTestUserContext(TestUserContext[] ucArray)
        {
            _ucArray = ucArray;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var uc in _ucArray)
            {
                await uc.DisposeAsync();
            }
        }

        public IEnumerator<TestUserContext> GetEnumerator()
        {
            return _ucArray.Cast<TestUserContext>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
