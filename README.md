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
* Unix should work as well (any system using ld, such as Linux or OS X)
* Implementation uses ILibrary, so you can even use libraries that
  implements extension functionality such as OpenGL or OpenAL.

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

Example with probing and attributes
===================================

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
				var opengl = LibraryInterfaceFactory.Implement<IOpenGL>(BeginCall, EndCall, s => "gl" + s);
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

