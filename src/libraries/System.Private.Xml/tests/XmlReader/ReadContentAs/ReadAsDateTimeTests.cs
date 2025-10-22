// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Xunit;

namespace System.Xml.XmlReaderTests
{
    public class DateTimeTests
    {
        private const string NameOfXmlDataAttribute = "someDataAttribute";
        private const string NameOfXmlRootNode = "Root";

        public static IEnumerable<object[]> ReadContentAs_InvalidXsdDateTimeValue_ShouldThrowXmlException_TestData()
        {
            yield return new object[] { $"<{NameOfXmlRootNode} {NameOfXmlDataAttribute}='2002-02-29T23:59:59.9999999999999+13:61'/>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>   99<!-- Comment inbetween-->99-1<![CDATA[2]]>-31T01:60:5<?a?>9.99<?a?>9999<![CDATA[4]]>9<?Zz?>-00<![CDATA[:]]>00   </{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}  >2002-13-30  </{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>2002-12-33</{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>3  000-0<?a?>2-29T23:59:59.999999999999-13:60</{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>21<?a?>00-02-29T23:59:59.9999999+13:60  </{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>yyy<?a?>y-MM-ddTHH:mm</{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}> ABC<?a?>D  </{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>  9<?a?>999 Z</{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>0<?a?></{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>99<?a?>99-12-31T12:59:59+14:00:00</{NameOfXmlRootNode}>" };
            yield return new object[] { $"<{NameOfXmlRootNode}>001-01-01T00:00:00+00:00</{NameOfXmlRootNode}>" };
        }

        public static IEnumerable<object[]> ReadContentAs_ValidXsdDateTimeValue_Success_TestData()
        {
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   9999-12-31   </{NameOfXmlRootNode}>",
                new DateTime(9999, 12, 31, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  2<?a?>00<!-- Comment inbetween-->0-02-29T23:59:5<?a?>9-13:<![CDATA[60]]>    </{NameOfXmlRootNode}>",
                new DateTime(2000, 2, 29, 23, 59, 59).Add(TimeZoneInfo.Local.GetUtcOffset(new DateTime(2000, 2, 29)) + new TimeSpan(14, 0, 0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>0001-<![CDATA[01]]>-01T0<?a?>0:00:00<!-- Comment inbetween--></{NameOfXmlRootNode}>",
                new DateTime(1, 1, 1, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  20<?a?>02-1<![CDATA[2]]>-3<!-- Comment inbetween-->0  </{NameOfXmlRootNode}>",
                new DateTime(2002, 12, 30, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[2]]>00<?a?>2-1<!-- Comment inbetween-->2-30Z  </{NameOfXmlRootNode}>",
                new DateTime(2002, 12, 30, 0, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <!-- Comment inbetween-->0002-01-01T00:00:00+00:00  </{NameOfXmlRootNode}>",
                new DateTime(2, 1, 1, 0, 0, 0).Add(TimeZoneInfo.Local.GetUtcOffset(new DateTime(2, 1, 1)))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<!-- Comment inbetween-->99-1<?a?>2-31T1<![CDATA[2]]>:59:59</{NameOfXmlRootNode}>",
                new DateTime(9999, 12, 31, 12, 59, 59)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  0<?a?>0:0<!-- Comment inbetween-->0:00+00:00   </{NameOfXmlRootNode}>",
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Utc).ToLocalTime()
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>00<!-- Comment inbetween-->01</{NameOfXmlRootNode}>",
                new DateTime(1, 1, 1, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  999<!-- Comment inbetween-->9  </{NameOfXmlRootNode}>",
                new DateTime(9999, 1, 1, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[0]]>001Z  </{NameOfXmlRootNode}>",
                new DateTime(1, 1, 1, 0, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[9]]>999Z</{NameOfXmlRootNode}>",
                new DateTime(9999, 1, 1, 0, 0, 0, 0)
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   2000-0<![CDATA[2]]>-29T23:59:59.999<?a?>9999   </{NameOfXmlRootNode}>",
                new DateTime(2000, 2, 29, 23, 59, 59).AddTicks(9999999)
            };
        }

        [Theory]
        [MemberData(nameof(ReadContentAs_InvalidXsdDateTimeValue_ShouldThrowXmlException_TestData))]
        public void ReadContentAs_InvalidXsdDateTimeValue_ShouldThrowXmlException(string xmlWithInvalidXsdDateTime)
        {
            var reader = Utils.CreateFragmentReader(xmlWithInvalidXsdDateTime);
            reader.PositionOnElement(NameOfXmlRootNode);
            if (!reader.MoveToAttribute(NameOfXmlDataAttribute))
            {
                reader.Read();
            }

            Exception throwedException = Assert.Throws<XmlException>(() => reader.ReadContentAs(typeof(DateTime), null));
            Assert.IsType<FormatException>(throwedException.InnerException);
        }

        [Theory]
        [MemberData(nameof(ReadContentAs_ValidXsdDateTimeValue_Success_TestData))]
        public void ReadContentAs_ValidXsdDateTimeValue_Success(string xmlWithValidXsdDateTime, DateTime expectedValue)
        {
            var reader = Utils.CreateFragmentReader(xmlWithValidXsdDateTime);
            reader.PositionOnElement(NameOfXmlRootNode);
            reader.Read();

            DateTime actualValue = (DateTime) reader.ReadContentAs(typeof(DateTime), null);

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
