PlatformInvoker
===============

Loosely bound platform invocation for .NET.

Intended to remove the need for static classes 
when using P/Invoke. Supply an interface to
the builder and get an implementation back.

Will also allow exception handling of the 
GetLastError() error handling pattern.
