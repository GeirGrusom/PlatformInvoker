using System;

namespace Platform.Invoke.Tests
{
    public class DelegateProcTest
    {
        public DelegateProcTest()
        {
            _foo = () => "Hello World!";
        }

        private readonly Func<string> _foo;

        public string Foo()
        {
            return _foo();
        }
    }
}