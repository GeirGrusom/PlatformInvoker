PlatformInvoker
===============

Loosely bound platform invocation for .NET.

Intended to remove the need for static classes 
when using P/Invoke. Supply an interface to
the builder and get an implementation back.

What does it support?
=====================

* Mocking P/Invoke functions without manually writing a wrapper.
* Adding pre-call and post-call probing which allows exceptions
  for GetLastError() in the error handling pattern common in C
  libraries.
* Marshal attributes still work.
* Both 32-bit and 64-bit is supported.
* Unix support on Mono and .NET Core
* Can support libraries that implement extension systems like OpenAL and OpenGL by implementing
  ILibrary.

Example
=======
```csharp
using System;
using Platform.Invoke;
using System.Runtime.InteropServices;

namespace Example
{
    public interface IMessageBox
    {
        int MessageBox(IntPtr hWnd, [In]string lpText, [In]string lpCaption, uint uType);
    }

    public class Program
    {
        static void Main()
        {
            var msg = LibraryInterfaceFactory.Implement<IMessageBox>("user32");
            msg.MessageBox(IntPtr.Zero, "Hello World!", "Hello World!", 1);
        }
    }
}
```

Example with probing and attributes
===================================

```csharp
using System;
using System.Reflection;

using Platform.Invoke;
using Platform.Invoke.Attributes;

namespace Example
{
    public enum ErrorCode : uint
    {
        NoError = 0,
        InvalidOperation = 0x0502,
    }

    [Library("opengl32")]
    [EntryPointFormat("gl{0}")]
    public interface IOpenGL
    {
        [SkipProbe] // Don't invoke probe actions on this method. It would cause infinite recursion.
        ErrorCode GetError();

        void ClearColor (float red, float green, float blue, float alpha);
    }

    public class Program
    {
        static void BeginCall(MethodInfo method, IOpenGL gl)
        {
            gl.GetError(); // Clear last error state
        }

        static void EndCall(MethodInfo method, IOpenGL gl)
        {
            var error = gl.GetError();
            if(error == ErrorCode.InvalidOperation)
                throw new InvalidOperationException();
        }


        private static void Main()
        {
            var opengl = LibraryInterfaceFactory.Implement<IOpenGL>(BeginCall, EndCall);
            try
            {
                opengl.ClearColor(0, 0, 0, 1);
                Console.WriteLine("Should have thrown InvalidOperationException since there is no context bound...");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Expected exception :D");
            }
        }
    }
}
```

