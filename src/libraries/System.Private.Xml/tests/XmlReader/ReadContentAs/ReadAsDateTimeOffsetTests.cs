// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Xunit;

namespace System.Xml.XmlReaderTests
{
    public class DateTimeOffsetTests
    {
        private const string NameOfXmlDataAttribute = "someDataAttribute";
        private const string NameOfXmlRootNode = "Root";

        public static IEnumerable<object[]> ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException_TestData()
        {
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  20<?9?>02-1<![CDATA[2]]>-3<!-- Comment inbetween-->0  </{NameOfXmlRootNode}>"
            };
        }

        public static IEnumerable<object[]> ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException_TestData()
        {
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}  >2002-13-30  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode} {NameOfXmlDataAttribute}='2002-02-29T23:59:59.9999999999999+13:61'/>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   001-01-01<?a?>T00:00:00+00:00   </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   21<?a?>00<!-- Comment inbetween-->-02-29T23:59:59.9999999+13:60  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   99<!-- Comment inbetween-->99-1<![CDATA[2]]>-31T01:60:5<?a?>9.99<?a?>9999<![CDATA[4]]>9<?Zz?>-00<![CDATA[:]]>00   </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  200<?a?>2-12-33</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  9<?a?>9<!-- Comment inbetween-->99  Z  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  9<?a?>999 Z</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}> ABC<?a?>D  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[z<!-- Comment inbetween-->]]></{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[Z<!-- Comment inbetween-->]]></{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><?a?>0<!-- Comment inbetween--></{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>0<?a?></{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>0<?a?></{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>0001-<![CDATA[0<!-- Comment inbetween-->1]]>-01T0<?a?>0:00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>001-01-01T0<?a?>0:00:00<!-- Comment inbetween-->+00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>001-01-01T00:00:00+00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>20<?a?>0<![CDATA[2<?a?>]]>-13-30  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>2002-12-33</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>21<!-- Comment inbetween-->00-02-29T23:59:59.9999999+13:60</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>21<?a?>00-02-29T23:59:59.9999999+13:60  </{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>3  000-0<?a?>2-29T23:59:59.999999999999-13:60</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>3000-<?a?>02-29T<!-- Comment inbetween-->23:59:59.999999999999-13:60</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>3000-02-29T23:59:<!-- Comment inbetween-->9.999999999999-13:60</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<?a?>99-12-31T12:59:59+14:00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<?a?>9Z</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>9999-1<![CDATA[2]]>-31T12:59:59+14:00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>9999-12-31T12:5<?a?>9:59+14:00:00</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>A<?a?>B<!-- Comment inbetween-->CD</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>yyy<?a?>y-MM-ddTHH:mm</{NameOfXmlRootNode}>"
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>yyyy-MM-ddTHH:mm</{NameOfXmlRootNode}>"
            };
        }

        public static IEnumerable<object[]> ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException_TestData()
        {
            foreach (object[] currentTestData in ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException_TestData())
            {
                yield return currentTestData;
            }
        }

        public static IEnumerable<object[]> ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException_TestData()
        {
            foreach (object[] currentTestData in ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException_TestData())
            {
                yield return currentTestData;
            }
        }

        public static IEnumerable<object[]> ReadContentAs_ValidXsdDateTimeOffsetValue_Success_TestData()
        {
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   9999-12-31   </{NameOfXmlRootNode}>",
                new DateTimeOffset(9999, 12, 31, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(9999, 12, 31)))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[0]]>001Z  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[9]]>999Z</{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  20<?a?>02-1<![CDATA[2]]>-3<!-- Comment inbetween-->0  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2002, 12, 30, 0, 0, 0, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[9]]>999Z</{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   2000-0<![CDATA[2]]>-29T23:59:59.999<?a?>9999   </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2000, 2, 29, 23, 59, 59, DateTimeKind.Local).AddTicks(9999999))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  2<?a?>00<!-- Comment inbetween-->0-02-29T23:59:5<?a?>9-13:<![CDATA[60]]>    </{NameOfXmlRootNode}>",
                new DateTimeOffset(2000, 2, 29, 23, 59, 59, new TimeSpan(-14, 0, 0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[2]]>00<?a?>2-1<!-- Comment inbetween-->2-30Z  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2002, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <!-- Comment inbetween-->0002-01-01T00:00:00+00:00  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<!-- Comment inbetween-->99-1<?a?>2-31T1<![CDATA[2]]>:59:59</{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 12, 31, 12, 59, 59, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  999<!-- Comment inbetween-->9  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   2000-0<![CDATA[2]]>-29T23:59:59.999<?a?>9999   </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2000, 2, 29, 23, 59, 59, DateTimeKind.Local).AddTicks(9999999))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  2<?a?>00<!-- Comment inbetween-->0-02-29T23:59:5<?a?>9-13:<![CDATA[60]]>    </{NameOfXmlRootNode}>",
                new DateTimeOffset(2000, 2, 29, 23, 59, 59, new TimeSpan(-14, 0, 0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   200<!-- Comment inbetween-->2-<![CDATA[12]]>-3<?a?>0Z   </{NameOfXmlRootNode}>",
                new DateTimeOffset(2002, 12, 30, 0, 0, 0, TimeSpan.FromHours(0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  20<?a?>02-1<![CDATA[2]]>-3<!-- Comment inbetween-->0  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2002, 12, 30, 0, 0, 0, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[00]]>01-01-01<?a?>T00:00:0<!-- Comment inbetween-->0+00:00</{NameOfXmlRootNode}>",
                new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromHours(0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}><![CDATA[9]]>99<?a?>9-12-31T12:<!-- Comment inbetween-->5<![CDATA[9]]>:5<![CDATA[9]]>+14:00</{NameOfXmlRootNode}>",
                new DateTimeOffset(9999, 12, 31, 12, 59, 59, TimeSpan.FromHours(+14))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  9<![CDATA[9]]>99-1<?a?>2-31T1<!-- Comment inbetween-->2:59:59-10:60     </{NameOfXmlRootNode}>",
                new DateTimeOffset(9999, 12, 31, 12, 59, 59, TimeSpan.FromHours(-11))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  20<!-- Comment inbetween-->05 </{NameOfXmlRootNode}>",
                new DateTimeOffset(2005, 1, 1, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(2005, 1, 1)))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}> 9<![CDATA[9]]>9<!-- Comment inbetween-->9<?a?> </{NameOfXmlRootNode}>",
                new DateTimeOffset(9999, 1, 1, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(9999, 1, 1)))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}> 0<?a?>0<!-- Comment inbetween-->01Z </{NameOfXmlRootNode}>",
                new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromHours(0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<!-- Comment inbetween-->9<![CDATA[9]]>Z<?a?></{NameOfXmlRootNode}>",
                new DateTimeOffset(9999, 1, 1, 0, 0, 0, TimeSpan.FromHours(0))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>2<!-- Comment inbetween-->000-02-29T23:5<?a?>9:5<![CDATA[9]]>.999999<![CDATA[9]]>+13:60  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2000, 2, 29, 23, 59, 59).AddTicks(9999999), TimeSpan.FromHours(14))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>    20<?a?>00-02-29T23:59:59.999999999999-<!-- Comment inbetween-->13:60</{NameOfXmlRootNode}>",
                new DateTimeOffset(2000, 3, 1, 0, 0, 0, TimeSpan.FromHours(-14))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>   0001-01-01T00<?a?>:00:00<!-- Comment inbetween-->-1<!-- Comment inbetween-->3:<![CDATA[60]]>   </{NameOfXmlRootNode}>",
                new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromHours(-14))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[2]]>00<?a?>2-1<!-- Comment inbetween-->2-30Z  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2002, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>2<?a?>00<!-- Comment inbetween-->2-12-3<![CDATA[0]]></{NameOfXmlRootNode}>",
                new DateTimeOffset(2002, 12, 30, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(2002, 12, 30)))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <!-- Comment inbetween-->0002-01-01T00:00:00+00:00  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(2, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>99<!-- Comment inbetween-->99-1<?a?>2-31T1<![CDATA[2]]>:59:59</{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 12, 31, 12, 59, 59, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  999<!-- Comment inbetween-->9  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Local))
            };
            yield return new object[]
            {
                $"<{NameOfXmlRootNode}>  <![CDATA[0]]>001Z  </{NameOfXmlRootNode}>",
                new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
            };
        }

        public static IEnumerable<object[]> ReadContentAsDateTimeOffset_ValidXsdDateTimeOffsetValue_Success_TestData()
        {
            foreach (object[] currentTestData in ReadContentAs_ValidXsdDateTimeOffsetValue_Success_TestData())
            {
                yield return currentTestData;
            }
        }

        [Theory]
        [MemberData(nameof(ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException_TestData))]
        public void ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException(string xmlWithInvalidXsdDateTimeOffset)
        {
            var reader = Utils.CreateFragmentReader(xmlWithInvalidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            if (!reader.MoveToAttribute(NameOfXmlDataAttribute))
            {
                reader.Read();
            }

            Exception throwedException = Assert.Throws<XmlException>(() => reader.ReadContentAs(typeof(DateTimeOffset), null));
            Assert.Null(throwedException.InnerException);
        }

        [Theory]
        [MemberData(nameof(ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException_TestData))]
        public void ReadContentAs_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException(string xmlWithInvalidXsdDateTimeOffset)
        {
            var reader = Utils.CreateFragmentReader(xmlWithInvalidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            if (!reader.MoveToAttribute(NameOfXmlDataAttribute))
            {
                reader.Read();
            }

            Exception throwedException = Assert.Throws<XmlException>(() => reader.ReadContentAs(typeof(DateTimeOffset), null));
            Assert.IsType<FormatException>(throwedException.InnerException);
        }

        [Theory]
        [MemberData(nameof(ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException_TestData))]
        public void ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlException(string xmlWithInvalidXsdDateTimeOffset)
        {
            var reader = Utils.CreateFragmentReader(xmlWithInvalidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            if (!reader.MoveToAttribute(NameOfXmlDataAttribute))
            {
                reader.Read();
            }

            Exception throwedException = Assert.Throws<XmlException>(() => reader.ReadContentAsDateTimeOffset());
            Assert.Null(throwedException.InnerException);
        }

        [Theory]
        [MemberData(nameof(ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException_TestData))]
        public void ReadContentAsDateTimeOffset_InvalidXsdDateTimeOffsetValue_ShouldThrowXmlExceptionWithFormatExceptionAsInnerException(string xmlWithInvalidXsdDateTimeOffset)
        {
            var reader = Utils.CreateFragmentReader(xmlWithInvalidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            if (!reader.MoveToAttribute(NameOfXmlDataAttribute))
            {
                reader.Read();
            }

            Exception throwedException = Assert.Throws<XmlException>(() => reader.ReadContentAsDateTimeOffset());
            Assert.IsType<FormatException>(throwedException.InnerException);
        }

        [Theory]
        [MemberData(nameof(ReadContentAs_ValidXsdDateTimeOffsetValue_Success_TestData))]
        public void ReadContentAs_ValidXsdDateTimeOffsetValue_Success(string xmlWithValidXsdDateTimeOffset, DateTimeOffset expectedValue)
        {
            var reader = Utils.CreateFragmentReader(xmlWithValidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            reader.Read();

            DateTimeOffset actualValue = (DateTimeOffset)reader.ReadContentAs(typeof(DateTimeOffset), null);

            Assert.Equal(expectedValue, actualValue);
        }

        [Theory]
        [MemberData(nameof(ReadContentAsDateTimeOffset_ValidXsdDateTimeOffsetValue_Success_TestData))]
        public void ReadContentAsDateTimeOffset_ValidXsdDateTimeOffsetValue_Success(string xmlWithValidXsdDateTimeOffset, DateTimeOffset expectedValue)
        {
            var reader = Utils.CreateFragmentReader(xmlWithValidXsdDateTimeOffset);
            reader.PositionOnElement(NameOfXmlRootNode);
            reader.Read();

            DateTimeOffset actualValue = reader.ReadContentAsDateTimeOffset();

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
