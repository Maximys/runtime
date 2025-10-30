// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OLEDB.Test.ModuleCore;
using XmlCoreTest.Common;
using Xunit;

namespace System.Xml.XmlWriterApiTests
{
    //[TestCase(Name = "WriteFullEndElement")]
    public class TCFullEndElement
    {
        // Sanity test for WriteFullEndElement()
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_1(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                w.WriteStartElement("Root");
                w.WriteFullEndElement();
            }
            Assert.True(utils.CompareReader("<Root></Root>"));
        }

        // Call WriteFullEndElement before calling WriteStartElement
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_2(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                try
                {
                    w.WriteFullEndElement();
                }
                catch (InvalidOperationException e)
                {
                    CError.WriteLineIgnore("Exception: " + e.ToString());
                    CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                    return;
                }
            }
            CError.WriteLine("Did not throw exception");
            Assert.Fail();
        }

        // Call WriteFullEndElement after WriteEndElement
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_3(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                try
                {
                    w.WriteStartElement("Root");
                    w.WriteEndElement();
                    w.WriteFullEndElement();
                }
                catch (InvalidOperationException e)
                {
                    CError.WriteLineIgnore("Exception: " + e.ToString());
                    CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                    return;
                }
            }
            CError.WriteLine("Did not throw exception");
            Assert.Fail();
        }

        // Call WriteFullEndElement without closing attributes
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_4(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                w.WriteStartElement("Root");
                w.WriteStartAttribute("a");
                w.WriteString("b");
                w.WriteFullEndElement();
            }
            Assert.True(utils.CompareReader("<Root a=\"b\"></Root>"));
        }

        // Call WriteFullEndElement after WriteStartAttribute
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_5(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                w.WriteStartElement("Root");
                w.WriteStartAttribute("a");
                w.WriteFullEndElement();
            }
            Assert.True(utils.CompareReader("<Root a=\"\"></Root>"));
        }

        // WriteFullEndElement for 100 nested elements
        [Theory]
        [XmlWriterInlineData]
        public void fullEndElement_6(XmlWriterUtils utils)
        {
            using (XmlWriter w = utils.CreateWriter())
            {
                for (int i = 0; i < 100; i++)
                {
                    string eName = "Node" + i.ToString();
                    w.WriteStartElement(eName);
                }
                for (int i = 0; i < 100; i++)
                    w.WriteFullEndElement();

                w.Dispose();
                Assert.True(utils.CompareBaseline("100FullEndElements.txt"));
            }
        }

        //[TestCase(Name = "Element Namespace")]
        public partial class TCElemNamespace
        {
            // Multiple NS decl for same prefix on an element
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", "x", null, "foo");
                        w.WriteAttributeString("xmlns", "x", null, "bar");
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Multiple NS decl for same prefix (same NS value) on an element
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", "x", null, "foo");
                        w.WriteAttributeString("xmlns", "x", null, "foo");
                        w.WriteEndElement();
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Element and attribute have same prefix, but different namespace value
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "foo");
                    w.WriteAttributeString("x", "a", "bar", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<~f x a~:Root ~a p1 a~:a=\"b\" xmlns:~a p1 A~=\"bar\" xmlns:~f x A~=\"foo\" />"));
            }

            // Nested elements have same prefix, but different namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "foo");
                    w.WriteStartElement("x", "level1", "bar");
                    w.WriteStartElement("x", "level2", "blah");
                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x:Root xmlns:x=\"foo\"><x:level1 xmlns:x=\"bar\"><x:level2 xmlns:x=\"blah\" /></x:level1></x:Root>"));
            }

            // Mapping reserved prefix xml to invalid namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("xml", "Root", "blah");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Mapping reserved prefix xml to correct namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("xml", "Root", "http://www.w3.org/XML/1998/namespace");
                    w.WriteEndElement();
                }

                Assert.True(utils.CompareReader("<xml:Root />"));
            }

            // Write element with prefix beginning with xml
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartElement("xmlA", "elem1", "test");
                    w.WriteEndElement();
                    w.WriteStartElement("xMlB", "elem2", "test");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><xmlA:elem1 xmlns:xmlA=\"test\" /><xMlB:elem2 xmlns:xMlB=\"test\" /></Root>"));
            }

            // Reuse prefix that refers the same as default namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "foo", "uri-1");
                    w.WriteStartElement("", "bar", "uri-1");
                    w.WriteStartElement("x", "bop", "uri-1");
                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x:foo xmlns:x=\"uri-1\"><bar xmlns=\"uri-1\"><x:bop /></bar></x:foo>"));
            }

            // Should throw error for prefix=xmlns
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_9(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("xmlns", "localname", "uri:bogus");
                        w.WriteEndElement();
                    }
                    catch (Exception e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw error");
                Assert.Fail();
            }

            // Create nested element without prefix but with namespace of parent element with a defined prefix
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_10(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "x", null, "fo");
                    w.WriteStartElement("level1", "fo");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:x=\"fo\"><x:level1 /></Root>"));
            }

            // Create different prefix for element and attribute that have same namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_11(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "foo");
                    w.WriteAttributeString("y", "attr", "foo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x:Root y:attr=\"b\" xmlns:y=\"foo\" xmlns:x=\"foo\" />"));
            }

            // Create same prefix for element and attribute that have same namespace
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_12(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "foo");
                    w.WriteAttributeString("x", "attr", "foo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x:Root x:attr=\"b\" xmlns:x=\"foo\" />"));
            }

            // Try to re-define NS prefix on attribute which is already defined on an element
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_13(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "foo");
                    w.WriteAttributeString("x", "attr", "bar", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<~f x a~:Root ~a p1 a~:attr=\"test\" xmlns:~a p1 A~=\"bar\" xmlns:~f x A~=\"foo\" />"));
            }

            // Namespace string contains surrogates, reuse at different levels
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_14(XmlWriterUtils utils)
            {
                string uri = "urn:\uD800\uDC00";

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteAttributeString("xmlns", "pre", null, uri);
                    w.WriteElementString("elt", uri, "text");
                    w.WriteEndElement();
                }
                string strExpected = string.Format("<root xmlns:pre=\"{0}\"><pre:elt>text</pre:elt></root>", uri);
                Assert.True(utils.CompareReader(strExpected));
            }

            // Namespace containing entities, use at multiple levels
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_15(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string strxml = "<?xml version=\"1.0\" ?><root xmlns:foo=\"urn:&lt;&gt;\"><foo:elt1 /><foo:elt2 /><foo:elt3 /></root>";

                    XmlReader xr = ReaderHelper.Create(new StringReader(strxml));
                    w.WriteNode(xr, false);
                    xr.Dispose();
                }
                Assert.True(utils.CompareReader("<root xmlns:foo=\"urn:&lt;&gt;\"><foo:elt1 /><foo:elt2 /><foo:elt3 /></root>"));
            }

            // Verify it resets default namespace when redefined earlier in the stack
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_16(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("", "x", "foo");
                    w.WriteAttributeString("xmlns", "foo");
                    w.WriteStartElement("", "y", "");
                    w.WriteStartElement("", "z", "foo");
                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x xmlns=\"foo\"><y xmlns=\"\"><z xmlns=\"foo\" /></y></x>"));
            }

            // The default namespace for an element can not be changed once it is written out
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_17(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", null, "test");
                        w.WriteEndElement();
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Map XML NS 'http://www.w3.org/XML/1998/namaespace' to another prefix
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_18(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("foo", "bar", "http://www.w3.org/XML/1998/namaespace");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<foo:bar xmlns:foo=\"http://www.w3.org/XML/1998/namaespace\" />"));
            }

            // Pass NULL as NS to WriteStartElement
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_19(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("foo", "Root", "NS");
                    w.WriteStartElement("bar", null);
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<foo:Root xmlns:foo=\"NS\"><bar /></foo:Root>"));
            }

            // Write element in reserved XML namespace, should error
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_20(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("foo", "Root", "http://www.w3.org/XML/1998/namespace");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                Assert.Fail();
            }

            // Write element in reserved XMLNS namespace, should error
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_21(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("foo", "Root", "http://www.w3.org/XML/1998/namespace");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                Assert.Fail();
            }

            // Mapping a prefix to empty ns should error
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_22(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("pre", "test", string.Empty);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Pass null prefix to WriteStartElement()
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_23(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement(null, "Root", "ns");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns='ns' />"));
            }

            // Pass String.Empty prefix to WriteStartElement()
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_24(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement(string.Empty, "Root", "ns");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns='ns' />"));
            }

            // Pass null ns to WriteStartElement()
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_25(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root", null);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root />"));
            }

            // Pass String.Empty ns to WriteStartElement()
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_26(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root", string.Empty);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root />"));
            }

            // Pass null prefix to WriteStartElement() when namespace is in scope
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_27(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteElementString(null, "child", "ns", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root xmlns:pre='ns'><pre:child>test</pre:child></pre:Root>"));
            }

            // Pass String.Empty prefix to WriteStartElement() when namespace is in scope
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_28(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteElementString(string.Empty, "child", "ns", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root xmlns:pre='ns'><child xmlns='ns'>test</child></pre:Root>"));
            }

            // Pass null ns to WriteStartElement() when prefix is in scope
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_29(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteElementString("pre", "child", null, "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root xmlns:pre='ns'><pre:child>test</pre:child></pre:Root>"));
            }

            // Pass String.Empty ns to WriteStartElement() when prefix is in scope
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_30(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("pre", "Root", "ns");
                        w.WriteElementString("pre", "child", string.Empty, "test");
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                }
                Assert.Fail();
            }

            // Pass String.Empty ns to WriteStartElement() when prefix is in scope
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_31(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("pre", "Root", "ns");
                        w.WriteElementString("pre", "child", string.Empty, "test");
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                }
                Assert.Fail();
            }

            // Mapping empty ns uri to a prefix should error
            [Theory]
            [XmlWriterInlineData]
            public void elemNamespace_32(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("prefix", "localname", null);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        return;
                    }
                }
                Assert.Fail();
            }
        }

        //[TestCase(Name = "Attribute Namespace")]
        public partial class TCAttrNamespace
        {
            // Define prefix 'xml' with invalid namespace URI 'foo'
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", "xml", null, "foo");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Bind NS prefix 'xml' with valid namespace URI
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "xml", null, "http://www.w3.org/XML/1998/namespace");
                    w.WriteEndElement();
                }
                string exp = (utils.WriterType == WriterType.UnicodeWriter) ? "<Root />" : "<Root xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" />";
                Assert.True(utils.CompareReader(exp));
            }

            // Bind NS prefix 'xmlA' with namespace URI 'foo'
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "xmlA", null, "foo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:xmlA=\"foo\" />"));
            }

            // Write attribute xml:space with correct namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "default");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xml:space=\"default\" />"));
            }

            // Write attribute xml:space with incorrect namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xml", "space", "foo", "default");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Write attribute xml:lang with incorrect namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xml", "lang", "foo", "EN");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }


            // WriteAttribute, define namespace attribute before value attribute
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "x", null, "fo");
                    w.WriteAttributeString("a", "fo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:x=\"fo\" x:a=\"b\" />"));
            }

            // WriteAttribute, define namespace attribute after value attribute
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", "fo", "b");
                    w.WriteAttributeString("xmlns", "x", null, "fo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root x:a=\"b\" xmlns:x=\"fo\" />"));
            }

            // WriteAttribute, redefine prefix at different scope and use both of them
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_9(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("level1");
                    w.WriteAttributeString("xmlns", "x", null, "fo");
                    w.WriteAttributeString("a", "fo", "b");
                    w.WriteStartElement("level2");
                    w.WriteAttributeString("xmlns", "x", null, "bar");
                    w.WriteAttributeString("c", "bar", "d");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<level1 xmlns:x=\"fo\" x:a=\"b\"><level2 xmlns:x=\"bar\" x:c=\"d\" /></level1>"));
            }

            // WriteAttribute, redefine namespace at different scope and use both of them
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_10(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("level1");
                    w.WriteAttributeString("xmlns", "x", null, "fo");
                    w.WriteAttributeString("a", "fo", "b");
                    w.WriteStartElement("level2");
                    w.WriteAttributeString("xmlns", "y", null, "fo");
                    w.WriteAttributeString("c", "fo", "d");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<level1 xmlns:x=\"fo\" x:a=\"b\"><level2 xmlns:y=\"fo\" y:c=\"d\" /></level1>"));
            }

            // WriteAttribute with colliding prefix with element
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_11(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "fo");
                    w.WriteAttributeString("x", "a", "bar", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<~f x a~:Root ~a p1 a~:a=\"b\" xmlns:~a p1 A~=\"bar\" xmlns:~f x A~=\"fo\" />"));
            }

            // WriteAttribute with colliding namespace with element
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_12(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("x", "Root", "fo");
                    w.WriteAttributeString("y", "a", "fo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<x:Root y:a=\"b\" xmlns:y=\"fo\" xmlns:x=\"fo\" />"));
            }

            // WriteAttribute with namespace but no prefix
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_13(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("a", "fo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<Root ~a p1 a~:a=\"b\" xmlns:~a p1 A~=\"fo\" />"));
            }

            // WriteAttribute for 2 attributes with same prefix but different namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_14(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", "fo", "b");
                    w.WriteAttributeString("x", "c", "bar", "d");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<Root ~f x a~:a=\"b\" ~a p2 a~:c=\"d\" xmlns:~a p2 A~=\"bar\" xmlns:~f x A~=\"fo\" />"));
            }

            // WriteAttribute with String.Empty and null as namespace and prefix values
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_15(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString(null, "a", null, "b");
                    w.WriteAttributeString(string.Empty, "c", string.Empty, "d");
                    w.WriteAttributeString(null, "e", string.Empty, "f");
                    w.WriteAttributeString(string.Empty, "g", null, "h");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"b\" c=\"d\" e=\"f\" g=\"h\" />"));
            }

            // WriteAttribute to manually create attribute of xmlns:x
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_16(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "x", null, "test");
                    w.WriteStartElement("x", "level1", null);
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:x=\"test\"><x:level1 /></Root>"));
            }

            // WriteAttribute with namespace value = null while a prefix exists
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_17(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", null, "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"b\" />"));
            }

            // WriteAttribute with namespace value = String.Empty while a prefix exists
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_18(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", string.Empty, "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"b\" />"));
            }


            // WriteAttribe in nested elements with same namespace but different prefix
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_19(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("a", "x", "fo", "y");
                    w.WriteAttributeString("xmlns", "a", null, "fo");
                    w.WriteStartElement("level1");
                    w.WriteAttributeString("b", "x", "fo", "y");
                    w.WriteAttributeString("xmlns", "b", null, "fo");
                    w.WriteStartElement("level2");
                    w.WriteAttributeString("c", "x", "fo", "y");
                    w.WriteAttributeString("xmlns", "c", null, "fo");
                    w.WriteEndElement();
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a:x=\"y\" xmlns:a=\"fo\"><level1 b:x=\"y\" xmlns:b=\"fo\"><level2 c:x=\"y\" xmlns:c=\"fo\" /></level1></Root>"));
            }

            // WriteAttribute for x:a and xmlns:a diff namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_20(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", "bar", "b");
                    w.WriteAttributeString("xmlns", "a", null, "foo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root x:a=\"b\" xmlns:a=\"foo\" xmlns:x=\"bar\" />"));
            }

            // WriteAttribute for x:a and xmlns:a same namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_21(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("x", "a", "foo", "b");
                    w.WriteAttributeString("xmlns", "a", null, "foo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root x:a=\"b\" xmlns:a=\"foo\" xmlns:x=\"foo\" />"));
            }

            // WriteAttribute with colliding NS and prefix for 2 attributes
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_22(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "x", null, "foo");
                    w.WriteAttributeString("x", "a", "foo", "b");
                    w.WriteAttributeString("x", "c", "foo", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:x=\"foo\" x:a=\"b\" x:c=\"b\" />"));
            }

            // WriteAttribute with DQ in namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_23(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("a", "\"", "b");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<Root ~a p1 a~:a=\"b\" xmlns:~a p1 A~=\"&quot;\" />"));
            }

            // Attach prefix with empty namespace
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_24(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", "foo", "bar", "");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Explicitly write namespace attribute that maps XML NS 'http://www.w3.org/XML/1998/namaespace' to another prefix
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_25(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("xmlns", "foo", "", "http://www.w3.org/XML/1998/namaespace");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root xmlns:foo=\"http://www.w3.org/XML/1998/namaespace\" />"));
            }

            // Map XML NS 'http://www.w3.org/XML/1998/namaespace' to another prefix
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_26(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("foo", "bar", "http://www.w3.org/XML/1998/namaespace", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root foo:bar=\"test\" xmlns:foo=\"http://www.w3.org/XML/1998/namaespace\" />"));
            }

            // Pass empty namespace to WriteAttributeString(prefix, name, ns, value)
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_27(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "urn:pre");
                    w.WriteAttributeString("pre", "attr", "", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root attr=\"test\" xmlns:pre=\"urn:pre\" />"));
            }

            // Write attribute with prefix = xmlns
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_28(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteAttributeString("xmlns", "xmlns", null, "test");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Write attribute in reserved XML namespace, should error
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_29(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("foo");
                        w.WriteAttributeString("aaa", "bbb", "http://www.w3.org/XML/1998/namespace", "ccc");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                Assert.Fail();
            }

            // Write attribute in reserved XMLNS namespace, should error
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_30(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("foo");
                        w.WriteStartAttribute("aaa", "bbb", "http://www.w3.org/XML/1998/namespace");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                Assert.Fail();
            }

            // WriteAttributeString with no namespace under element with empty prefix
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_31(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("d", "Data", "http://example.org/data");
                    w.WriteStartElement("g", "GoodStuff", "http://example.org/data/good");
                    w.WriteAttributeString("hello", "world");
                    w.WriteEndElement();
                    w.WriteStartElement("BadStuff", "http://example.org/data/bad");
                    w.WriteAttributeString("hello", "world");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<d:Data xmlns:d=\"http://example.org/data\">" +
                                    "<g:GoodStuff hello=\"world\" xmlns:g=\"http://example.org/data/good\" />" +
                                    "<BadStuff hello=\"world\" xmlns=\"http://example.org/data/bad\" />" +
                                    "</d:Data>"));
            }

            // Pass null prefix to WriteAttributeString()
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_32(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString(null, "attr", "ns", "value");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<Root ~a p1 a~:attr=\"value\" xmlns:~a p1 A~=\"ns\" />"));
            }

            // Pass String.Empty prefix to WriteAttributeString()
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_33(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString(string.Empty, "attr", "ns", "value");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareString("<Root ~a p1 a~:attr=\"value\" xmlns:~a p1 A~=\"ns\" />"));
            }

            // Pass null ns to WriteAttributeString()
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_34(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("pre", "attr", null, "value");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root attr='value' />"));
            }

            // Pass String.Empty ns to WriteAttributeString()
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_35(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteAttributeString("pre", "attr", string.Empty, "value");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root attr='value' />"));
            }

            // Pass null prefix to WriteAttributeString() when namespace is in scope
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_36(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteAttributeString(null, "child", "ns", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root pre:child='test' xmlns:pre='ns' />"));
            }

            // Pass String.Empty prefix to WriteAttributeString() when namespace is in scope
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_37(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteAttributeString(string.Empty, "child", "ns", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root pre:child='test' xmlns:pre='ns' />"));
            }

            // Pass null ns to WriteAttributeString() when prefix is in scope
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_38(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteAttributeString("pre", "child", null, "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root pre:child='test' xmlns:pre='ns' />"));
            }

            // Pass String.Empty ns to WriteAttributeString() when prefix is in scope
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_39(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "Root", "ns");
                    w.WriteAttributeString("pre", "child", string.Empty, "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:Root child='test' xmlns:pre='ns' />"));
            }

            // Mapping empty ns uri to a prefix should error
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_40(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteAttributeString("xmlns", null, null, "test");
                        w.WriteEndElement();
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        return;
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        return;
                    }
                }
                Assert.Fail();
            }

            // WriteStartAttribute with prefix = null, localName = xmlns - case 2
            [Theory]
            [XmlWriterInlineData]
            public void attrNamespace_42(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("pre", "foo", "ns1");
                    w.WriteAttributeString(null, "xmlns", "http://www.w3.org/2000/xmlns/", "ns");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<pre:foo xmlns='ns' xmlns:pre='ns1' />"));
            }
        }

        //[TestCase(Name = "WriteCData")]
        public partial class TCCData
        {
            // WriteCData with null
            [Theory]
            [XmlWriterInlineData]
            public void CData_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData(null);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[]]></Root>"));
            }

            // WriteCData with String.Empty
            [Theory]
            [XmlWriterInlineData]
            public void CData_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData(string.Empty);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[]]></Root>"));
            }

            // WriteCData Sanity test
            [Theory]
            [XmlWriterInlineData]
            public void CData_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("This text is in a CDATA section");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[This text is in a CDATA section]]></Root>"));
            }

            // WriteCData with valid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void CData_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("\uD812\uDD12");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[\uD812\uDD12]]></Root>"));
            }

            // WriteCData with ]]>
            [Theory]
            [XmlWriterInlineData]
            public void CData_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("test ]]> test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[test ]]]]><![CDATA[> test]]></Root>"));
            }

            // WriteCData with & < > chars, they should not be escaped
            [Theory]
            [XmlWriterInlineData]
            public void CData_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("<greeting>Hello World! & Hello XML</greeting>");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[<greeting>Hello World! & Hello XML</greeting>]]></Root>"));
            }

            // WriteCData with <![CDATA[
            [Theory]
            [XmlWriterInlineData]
            public void CData_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("<![CDATA[");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[<![CDATA[]]></Root>"));
            }
            // CData state machine
            [Theory]
            [XmlWriterInlineData]
            public void CData_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("]x]>]]x> x]x]x> x]]x]]x>");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[]x]>]]x> x]x]x> x]]x]]x>]]></Root>"));
            }

            // WriteCData with invalid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void CData_9(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteCData("\uD812");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WriteCData after root element
            [Theory]
            [XmlWriterInlineData]
            public void CData_10(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteEndElement();
                        w.WriteCData("foo");
                    }
                    catch (InvalidOperationException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Call WriteCData twice - that should write two CData blocks
            [Theory]
            [XmlWriterInlineData]
            public void CData_11(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCData("foo");
                    w.WriteCData("bar");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><![CDATA[foo]]><![CDATA[bar]]></Root>"));
            }

            // WriteCData with empty string at the buffer boundary
            [Theory]
            [XmlWriterInlineData]
            public void CData_12(XmlWriterUtils utils)
            {
                // WriteCData with empty string when the write buffer looks like
                // <r>aaaaaaa....   (currently length is 2048 * 3 - len("<![CDATA[")
                int buflen = 2048 * 3;
                string xml1 = "<r>";
                string xml3 = "<![CDATA[";
                int padlen = buflen - xml1.Length - xml3.Length;
                string xml2 = new string('a', padlen);
                string xml4 = "]]></r>";
                string expXml = string.Format("{0}{1}{2}{3}", xml1, xml2, xml3, xml4);
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("r");
                    w.WriteRaw(xml2);
                    w.WriteCData("");
                    w.WriteEndElement();
                }

                Assert.True(utils.CompareReader(expXml));
            }

            [Theory]
            [XmlWriterInlineData(0x0d, NewLineHandling.Replace, "<r><![CDATA[\r\n]]></r>" )]
            [XmlWriterInlineData(0x0d, NewLineHandling.None, "<r><![CDATA[\r]]></r>" )]
            [XmlWriterInlineData(0x0d, NewLineHandling.Entitize, "<r><![CDATA[\r]]></r>" )]
            [XmlWriterInlineData(0x0a, NewLineHandling.Replace, "<r><![CDATA[\r\n]]></r>" )]
            [XmlWriterInlineData(0x0a, NewLineHandling.None, "<r><![CDATA[\n]]></r>" )]
            [XmlWriterInlineData(0x0a, NewLineHandling.Entitize, "<r><![CDATA[\n]]></r>" )]
            public void CData_13(XmlWriterUtils utils, char ch, NewLineHandling nlh, string expXml)
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.NewLineHandling = nlh;
                xws.NewLineChars = "\r\n";

                using (XmlWriter w = utils.CreateWriter(xws))
                {
                    w.WriteStartElement("r");
                    w.WriteCData(new string(ch, 1));
                    w.WriteEndElement();
                }
                Assert.Equal(expXml, utils.GetString());
            }
        }

        //[TestCase(Name = "WriteComment")]
        public partial class TCComment
        {
            // Sanity test for WriteComment
            [Theory]
            [XmlWriterInlineData]
            public void comment_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteComment("This text is a comment");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><!--This text is a comment--></Root>"));
            }

            // Comment value = String.Empty
            [Theory]
            [XmlWriterInlineData]
            public void comment_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteComment(string.Empty);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><!----></Root>"));
            }

            // Comment value = null
            [Theory]
            [XmlWriterInlineData]
            public void comment_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteComment(null);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><!----></Root>"));
            }

            // WriteComment with valid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void comment_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteComment("\uD812\uDD12");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><!--\uD812\uDD12--></Root>"));
            }

            // WriteComment with invalid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void comment_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteComment("\uD812");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw error");
                Assert.Fail();
            }

            // WriteComment with -- in value
            [Theory]
            [XmlWriterInlineData]
            public void comment_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteComment("test --");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><!--test - - --></Root>"));
            }
        }

        //[TestCase(Name = "WriteEntityRef")]
        public partial class TCEntityRef
        {
            [Theory]
            [XmlWriterInlineData("null")]
            [XmlWriterInlineData("String.Empty")]
            [XmlWriterInlineData("test<test")]
            [XmlWriterInlineData("test>test")]
            [XmlWriterInlineData("test&test")]
            [XmlWriterInlineData("&test;")]
            [XmlWriterInlineData("test'test")]
            [XmlWriterInlineData("test\"test")]
            [XmlWriterInlineData("\xD")]
            [XmlWriterInlineData("\xD")]
            [XmlWriterInlineData("\xD\xA")]
            public void entityRef_1(XmlWriterUtils utils, string param)
            {
                string temp = null;
                switch (param)
                {
                    case "null":
                        temp = null;
                        break;
                    case "String.Empty":
                        temp = string.Empty;
                        break;
                    default:
                        temp = param;
                        break;
                }
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteEntityRef(temp);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                    catch (NullReferenceException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw error");
                Assert.Fail();
            }

            // WriteEntityRef with entity defined in doctype
            [Theory]
            [XmlWriterInlineData]
            public void entityRef_2(XmlWriterUtils utils)
            {
                string exp = utils.IsIndent() ?
                    "<!DOCTYPE Root [<!ENTITY e \"test\">]>" + Environment.NewLine + "<Root>&e;</Root>" :
                    "<!DOCTYPE Root [<!ENTITY e \"test\">]><Root>&e;</Root>";

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteDocType("Root", null, null, "<!ENTITY e \"test\">");
                    w.WriteStartElement("Root");
                    w.WriteEntityRef("e");
                    w.WriteEndElement();
                }

                Assert.Equal(exp, utils.GetString());
            }

            // WriteEntityRef in value for xml:lang attribute
            [Theory]
            [XmlWriterInlineData]
            public void entityRef_3(XmlWriterUtils utils)
            {
                string exp = utils.IsIndent() ?
                    "<!DOCTYPE root [<!ENTITY e \"en-us\">]>" + Environment.NewLine + "<root xml:lang=\"&e;&lt;\" />" :
                    "<!DOCTYPE root [<!ENTITY e \"en-us\">]><root xml:lang=\"&e;&lt;\" />";

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteDocType("root", null, null, "<!ENTITY e \"en-us\">");
                    w.WriteStartElement("root");
                    w.WriteStartAttribute("xml", "lang", null);
                    w.WriteEntityRef("e");
                    w.WriteString("<");
                    w.WriteEndAttribute();
                    w.WriteEndElement();
                }

                Assert.Equal(exp, utils.GetString());
            }

            // XmlWriter: Entity Refs are entitized twice in xml:lang attributes
            [Theory]
            [XmlWriterInlineData]
            public void var_14(XmlWriterUtils utils)
            {
                string exp = utils.IsIndent() ?
                    "<!DOCTYPE root [<!ENTITY e \"en-us\">]>" + Environment.NewLine + "<root xml:lang=\"&e;\" />" :
                    "<!DOCTYPE root [<!ENTITY e \"en-us\">]><root xml:lang=\"&e;\" />";

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteDocType("root", null, null, "<!ENTITY e \"en-us\">");
                    w.WriteStartElement("root");
                    w.WriteStartAttribute("xml", "lang", null);
                    w.WriteEntityRef("e");
                    w.WriteEndAttribute();
                    w.WriteEndElement();
                }

                Assert.Equal(exp, utils.GetString());
            }
        }

        //[TestCase(Name = "WriteCharEntity")]
        public partial class TCCharEntity
        {
            // WriteCharEntity with valid Unicode character
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("a");
                    w.WriteCharEntity('\uD23E');
                    w.WriteEndAttribute();
                    w.WriteCharEntity('\uD7FF');
                    w.WriteCharEntity('\uE000');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"&#xD23E;\">&#xD7FF;&#xE000;</Root>"));
            }

            // Call WriteCharEntity after WriteStartElement/WriteEndElement
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteCharEntity('\uD001');
                    w.WriteStartElement("elem");
                    w.WriteCharEntity('\uF345');
                    w.WriteEndElement();
                    w.WriteCharEntity('\u0048');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&#xD001;<elem>&#xF345;</elem>&#x48;</Root>"));
            }

            // Call WriteCharEntity after WriteStartAttribute/WriteEndAttribute
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("a");
                    w.WriteCharEntity('\u1289');
                    w.WriteEndAttribute();
                    w.WriteCharEntity('\u2584');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"&#x1289;\">&#x2584;</Root>"));
            }

            // Character from low surrogate range
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteCharEntity('\uDD12');
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Character from high surrogate range
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteCharEntity('\uD812');
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Sanity test, pass 'a'
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteCharEntity('c');
                    w.WriteEndElement();
                }
                string strExp = "<root>&#x63;</root>";
                Assert.True(utils.CompareReader(strExp));
            }

            // WriteCharEntity for special attributes
            [Theory]
            [XmlWriterInlineData]
            public void charEntity_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteStartAttribute("xml", "lang", null);
                    w.WriteCharEntity('A');
                    w.WriteString("\n");
                    w.WriteEndAttribute();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root xml:lang=\"A&#xA;\" />"));
            }

            // XmlWriter generates invalid XML
            [Theory]
            [XmlWriterInlineData]
            public void bug35637(XmlWriterUtils utils)
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                };

                using (XmlWriter xw = utils.CreateWriter())
                {
                    xw.WriteStartElement("root");
                    for (int i = 0; i < 150; i++)
                    {
                        xw.WriteElementString("e", "\u00e6\u00f8\u00e5\u00e9\u00ed\u00e8\u00f9\u00f6\u00f1\u00ea\u00fb\u00ee\u00c2\u00c5\u00d8\u00f5\u00cf");
                    }
                    xw.WriteElementString("end", "end");
                    xw.WriteEndElement();
                }

                using (XmlReader reader = utils.GetReader())
                {
                    reader.ReadToDescendant("end"); // should not throw here
                }

                return;
            }
        }

        //[TestCase(Name = "WriteSurrogateCharEntity")]
        public partial class TCSurrogateCharEntity
        {
            // SurrogateCharEntity after WriteStartElement/WriteEndElement
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteSurrogateCharEntity('\uDF41', '\uD920');
                    w.WriteStartElement("Elem");
                    w.WriteSurrogateCharEntity('\uDE44', '\uDAFF');
                    w.WriteEndElement();
                    w.WriteSurrogateCharEntity('\uDC22', '\uD820');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&#x58341;<Elem>&#xCFE44;</Elem>&#x18022;</Root>"));
            }

            // SurrogateCharEntity after WriteStartAttribute/WriteEndAttribute
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("a");
                    w.WriteSurrogateCharEntity('\uDF41', '\uD920');
                    w.WriteEndAttribute();
                    w.WriteSurrogateCharEntity('\uDE44', '\uDAFF');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"&#x58341;\">&#xCFE44;</Root>"));
            }

            // Test with limits of surrogate range
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("a");
                    w.WriteSurrogateCharEntity('\uDC00', '\uD800');
                    w.WriteEndAttribute();
                    w.WriteSurrogateCharEntity('\uDFFF', '\uD800');
                    w.WriteSurrogateCharEntity('\uDC00', '\uDBFF');
                    w.WriteSurrogateCharEntity('\uDFFF', '\uDBFF');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"&#x10000;\">&#x103FF;&#x10FC00;&#x10FFFF;</Root>"));
            }

            // Middle surrogate character
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteSurrogateCharEntity('\uDD12', '\uDA34');
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&#x9D112;</Root>"));
            }

            // Invalid high surrogate character
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteSurrogateCharEntity('\uDD12', '\uDD01');
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Invalid low surrogate character
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteSurrogateCharEntity('\u1025', '\uD900');
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Swap high-low surrogate characters
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteSurrogateCharEntity('\uD9A2', '\uDE34');
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WriteSurrogateCharEntity for special attributes
            [Theory]
            [XmlWriterInlineData]
            public void surrogateEntity_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteStartAttribute("xml", "lang", null);
                    w.WriteSurrogateCharEntity('\uDC00', '\uDBFF');
                    w.WriteEndAttribute();
                    w.WriteEndElement();
                }
                string strExp = "<root xml:lang=\"&#x10FC00;\" />";
                Assert.True(utils.CompareReader(strExp));
            }
        }

        //[TestCase(Name = "WriteProcessingInstruction")]
        public partial class TCPI
        {
            // Sanity test for WritePI
            [Theory]
            [XmlWriterInlineData]
            public void pi_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("test", "This text is a PI");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?test This text is a PI?></Root>"));
            }

            // PI text value = null
            [Theory]
            [XmlWriterInlineData]
            public void pi_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("test", null);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?test?></Root>"));
            }

            // PI text value = String.Empty
            [Theory]
            [XmlWriterInlineData]
            public void pi_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("test", string.Empty);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?test?></Root>"));
            }

            // PI name = null should error
            [Theory]
            [XmlWriterInlineData]
            public void pi_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteProcessingInstruction(null, "test");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                    catch (NullReferenceException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Element, "WriteState should be Element ");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // PI name = String.Empty should error
            [Theory]
            [XmlWriterInlineData]
            public void pi_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteProcessingInstruction(string.Empty, "test");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WritePI with xmlns as the name value
            [Theory]
            [XmlWriterInlineData]
            public void pi_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("xmlns", "text");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?xmlns text?></Root>"));
            }

            // WritePI with XmL as the name value
            [Theory]
            [XmlWriterInlineData]
            public void pi_7(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteProcessingInstruction("XmL", "text");
                        w.WriteEndElement();
                        w.Dispose();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WritePI before XmlDecl
            [Theory]
            [XmlWriterInlineData]
            public void pi_8(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteProcessingInstruction("pi", "text");
                        w.WriteStartDocument(true);
                    }
                    catch (InvalidOperationException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WritePI (after StartDocument) with name = 'xml' text = 'version = 1.0' should error
            [Theory]
            [XmlWriterInlineData]
            public void pi_9(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartDocument();
                        w.WriteProcessingInstruction("xml", "version = \"1.0\"");
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WritePI (before StartDocument) with name = 'xml' text = 'version = 1.0' should error
            [Theory]
            [XmlWriterInlineData]
            public void pi_10(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteProcessingInstruction("xml", "version = \"1.0\"");
                        w.WriteStartDocument();
                    }
                    catch (InvalidOperationException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Include PI end tag ?> as part of the text value
            [Theory]
            [XmlWriterInlineData]
            public void pi_11(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("badpi", "text ?>");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?badpi text ? >?></Root>"));
            }

            // WriteProcessingInstruction with valid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void pi_12(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteProcessingInstruction("pi", "\uD812\uDD12");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root><?pi \uD812\uDD12?></Root>"));
            }

            // WritePI with invalid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void pi_13(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteProcessingInstruction("pi", "\uD812");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }
        }

        //[TestCase(Name = "WriteNmToken")]
        public partial class TCWriteNmToken
        {
            [Theory]
            [XmlWriterInlineData("null")]
            [XmlWriterInlineData("String.Empty")]
            public void writeNmToken_1(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        string temp;
                        if (param == "null")
                            temp = null;
                        else
                            temp = string.Empty;
                        w.WriteNmToken(temp);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);//by design 396962
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Sanity test, Name = foo
            [Theory]
            [XmlWriterInlineData]
            public void writeNmToken_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteNmToken("foo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root>foo</root>"));
            }

            // Name contains letters, digits, . _ - : chars
            [Theory]
            [XmlWriterInlineData]
            public void writeNmToken_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteNmToken("_foo:1234.bar-");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root>_foo:1234.bar-</root>"));
            }

            [Theory]
            [XmlWriterInlineData("test test")]
            [XmlWriterInlineData("test?")]
            [XmlWriterInlineData("test'")]
            [XmlWriterInlineData("\"test")]
            public void writeNmToken_4(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteNmToken(param);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);
                        return;
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }
        }

        //[TestCase(Name = "WriteName")]
        public partial class TCWriteName
        {
            [Theory]
            [XmlWriterInlineData("null")]
            [XmlWriterInlineData("String.Empty")]
            public void writeName_1(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        string temp;
                        if (param == "null")
                            temp = null;
                        else
                            temp = string.Empty;
                        w.WriteName(temp);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Sanity test, Name = foo
            [Theory]
            [XmlWriterInlineData]
            public void writeName_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteName("foo");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root>foo</root>"));
            }

            // Sanity test, Name = foo:bar
            [Theory]
            [XmlWriterInlineData]
            public void writeName_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteName("foo:bar");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root>foo:bar</root>"));
            }

            [Theory]
            [XmlWriterInlineData(":bar")]
            [XmlWriterInlineData("foo bar")]
            public void writeName_4(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteName(param);
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);
                        return;
                    }
                    catch (XmlException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        utils.CheckElementState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }
        }

        //[TestCase(Name = "WriteQualifiedName")]
        public partial class TCWriteQName
        {
            [Theory]
            [XmlWriterInlineData("null")]
            [XmlWriterInlineData("String.Empty")]
            public void writeQName_1(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteAttributeString("xmlns", "foo", null, "test");
                        string temp;
                        if (param == "null")
                            temp = null;
                        else
                            temp = string.Empty;
                        w.WriteQualifiedName(temp, "test");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                    catch (NullReferenceException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.True(utils.WriterType == WriterType.CustomWriter);
            }

            // WriteQName with correct NS
            [Theory]
            [XmlWriterInlineData]
            public void writeQName_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("root");
                    w.WriteAttributeString("xmlns", "foo", null, "test");
                    w.WriteQualifiedName("bar", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root xmlns:foo=\"test\">foo:bar</root>"));
            }

            // WriteQName when NS is auto-generated
            [Theory]
            [XmlWriterInlineData]
            public void writeQName_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("foo", "root", "test");
                    w.WriteQualifiedName("bar", "test");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<foo:root xmlns:foo=\"test\">foo:bar</foo:root>"));
            }

            // QName = foo:bar when foo is not in scope
            [Theory]
            [XmlWriterInlineData]
            public void writeQName_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteQualifiedName("bar", "foo");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        if (utils.WriterType == WriterType.CustomWriter)
                        {
                            CError.Compare(w.WriteState, WriteState.Element, "WriteState should be Element");
                        }
                        else
                        {
                            CError.Compare(w.WriteState, WriteState.Error, "WriteState should be Error");
                        }
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            [Theory]
            [XmlWriterInlineData(":bar")]
            [XmlWriterInlineData("foo bar")]
            public void writeQName_5(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("root");
                        w.WriteAttributeString("xmlns", "foo", null, "test");
                        w.WriteQualifiedName(param, "test");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore(e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.True(utils.WriterType == WriterType.CustomWriter);
            }
        }

        //[TestCase(Name = "WriteChars")]
        public partial class TCWriteChars : TCWriteBuffer
        {
            // WriteChars with valid buffer, number, count
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string s = "test the buffer";
                    char[] buf = s.ToCharArray();
                    w.WriteStartElement("Root");
                    w.WriteChars(buf, 0, 4);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>test</Root>"));
            }

            // WriteChars with & < >
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string s = "&<>theend";
                    char[] buf = s.ToCharArray();

                    w.WriteStartElement("Root");
                    w.WriteChars(buf, 0, 5);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&amp;&lt;&gt;th</Root>"));
            }

            // WriteChars following WriteStartAttribute
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string s = "valid";
                    char[] buf = s.ToCharArray();

                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("a");
                    w.WriteChars(buf, 0, 5);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a=\"valid\" />"));
            }

            // WriteChars with entity ref included
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string s = "this is an entity &foo;";
                    char[] buf = s.ToCharArray();

                    w.WriteStartElement("Root");
                    w.WriteChars(buf, 0, buf.Length);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>this is an entity &amp;foo;</Root>"));
            }

            // WriteChars with buffer = null
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteChars(null, 0, 0);
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, WriteState.Error, WriteState.Element, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WriteChars with count > buffer size
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_6(XmlWriterUtils utils)
            {
                VerifyInvalidWrite(utils, "WriteChars", 5, 0, 6, typeof(ArgumentOutOfRangeException));
            }

            // WriteChars with count < 0
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_7(XmlWriterUtils utils)
            {
                VerifyInvalidWrite(utils, "WriteChars", 5, 2, -1, typeof(ArgumentOutOfRangeException));
            }

            // WriteChars with index > buffer size
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_8(XmlWriterUtils utils)
            {
                VerifyInvalidWrite(utils, "WriteChars", 5, 6, 1, typeof(ArgumentOutOfRangeException));
            }

            // WriteChars with index < 0
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_9(XmlWriterUtils utils)
            {
                VerifyInvalidWrite(utils, "WriteChars", 5, -1, 1, typeof(ArgumentOutOfRangeException));
            }

            // WriteChars with index + count exceeds buffer
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_10(XmlWriterUtils utils)
            {
                VerifyInvalidWrite(utils, "WriteChars", 5, 2, 5, typeof(ArgumentOutOfRangeException));
            }

            // WriteChars for xml:lang attribute, index = count = 0
            [Theory]
            [XmlWriterInlineData]
            public void writeChars_11(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string s = "en-us;";
                    char[] buf = s.ToCharArray();

                    w.WriteStartElement("root");
                    w.WriteStartAttribute("xml", "lang", null);
                    w.WriteChars(buf, 0, 0);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<root xml:lang=\"\" />"));
            }
        }

        //[TestCase(Name = "WriteString")]
        public partial class TCWriteString
        {
            // WriteString(null)
            [Theory]
            [XmlWriterInlineData]
            public void writeString_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString(null);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root />"));
            }

            // WriteString(String.Empty)
            [Theory]
            [XmlWriterInlineData]
            public void writeString_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString(string.Empty);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root></Root>"));
            }

            // WriteString with valid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void writeString_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString("\uD812\uDD12");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>\uD812\uDD12</Root>"));
            }

            // WriteString with invalid surrogate pair
            [Theory]
            [XmlWriterInlineData]
            public void writeString_4(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteString("\uD812");
                        w.WriteEndElement();
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // WriteString with entity reference
            [Theory]
            [XmlWriterInlineData]
            public void writeString_5(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString("&test;");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&amp;test;</Root>"));
            }

            // WriteString with single/double quote, &, <, >
            [Theory]
            [XmlWriterInlineData]
            public void writeString_6(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString("' & < > \"");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>&apos; &amp; &lt; &gt; \"</Root>"));
            }

            // WriteString for value greater than x1F
            [Theory]
            [XmlWriterInlineData]
            public void writeString_9(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString(XmlConvert.ToString('\x21'));
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>!</Root>"));
            }

            // WriteString with CR, LF, CR LF inside element
            [Theory]
            [XmlWriterInlineData]
            public void writeString_10(XmlWriterUtils utils)
            {
                // By default NormalizeNewLines = false and NewLineChars = \r\n
                // So \r, \n or \r\n gets replaces by \r\n in element content
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartElement("ws1");
                    w.WriteString("\r");
                    w.WriteEndElement();
                    w.WriteStartElement("ws2");
                    w.WriteString("\n");
                    w.WriteEndElement();
                    w.WriteStartElement("ws3");
                    w.WriteString("\r\n");
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareBaseline("writeStringWhiespaceInElem.txt"));
            }

            // WriteString with CR, LF, CR LF inside attribute value
            [Theory]
            [XmlWriterInlineData]
            public void writeString_11(XmlWriterUtils utils)
            {
                // \r, \n and \r\n gets replaced by char entities &#xD; &#xA; and &#xD;&#xA; respectively

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteStartAttribute("attr1");
                    w.WriteString("\r");
                    w.WriteStartAttribute("attr2");
                    w.WriteString("\n");
                    w.WriteStartAttribute("attr3");
                    w.WriteString("\r\n");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareBaseline("writeStringWhiespaceInAttr.txt"));
            }

            // Call WriteString for LF inside attribute
            [Theory]
            [XmlWriterInlineData]
            public void writeString_12(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root", "");
                    w.WriteStartAttribute("a1", "");
                    w.WriteString("x\ny");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root a1=\"x&#xA;y\" />"));
            }

            // Surrogate characters in text nodes, range limits
            [Theory]
            [XmlWriterInlineData]
            public void writeString_13(XmlWriterUtils utils)
            {
                char[] invalidXML = { '\uD800', '\uDC00', '\uD800', '\uDFFF', '\uDBFF', '\uDC00', '\uDBFF', '\uDFFF' };
                string invXML = new string(invalidXML);

                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString(invXML);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root>\uD800\uDC00\uD800\uDFFF\uDBFF\uDC00\uDBFF\uDFFF</Root>"));
            }

            // High surrogate on last position
            [Theory]
            [XmlWriterInlineData]
            public void writeString_14(XmlWriterUtils utils)
            {
                char[] invalidXML = { 'a', 'b', '\uDA34' };
                string invXML = new string(invalidXML);

                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteString(invXML);
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Low surrogate on first position
            [Theory]
            [XmlWriterInlineData]
            public void writeString_15(XmlWriterUtils utils)
            {
                char[] invalidXML = { '\uDF20', 'b', 'c' };
                string invXML = new string(invalidXML);

                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteString(invXML);
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }

            // Swap low-high surrogates
            [Theory]
            [XmlWriterInlineData]
            public void writeString_16(XmlWriterUtils utils)
            {
                char[] invalidXML = { 'a', '\uDE40', '\uDA72', 'c' };
                string invXML = new string(invalidXML);

                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteString(invXML);
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        utils.CheckErrorState(w.WriteState);
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }
        }

        //[TestCase(Name = "WriteWhitespace")]
        public partial class TCWhiteSpace
        {
            // WriteWhitespace with values #x20 #x9 #xD #xA
            [Theory]
            [XmlWriterInlineData]
            public void whitespace_1(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString("text");
                    w.WriteWhitespace("\x20");
                    w.WriteString("text");
                    w.WriteWhitespace("\x9");
                    w.WriteString("text");
                    w.WriteWhitespace("\xD");
                    w.WriteString("text");
                    w.WriteWhitespace("\xA");
                    w.WriteString("text");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareBaseline("whitespace1.txt"));
            }

            // WriteWhitespace in the middle of text
            [Theory]
            [XmlWriterInlineData]
            public void whitespace_2(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartElement("Root");
                    w.WriteString("text");
                    w.WriteWhitespace("\xD");
                    w.WriteString("text");
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareBaseline("whitespace2.txt"));
            }

            // WriteWhitespace before and after root element
            [Theory]
            [XmlWriterInlineData]
            public void whitespace_3(XmlWriterUtils utils)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    w.WriteStartDocument();
                    w.WriteWhitespace("\x20");
                    w.WriteStartElement("Root");
                    w.WriteEndElement();
                    w.WriteWhitespace("\x20");
                    w.WriteEndDocument();
                }
                Assert.True(utils.CompareBaseline("whitespace3.txt"));
            }

            [Theory]
            [XmlWriterInlineData("null")]
            [XmlWriterInlineData("String.Empty")]
            public void whitespace_4(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    string temp;
                    if (param == "null")
                        temp = null;
                    else
                        temp = string.Empty;
                    w.WriteStartElement("Root");

                    w.WriteWhitespace(temp);
                    w.WriteEndElement();
                }
                Assert.True(utils.CompareReader("<Root></Root>"));
            }

            [Theory]
            [XmlWriterInlineData("a")]
            [XmlWriterInlineData("\xE")]
            [XmlWriterInlineData("\x0")]
            [XmlWriterInlineData("\x10")]
            [XmlWriterInlineData("\x1F")]
            public void whitespace_5(XmlWriterUtils utils, string param)
            {
                using (XmlWriter w = utils.CreateWriter())
                {
                    try
                    {
                        w.WriteStartElement("Root");
                        w.WriteWhitespace(param);
                    }
                    catch (ArgumentException e)
                    {
                        CError.WriteLineIgnore("Exception: " + e.ToString());
                        CError.Compare(w.WriteState, (utils.WriterType == WriterType.CharCheckingWriter) ? WriteState.Element : WriteState.Error, "WriteState should be Error");
                        return;
                    }
                }
                CError.WriteLine("Did not throw exception");
                Assert.Fail();
            }
        }
    }
}
