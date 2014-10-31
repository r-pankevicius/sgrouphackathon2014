using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace GenerateFiles
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(Add("123", "456"), "579");
            Assert.AreEqual(Add("111", "222"), "333");
            Assert.AreEqual(Add("222", "119"), "341");
            Assert.AreEqual(Add("222", "191"), "413");
            Assert.AreEqual(Add("222", "999"), "1221");
            Assert.AreEqual(Add("200", "999"), "1199");
            Assert.AreEqual(Add("11111", "22"), "11133");
            Assert.AreEqual(Add("22", "11111"), "11133");

            // a + b = c
            // c - b = a
            // c - a = b
            // -a -b = -c
            // b - c = -a
            // a - c = -b
        }

        private string Add(string first, string second)
        {
            using (var a = new MemoryStream(Encoding.ASCII.GetBytes(first)))
            using (var b = new MemoryStream(Encoding.ASCII.GetBytes(second)))
            {
                char[] digits = Program.Add(a, b).ToArray();
                string sum = digits.Aggregate(string.Empty, (s, c) => s + c, s => s);
                return sum;
            }
        }

    }
}
