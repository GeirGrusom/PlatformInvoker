using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Platform.Invoke.CoreTests.Windows
{
    [TestFixture]
    public class WindowsLibrary_Tests
    {
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary([In]string filename);

        delegate bool PerformanceTimer(out long counter);

        [Test]
        public void GetProcedure_ReturnsCorrectProcedure()
        {
            // Arrange
            var lib = new Invoke.Windows.WindowsLibrary(LoadLibrary("kernel32"), "Kernel32");

            // Act
            var perf = lib.GetProcedure<PerformanceTimer>("QueryPerformanceFrequency");

            // Assert
            perf(out long frequency);
            Assert.That(frequency, Is.EqualTo(System.Diagnostics.Stopwatch.Frequency));
        }
    }
}
