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

Open issues
===========
* The default marshalling for strings is Ansi. 
* Be aware that the Windows API postfix Ansi functions with A 
  and Unicode with W.

Example
=======
    using System;
    using Platform.Invoke;

    namespace Example
    {
        public interface IMessageBox
        {
            int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        }

        public class Program
        {
            static void Main()
            {
                var msg = LibraryInterfaceFactory.Implement<IMessageBox>("user32", s => s + "A");
                msg.MessageBox(IntPtr.Zero, "Hello World!", "Hello World!", 1);
            }
        }
    }
    
Notice the the second argument which appends "A" to all function
call mappings in User32 in reference to the second issue.

