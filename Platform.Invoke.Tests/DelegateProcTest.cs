using System;

namespace Platform.Invoke.Tests
{
    public class DelegateProcTest
    {
        public DelegateProcTest()
        {
            _foo = (s) => "Hello" + s + " World!";
        }

        private readonly Func<string, string> _foo;

        public string Foo(string argument)
        {
            return _foo(argument);
        }
    }
}