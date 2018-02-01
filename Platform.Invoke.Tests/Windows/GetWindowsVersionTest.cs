using NUnit.Framework;
using Platform.Invoke.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke.Tests.Windows
{
    [TestFixture]
    public class GetWindowsVersionTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Assert.Inconclusive("Test is not applicable to this operating system.");
            }
        }

        public interface IGetWindowsVersion
        {
            int GetVersion();
        }

        public interface IGetWindowsDirectory
        {
            int GetSystemWindowsDirectory([Out][MarshalAs(UnmanagedType.LPTStr, SizeParamIndex = 1)]string lpBuffer, uint size);
        }

        [Test]
        public void GetVersion_ReturnsNotNull()
        {
            var interf = LibraryInterfaceFactory.Implement<IGetWindowsVersion>("kernel32");

            var retVal = interf.GetVersion();

            Assert.That(retVal, Is.Not.EqualTo(0));
        }

        [Test]
        public void GetWindowsDirectory_ReturnsWindowsDirectory()
        {
            var interf = LibraryInterfaceFactory.Implement<IGetWindowsDirectory>("kernel32");

            string result = new string('\0', 1024);
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var retVal = interf.GetSystemWindowsDirectory(result, 1024);
            result = result.Substring(0, retVal);
            Assert.That(result, Is.EqualTo(winDir));

        }
    }
}
