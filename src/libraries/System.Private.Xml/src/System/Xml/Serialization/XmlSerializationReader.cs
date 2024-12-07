// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization.CodeGenerations;
using System.Xml.Serialization.Types;

namespace System.Xml.Serialization
{
    ///<internalonly/>
    public abstract class XmlSerializationReader : XmlSerializationGeneratedCode
    {
        private XmlReader _r = null!;
        private XmlDocument? _d;
        private Hashtable? _callbacks;
        private Hashtable _types = null!;
        private Hashtable _typesReverse = null!;
        private XmlDeserializationEvents _events;
        private Hashtable? _targets;
        private Hashtable? _referencedTargets;
        private ArrayList? _targetsWithoutIds;
        private ArrayList? _fixups;
        private ArrayList? _collectionFixups;
        private bool _soap12;
        private bool _isReturnValue;
        private bool _decodeName = true;

        private string _schemaNsID = null!;
        private string _schemaNs1999ID = null!;
        private string _schemaNs2000ID = null!;
        private string _schemaNonXsdTypesNsID = null!;
        private string _instanceNsID = null!;
        private string _instanceNs2000ID = null!;
        private string _instanceNs1999ID = null!;
        private string _soapNsID = null!;
        private string _soap12NsID = null!;
        private string _wsdlNsID = null!;
        private string _wsdlArrayTypeID = null!;
        private string _nullID = null!;
        private string _nilID = null!;
        private string _typeID = null!;
        private string _arrayTypeID = null!;
        private string _itemTypeID = null!;
        private string _arraySizeID = null!;
        private string _arrayID = null!;
        private string _urTypeID = null!;
        private string _stringID = null!;
        private string _intID = null!;
        private string _booleanID = null!;
        private string _shortID = null!;
        private string _longID = null!;
        private string _floatID = null!;
        private string _doubleID = null!;
        private string _decimalID = null!;
        private string _dateTimeID = null!;
        private string _qnameID = null!;
        private string _dateID = null!;
        private string _timeID = null!;
        private string _hexBinaryID = null!;
        private string _base64BinaryID = null!;
        private string _base64ID = null!;
        private string _unsignedByteID = null!;
        private string _byteID = null!;
        private string _unsignedShortID = null!;
        private string _unsignedIntID = null!;
        private string _unsignedLongID = null!;
        private string _oldDecimalID = null!;
        private string _oldTimeInstantID = null!;

        private string _anyURIID = null!;
        private string _durationID = null!;
        private string _ENTITYID = null!;
        private string _ENTITIESID = null!;
        private string _gDayID = null!;
        private string _gMonthID = null!;
        private string _gMonthDayID = null!;
        private string _gYearID = null!;
        private string _gYearMonthID = null!;
        private string _IDID = null!;
        private string _IDREFID = null!;
        private string _IDREFSID = null!;
        private string _integerID = null!;
        private string _languageID = null!;
        private string _nameID = null!;
        private string _NCNameID = null!;
        private string _NMTOKENID = null!;
        private string _NMTOKENSID = null!;
        private string _negativeIntegerID = null!;
        private string _nonPositiveIntegerID = null!;
        private string _nonNegativeIntegerID = null!;
        private string _normalizedStringID = null!;
        private string _NOTATIONID = null!;
        private string _positiveIntegerID = null!;
        private string _tokenID = null!;

        private string _charID = null!;
        private string _guidID = null!;
        private string _timeSpanID = null!;
        private string _dateTimeOffsetID = null!;

        protected abstract void InitIDs();

        // this method must be called before any generated deserialization methods are called
        internal void Init(XmlReader r, XmlDeserializationEvents events, string? encodingStyle)
        {
            _events = events;
            _r = r;
            _d = null;
            _soap12 = (encodingStyle == Soap12.Encoding);

            _schemaNsID = r.NameTable.Add(XmlSchema.Namespace);
            _schemaNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema");
            _schemaNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema");
            _schemaNonXsdTypesNsID = r.NameTable.Add(UrtTypes.Namespace);
            _instanceNsID = r.NameTable.Add(XmlSchema.InstanceNamespace);
            _instanceNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
            _instanceNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");
            _soapNsID = r.NameTable.Add(Soap.Encoding);
            _soap12NsID = r.NameTable.Add(Soap12.Encoding);
            r.NameTable.Add("schema");
            _wsdlNsID = r.NameTable.Add(Wsdl.Namespace);
            _wsdlArrayTypeID = r.NameTable.Add(Wsdl.ArrayType);
            _nullID = r.NameTable.Add("null");
            _nilID = r.NameTable.Add("nil");
            _typeID = r.NameTable.Add("type");
            _arrayTypeID = r.NameTable.Add("arrayType");
            _itemTypeID = r.NameTable.Add("itemType");
            _arraySizeID = r.NameTable.Add("arraySize");
            _arrayID = r.NameTable.Add("Array");
            _urTypeID = r.NameTable.Add(Soap.UrType);
            InitIDs();
        }

        protected bool DecodeName
        {
            get
            {
                return _decodeName;
            }
            set
            {
                _decodeName = value;
            }
        }

        protected XmlReader Reader
        {
            get
            {
                return _r;
            }
        }

        protected int ReaderCount
        {
            get { return 0; }
        }

        protected XmlDocument Document
        {
            get
            {
                if (_d == null)
                {
                    _d = new XmlDocument(_r.NameTable);
                    _d.SetBaseURI(_r.BaseURI);
                }
                return _d;
            }
        }

        ///<internalonly/>
        protected static Assembly? ResolveDynamicAssembly(string assemblyFullName)
        {
            return DynamicAssemblies.Get(assemblyFullName);
        }

        private void InitPrimitiveIDs()
        {
            if (_tokenID != null) return;
            _r.NameTable.Add(XmlSchema.Namespace);
            _r.NameTable.Add(UrtTypes.Namespace);

            _stringID = _r.NameTable.Add(DataTypeNames.String);
            _intID = _r.NameTable.Add(DataTypeNames.Int32);
            _booleanID = _r.NameTable.Add(DataTypeNames.Boolean);
            _shortID = _r.NameTable.Add(DataTypeNames.Int16);
            _longID = _r.NameTable.Add(DataTypeNames.Int64);
            _floatID = _r.NameTable.Add(DataTypeNames.Single);
            _doubleID = _r.NameTable.Add(DataTypeNames.Double);
            _decimalID = _r.NameTable.Add(DataTypeNames.Decimal);
            _dateTimeID = _r.NameTable.Add(DataTypeNames.DateTime);
            _qnameID = _r.NameTable.Add(DataTypeNames.XmlQualifiedName);
            _dateID = _r.NameTable.Add(DataTypeNames.Date);
            _timeID = _r.NameTable.Add(DataTypeNames.Time);
            _hexBinaryID = _r.NameTable.Add(DataTypeNames.ByteArrayHex);
            _base64BinaryID = _r.NameTable.Add(DataTypeNames.ByteArrayBase64);
            _unsignedByteID = _r.NameTable.Add(DataTypeNames.Byte);
            _byteID = _r.NameTable.Add(DataTypeNames.SByte);
            _unsignedShortID = _r.NameTable.Add(DataTypeNames.UInt16);
            _unsignedIntID = _r.NameTable.Add(DataTypeNames.UInt32);
            _unsignedLongID = _r.NameTable.Add(DataTypeNames.UInt64);
            _oldDecimalID = _r.NameTable.Add(DataTypeNames.Decimal);
            _oldTimeInstantID = _r.NameTable.Add(DataTypeNames.OldTimeInstant);
            _charID = _r.NameTable.Add(DataTypeNames.Char);
            _guidID = _r.NameTable.Add(DataTypeNames.Guid);
            _timeSpanID = _r.NameTable.Add(DataTypeNames.TimeSpan);
            _dateTimeOffsetID = _r.NameTable.Add(DataTypeNames.DateTimeOffset);
            _base64ID = _r.NameTable.Add(DataTypeNames.Base64);

            _anyURIID = _r.NameTable.Add(DataTypeNames.AnyUri);
            _durationID = _r.NameTable.Add(DataTypeNames.Duration);
            _ENTITYID = _r.NameTable.Add(DataTypeNames.Entity);
            _ENTITIESID = _r.NameTable.Add(DataTypeNames.Entities);
            _gDayID = _r.NameTable.Add(DataTypeNames.GDay);
            _gMonthID = _r.NameTable.Add(DataTypeNames.GMonth);
            _gMonthDayID = _r.NameTable.Add(DataTypeNames.GMonthDay);
            _gYearID = _r.NameTable.Add(DataTypeNames.GYear);
            _gYearMonthID = _r.NameTable.Add(DataTypeNames.GYearMonth);
            _IDID = _r.NameTable.Add(DataTypeNames.Id);
            _IDREFID = _r.NameTable.Add(DataTypeNames.IdRef);
            _IDREFSID = _r.NameTable.Add(DataTypeNames.IdRefs);
            _integerID = _r.NameTable.Add(DataTypeNames.Integer);
            _languageID = _r.NameTable.Add(DataTypeNames.Language);
            _nameID = _r.NameTable.Add(DataTypeNames.XmlName);
            _NCNameID = _r.NameTable.Add(DataTypeNames.NoncolonizedName);
            _NMTOKENID = _r.NameTable.Add(DataTypeNames.XmlNmToken);
            _NMTOKENSID = _r.NameTable.Add(DataTypeNames.XmlNmTokens);
            _negativeIntegerID = _r.NameTable.Add(DataTypeNames.NegativeInteger);
            _nonNegativeIntegerID = _r.NameTable.Add(DataTypeNames.NonNegativeInteger);
            _nonPositiveIntegerID = _r.NameTable.Add(DataTypeNames.NonPositiveInteger);
            _normalizedStringID = _r.NameTable.Add(DataTypeNames.NormalizedString);
            _NOTATIONID = _r.NameTable.Add(DataTypeNames.Notation);
            _positiveIntegerID = _r.NameTable.Add(DataTypeNames.PositiveInteger);
            _tokenID = _r.NameTable.Add(DataTypeNames.Token);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName? GetXsiType()
        {
            string? type = _r.GetAttribute(_typeID, _instanceNsID);
            if (type == null)
            {
                type = _r.GetAttribute(_typeID, _instanceNs2000ID);
                if (type == null)
                {
                    type = _r.GetAttribute(_typeID, _instanceNs1999ID);
                    if (type == null)
                        return null;
                }
            }
            return ToXmlQualifiedName(type, false);
        }

        // throwOnUnknown flag controls whether this method throws an exception or just returns
        // null if typeName.Namespace is unknown. the method still throws if typeName.Namespace
        // is recognized but typeName.Name isn't.
        private Type? GetPrimitiveType(XmlQualifiedName typeName, bool throwOnUnknown)
        {
            InitPrimitiveIDs();

            if (typeName.Namespace == _schemaNsID || typeName.Namespace == _soapNsID || typeName.Namespace == _soap12NsID)
            {
                if (typeName.Name == _stringID ||
                    typeName.Name == _anyURIID ||
                    typeName.Name == _durationID ||
                    typeName.Name == _ENTITYID ||
                    typeName.Name == _ENTITIESID ||
                    typeName.Name == _gDayID ||
                    typeName.Name == _gMonthID ||
                    typeName.Name == _gMonthDayID ||
                    typeName.Name == _gYearID ||
                    typeName.Name == _gYearMonthID ||
                    typeName.Name == _IDID ||
                    typeName.Name == _IDREFID ||
                    typeName.Name == _IDREFSID ||
                    typeName.Name == _integerID ||
                    typeName.Name == _languageID ||
                    typeName.Name == _nameID ||
                    typeName.Name == _NCNameID ||
                    typeName.Name == _NMTOKENID ||
                    typeName.Name == _NMTOKENSID ||
                    typeName.Name == _negativeIntegerID ||
                    typeName.Name == _nonPositiveIntegerID ||
                    typeName.Name == _nonNegativeIntegerID ||
                    typeName.Name == _normalizedStringID ||
                    typeName.Name == _NOTATIONID ||
                    typeName.Name == _positiveIntegerID ||
                    typeName.Name == _tokenID)
                    return typeof(string);
                else if (typeName.Name == _intID)
                    return typeof(int);
                else if (typeName.Name == _booleanID)
                    return typeof(bool);
                else if (typeName.Name == _shortID)
                    return typeof(short);
                else if (typeName.Name == _longID)
                    return typeof(long);
                else if (typeName.Name == _floatID)
                    return typeof(float);
                else if (typeName.Name == _doubleID)
                    return typeof(double);
                else if (typeName.Name == _decimalID)
                    return typeof(decimal);
                else if (typeName.Name == _dateTimeID)
                    return typeof(DateTime);
                else if (typeName.Name == _qnameID)
                    return typeof(XmlQualifiedName);
                else if (typeName.Name == _dateID)
                    return typeof(DateTime);
                else if (typeName.Name == _timeID)
                    return typeof(DateTime);
                else if (typeName.Name == _hexBinaryID)
                    return typeof(byte[]);
                else if (typeName.Name == _base64BinaryID)
                    return typeof(byte[]);
                else if (typeName.Name == _unsignedByteID)
                    return typeof(byte);
                else if (typeName.Name == _byteID)
                    return typeof(sbyte);
                else if (typeName.Name == _unsignedShortID)
                    return typeof(ushort);
                else if (typeName.Name == _unsignedIntID)
                    return typeof(uint);
                else if (typeName.Name == _unsignedLongID)
                    return typeof(ulong);
                else
                    throw CreateUnknownTypeException(typeName);
            }
            else if (typeName.Namespace == _schemaNs2000ID || typeName.Namespace == _schemaNs1999ID)
            {
                if (typeName.Name == _stringID ||
                    typeName.Name == _anyURIID ||
                    typeName.Name == _durationID ||
                    typeName.Name == _ENTITYID ||
                    typeName.Name == _ENTITIESID ||
                    typeName.Name == _gDayID ||
                    typeName.Name == _gMonthID ||
                    typeName.Name == _gMonthDayID ||
                    typeName.Name == _gYearID ||
                    typeName.Name == _gYearMonthID ||
                    typeName.Name == _IDID ||
                    typeName.Name == _IDREFID ||
                    typeName.Name == _IDREFSID ||
                    typeName.Name == _integerID ||
                    typeName.Name == _languageID ||
                    typeName.Name == _nameID ||
                    typeName.Name == _NCNameID ||
                    typeName.Name == _NMTOKENID ||
                    typeName.Name == _NMTOKENSID ||
                    typeName.Name == _negativeIntegerID ||
                    typeName.Name == _nonPositiveIntegerID ||
                    typeName.Name == _nonNegativeIntegerID ||
                    typeName.Name == _normalizedStringID ||
                    typeName.Name == _NOTATIONID ||
                    typeName.Name == _positiveIntegerID ||
                    typeName.Name == _tokenID)
                    return typeof(string);
                else if (typeName.Name == _intID)
                    return typeof(int);
                else if (typeName.Name == _booleanID)
                    return typeof(bool);
                else if (typeName.Name == _shortID)
                    return typeof(short);
                else if (typeName.Name == _longID)
                    return typeof(long);
                else if (typeName.Name == _floatID)
                    return typeof(float);
                else if (typeName.Name == _doubleID)
                    return typeof(double);
                else if (typeName.Name == _oldDecimalID)
                    return typeof(decimal);
                else if (typeName.Name == _oldTimeInstantID)
                    return typeof(DateTime);
                else if (typeName.Name == _qnameID)
                    return typeof(XmlQualifiedName);
                else if (typeName.Name == _dateID)
                    return typeof(DateTime);
                else if (typeName.Name == _timeID)
                    return typeof(DateTime);
                else if (typeName.Name == _hexBinaryID)
                    return typeof(byte[]);
                else if (typeName.Name == _byteID)
                    return typeof(sbyte);
                else if (typeName.Name == _unsignedShortID)
                    return typeof(ushort);
                else if (typeName.Name == _unsignedIntID)
                    return typeof(uint);
                else if (typeName.Name == _unsignedLongID)
                    return typeof(ulong);
                else
                    throw CreateUnknownTypeException(typeName);
            }
            else if (typeName.Namespace == _schemaNonXsdTypesNsID)
            {
                if (typeName.Name == _charID)
                    return typeof(char);
                else if (typeName.Name == _guidID)
                    return typeof(Guid);
                else
                    throw CreateUnknownTypeException(typeName);
            }
            else if (throwOnUnknown)
                throw CreateUnknownTypeException(typeName);
            else
                return null;
        }

        private bool IsPrimitiveNamespace(string ns)
        {
            return ns == _schemaNsID ||
                   ns == _schemaNonXsdTypesNsID ||
                   ns == _soapNsID ||
                   ns == _soap12NsID ||
                   ns == _schemaNs2000ID ||
                   ns == _schemaNs1999ID;
        }

        private string ReadStringValue()
        {
            if (_r.IsEmptyElement)
            {
                _r.Skip();
                return string.Empty;
            }
            _r.ReadStartElement();
            string retVal = _r.ReadString();
            ReadEndElement();
            return retVal;
        }

        private XmlQualifiedName ReadXmlQualifiedName()
        {
            string s;
            bool isEmpty = false;
            if (_r.IsEmptyElement)
            {
                s = string.Empty;
                isEmpty = true;
            }
            else
            {
                _r.ReadStartElement();
                s = _r.ReadString();
            }

            XmlQualifiedName retVal = ToXmlQualifiedName(s);
            if (isEmpty)
                _r.Skip();
            else
                ReadEndElement();
            return retVal;
        }

        private byte[] ReadByteArray(bool isBase64)
        {
            ArrayList list = new ArrayList();
            const int MAX_ALLOC_SIZE = 64 * 1024;
            int currentSize = 1024;
            byte[] buffer;
            int bytes = -1;
            int offset = 0;
            int total = 0;
            buffer = new byte[currentSize];
            list.Add(buffer);
            while (bytes != 0)
            {
                if (offset == buffer.Length)
                {
                    currentSize = Math.Min(currentSize * 2, MAX_ALLOC_SIZE);
                    buffer = new byte[currentSize];
                    offset = 0;
                    list.Add(buffer);
                }
                if (isBase64)
                {
                    bytes = _r.ReadElementContentAsBase64(buffer, offset, buffer.Length - offset);
                }
                else
                {
                    bytes = _r.ReadElementContentAsBinHex(buffer, offset, buffer.Length - offset);
                }
                offset += bytes;
                total += bytes;
            }

            byte[] result = new byte[total];
            offset = 0;
            foreach (byte[] block in list)
            {
                currentSize = Math.Min(block.Length, total);
                if (currentSize > 0)
                {
                    Buffer.BlockCopy(block, 0, result, offset, currentSize);
                    offset += currentSize;
                    total -= currentSize;
                }
            }
            list.Clear();
            return result;
        }

        protected object? ReadTypedPrimitive(XmlQualifiedName type)
        {
            return ReadTypedPrimitive(type, false);
        }

        private object? ReadTypedPrimitive(XmlQualifiedName type, bool elementCanBeType)
        {
            InitPrimitiveIDs();
            object? value;
            if (!IsPrimitiveNamespace(type.Namespace) || type.Name == _urTypeID)
            {
                return ReadXmlNodes(elementCanBeType);
            }

            if (type.Namespace == _schemaNsID || type.Namespace == _soapNsID || type.Namespace == _soap12NsID)
            {
                if (type.Name == _stringID ||
                    type.Name == _normalizedStringID)
                    value = ReadStringValue();
                else if (type.Name == _anyURIID ||
                    type.Name == _durationID ||
                    type.Name == _ENTITYID ||
                    type.Name == _ENTITIESID ||
                    type.Name == _gDayID ||
                    type.Name == _gMonthID ||
                    type.Name == _gMonthDayID ||
                    type.Name == _gYearID ||
                    type.Name == _gYearMonthID ||
                    type.Name == _IDID ||
                    type.Name == _IDREFID ||
                    type.Name == _IDREFSID ||
                    type.Name == _integerID ||
                    type.Name == _languageID ||
                    type.Name == _nameID ||
                    type.Name == _NCNameID ||
                    type.Name == _NMTOKENID ||
                    type.Name == _NMTOKENSID ||
                    type.Name == _negativeIntegerID ||
                    type.Name == _nonPositiveIntegerID ||
                    type.Name == _nonNegativeIntegerID ||
                    type.Name == _NOTATIONID ||
                    type.Name == _positiveIntegerID ||
                    type.Name == _tokenID)
                    value = CollapseWhitespace(ReadStringValue());
                else if (type.Name == _intID)
                    value = XmlConvert.ToInt32(ReadStringValue());
                else if (type.Name == _booleanID)
                    value = XmlConvert.ToBoolean(ReadStringValue());
                else if (type.Name == _shortID)
                    value = XmlConvert.ToInt16(ReadStringValue());
                else if (type.Name == _longID)
                    value = XmlConvert.ToInt64(ReadStringValue());
                else if (type.Name == _floatID)
                    value = XmlConvert.ToSingle(ReadStringValue());
                else if (type.Name == _doubleID)
                    value = XmlConvert.ToDouble(ReadStringValue());
                else if (type.Name == _decimalID)
                    value = XmlConvert.ToDecimal(ReadStringValue());
                else if (type.Name == _dateTimeID)
                    value = ToDateTime(ReadStringValue());
                else if (type.Name == _qnameID)
                    value = ReadXmlQualifiedName();
                else if (type.Name == _dateID)
                    value = ToDate(ReadStringValue());
                else if (type.Name == _timeID)
                    value = ToTime(ReadStringValue());
                else if (type.Name == _unsignedByteID)
                    value = XmlConvert.ToByte(ReadStringValue());
                else if (type.Name == _byteID)
                    value = XmlConvert.ToSByte(ReadStringValue());
                else if (type.Name == _unsignedShortID)
                    value = XmlConvert.ToUInt16(ReadStringValue());
                else if (type.Name == _unsignedIntID)
                    value = XmlConvert.ToUInt32(ReadStringValue());
                else if (type.Name == _unsignedLongID)
                    value = XmlConvert.ToUInt64(ReadStringValue());
                else if (type.Name == _hexBinaryID)
                    value = ToByteArrayHex(false);
                else if (type.Name == _base64BinaryID)
                    value = ToByteArrayBase64(false);
                else if (type.Name == _base64ID && (type.Namespace == _soapNsID || type.Namespace == _soap12NsID))
                    value = ToByteArrayBase64(false);
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else if (type.Namespace == _schemaNs2000ID || type.Namespace == _schemaNs1999ID)
            {
                if (type.Name == _stringID ||
                    type.Name == _normalizedStringID)
                    value = ReadStringValue();
                else if (type.Name == _anyURIID ||
                    type.Name == _durationID ||
                    type.Name == _ENTITYID ||
                    type.Name == _ENTITIESID ||
                    type.Name == _gDayID ||
                    type.Name == _gMonthID ||
                    type.Name == _gMonthDayID ||
                    type.Name == _gYearID ||
                    type.Name == _gYearMonthID ||
                    type.Name == _IDID ||
                    type.Name == _IDREFID ||
                    type.Name == _IDREFSID ||
                    type.Name == _integerID ||
                    type.Name == _languageID ||
                    type.Name == _nameID ||
                    type.Name == _NCNameID ||
                    type.Name == _NMTOKENID ||
                    type.Name == _NMTOKENSID ||
                    type.Name == _negativeIntegerID ||
                    type.Name == _nonPositiveIntegerID ||
                    type.Name == _nonNegativeIntegerID ||
                    type.Name == _NOTATIONID ||
                    type.Name == _positiveIntegerID ||
                    type.Name == _tokenID)
                    value = CollapseWhitespace(ReadStringValue());
                else if (type.Name == _intID)
                    value = XmlConvert.ToInt32(ReadStringValue());
                else if (type.Name == _booleanID)
                    value = XmlConvert.ToBoolean(ReadStringValue());
                else if (type.Name == _shortID)
                    value = XmlConvert.ToInt16(ReadStringValue());
                else if (type.Name == _longID)
                    value = XmlConvert.ToInt64(ReadStringValue());
                else if (type.Name == _floatID)
                    value = XmlConvert.ToSingle(ReadStringValue());
                else if (type.Name == _doubleID)
                    value = XmlConvert.ToDouble(ReadStringValue());
                else if (type.Name == _oldDecimalID)
                    value = XmlConvert.ToDecimal(ReadStringValue());
                else if (type.Name == _oldTimeInstantID)
                    value = ToDateTime(ReadStringValue());
                else if (type.Name == _qnameID)
                    value = ReadXmlQualifiedName();
                else if (type.Name == _dateID)
                    value = ToDate(ReadStringValue());
                else if (type.Name == _timeID)
                    value = ToTime(ReadStringValue());
                else if (type.Name == _unsignedByteID)
                    value = XmlConvert.ToByte(ReadStringValue());
                else if (type.Name == _byteID)
                    value = XmlConvert.ToSByte(ReadStringValue());
                else if (type.Name == _unsignedShortID)
                    value = XmlConvert.ToUInt16(ReadStringValue());
                else if (type.Name == _unsignedIntID)
                    value = XmlConvert.ToUInt32(ReadStringValue());
                else if (type.Name == _unsignedLongID)
                    value = XmlConvert.ToUInt64(ReadStringValue());
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else if (type.Namespace == _schemaNonXsdTypesNsID)
            {
                if (type.Name == _charID)
                    value = ToChar(ReadStringValue());
                else if (type.Name == _guidID)
                    value = new Guid(CollapseWhitespace(ReadStringValue()));
                else if (type.Name == _timeSpanID)
                    value = XmlConvert.ToTimeSpan(ReadStringValue());
                else if (type.Name == _dateTimeOffsetID)
                    value = XmlConvert.ToDateTimeOffset(ReadStringValue());
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else
                value = ReadXmlNodes(elementCanBeType);
            return value;
        }

        protected object? ReadTypedNull(XmlQualifiedName type)
        {
            InitPrimitiveIDs();
            object? value;
            if (!IsPrimitiveNamespace(type.Namespace) || type.Name == _urTypeID)
            {
                return null;
            }

            if (type.Namespace == _schemaNsID || type.Namespace == _soapNsID || type.Namespace == _soap12NsID)
            {
                if (type.Name == _stringID ||
                    type.Name == _anyURIID ||
                    type.Name == _durationID ||
                    type.Name == _ENTITYID ||
                    type.Name == _ENTITIESID ||
                    type.Name == _gDayID ||
                    type.Name == _gMonthID ||
                    type.Name == _gMonthDayID ||
                    type.Name == _gYearID ||
                    type.Name == _gYearMonthID ||
                    type.Name == _IDID ||
                    type.Name == _IDREFID ||
                    type.Name == _IDREFSID ||
                    type.Name == _integerID ||
                    type.Name == _languageID ||
                    type.Name == _nameID ||
                    type.Name == _NCNameID ||
                    type.Name == _NMTOKENID ||
                    type.Name == _NMTOKENSID ||
                    type.Name == _negativeIntegerID ||
                    type.Name == _nonPositiveIntegerID ||
                    type.Name == _nonNegativeIntegerID ||
                    type.Name == _normalizedStringID ||
                    type.Name == _NOTATIONID ||
                    type.Name == _positiveIntegerID ||
                    type.Name == _tokenID)
                    value = null;
                else if (type.Name == _intID)
                {
                    value = default(Nullable<int>);
                }
                else if (type.Name == _booleanID)
                    value = default(Nullable<bool>);
                else if (type.Name == _shortID)
                    value = default(Nullable<short>);
                else if (type.Name == _longID)
                    value = default(Nullable<long>);
                else if (type.Name == _floatID)
                    value = default(Nullable<float>);
                else if (type.Name == _doubleID)
                    value = default(Nullable<double>);
                else if (type.Name == _decimalID)
                    value = default(Nullable<decimal>);
                else if (type.Name == _dateTimeID)
                    value = default(Nullable<DateTime>);
                else if (type.Name == _qnameID)
                    value = null;
                else if (type.Name == _dateID)
                    value = default(Nullable<DateTime>);
                else if (type.Name == _timeID)
                    value = default(Nullable<DateTime>);
                else if (type.Name == _unsignedByteID)
                    value = default(Nullable<byte>);
                else if (type.Name == _byteID)
                    value = default(Nullable<sbyte>);
                else if (type.Name == _unsignedShortID)
                    value = default(Nullable<ushort>);
                else if (type.Name == _unsignedIntID)
                    value = default(Nullable<uint>);
                else if (type.Name == _unsignedLongID)
                    value = default(Nullable<ulong>);
                else if (type.Name == _hexBinaryID)
                    value = null;
                else if (type.Name == _base64BinaryID)
                    value = null;
                else if (type.Name == _base64ID && (type.Namespace == _soapNsID || type.Namespace == _soap12NsID))
                    value = null;
                else
                    value = null;
            }
            else if (type.Namespace == _schemaNonXsdTypesNsID)
            {
                if (type.Name == _charID)
                    value = default(Nullable<char>);
                else if (type.Name == _guidID)
                    value = default(Nullable<Guid>);
                else if (type.Name == _timeSpanID)
                    value = default(Nullable<TimeSpan>);
                else if (type.Name == _dateTimeOffsetID)
                    value = default(Nullable<DateTimeOffset>);
                else
                    value = null;
            }
            else
                value = null;
            return value;
        }

        protected bool IsXmlnsAttribute(string name)
        {
            if (!name.StartsWith("xmlns", StringComparison.Ordinal)) return false;
            if (name.Length == 5) return true;
            return name[5] == ':';
        }

        protected void ParseWsdlArrayType(XmlAttribute attr)
        {
            if (attr.LocalName == _wsdlArrayTypeID && attr.NamespaceURI == _wsdlNsID)
            {
                int colon = attr.Value.LastIndexOf(':');
                if (colon < 0)
                {
                    attr.Value = $"{_r.LookupNamespace("")}:{attr.Value}";
                }
                else
                {
                    attr.Value = $"{_r.LookupNamespace(attr.Value.Substring(0, colon))}:{attr.Value.AsSpan(colon + 1)}";
                }
            }
            return;
        }

        protected bool IsReturnValue
        {
            // value only valid for soap 1.1
            get { return _isReturnValue && !_soap12; }
            set { _isReturnValue = value; }
        }

        protected bool ReadNull()
        {
            if (!GetNullAttr()) return false;
            if (_r.IsEmptyElement)
            {
                _r.Skip();
                return true;
            }
            _r.ReadStartElement();
            while (_r.NodeType != XmlNodeType.EndElement)
            {
                UnknownNode(null);
            }
            ReadEndElement();
            return true;
        }

        protected bool GetNullAttr()
        {
            string? isNull =
                _r.GetAttribute(_nilID, _instanceNsID) ??
                _r.GetAttribute(_nullID, _instanceNsID) ??
                _r.GetAttribute(_nullID, _instanceNs2000ID) ??
                _r.GetAttribute(_nullID, _instanceNs1999ID);

            if (isNull == null || !XmlConvert.ToBoolean(isNull)) return false;
            return true;
        }

        protected string? ReadNullableString()
        {
            if (ReadNull()) return null;
            return _r.ReadElementString();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName? ReadNullableQualifiedName()
        {
            if (ReadNull()) return null;
            return ReadElementQualifiedName();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName ReadElementQualifiedName()
        {
            if (_r.IsEmptyElement)
            {
                XmlQualifiedName empty = new XmlQualifiedName(string.Empty, _r.LookupNamespace(""));
                _r.Skip();
                return empty;
            }
            XmlQualifiedName qname = ToXmlQualifiedName(CollapseWhitespace(_r.ReadString()));
            _r.ReadEndElement();
            return qname;
        }

        protected XmlDocument? ReadXmlDocument(bool wrapped)
        {
            XmlNode? n = ReadXmlNode(wrapped);
            if (n == null)
                return null;
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.ImportNode(n, true));
            return doc;
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected string? CollapseWhitespace(string? value)
        {
            if (value == null)
                return null;
            return value.Trim();
        }

        protected XmlNode? ReadXmlNode(bool wrapped)
        {
            XmlNode? node = null;
            if (wrapped)
            {
                if (ReadNull()) return null;
                _r.ReadStartElement();
                _r.MoveToContent();
                if (_r.NodeType != XmlNodeType.EndElement)
                    node = Document.ReadNode(_r);
                while (_r.NodeType != XmlNodeType.EndElement)
                {
                    UnknownNode(null);
                }
                _r.ReadEndElement();
            }
            else
            {
                node = Document.ReadNode(_r);
            }
            return node;
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static byte[]? ToByteArrayBase64(string? value)
        {
            return XmlCustomFormatter.ToByteArrayBase64(value);
        }

        protected byte[]? ToByteArrayBase64(bool isNull)
        {
            if (isNull)
            {
                return null;
            }
            return ReadByteArray(true); //means use Base64
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static byte[]? ToByteArrayHex(string? value)
        {
            return XmlCustomFormatter.ToByteArrayHex(value);
        }

        protected byte[]? ToByteArrayHex(bool isNull)
        {
            if (isNull)
            {
                return null;
            }
            return ReadByteArray(false); //means use BinHex
        }

        protected int GetArrayLength(string name, string ns)
        {
            if (GetNullAttr()) return 0;
            string? arrayType = _r.GetAttribute(_arrayTypeID, _soapNsID);
            SoapArrayInfo arrayInfo = ParseArrayType(arrayType!);
            if (arrayInfo.dimensions != 1) throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayDimentions, CurrentTag()));
            XmlQualifiedName qname = ToXmlQualifiedName(arrayInfo.qname, false);
            if (qname.Name != name) throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayTypeName, qname.Name, name, CurrentTag()));
            if (qname.Namespace != ns) throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayTypeNamespace, qname.Namespace, ns, CurrentTag()));
            return arrayInfo.length;
        }

        private struct SoapArrayInfo
        {
            public string qname;
            public int dimensions;
            public int length;
            public int jaggedDimensions;
        }

        private SoapArrayInfo ParseArrayType(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), SR.Format(SR.XmlMissingArrayType, CurrentTag()));
            }

            if (value.Length == 0)
            {
                throw new ArgumentException(SR.Format(SR.XmlEmptyArrayType, CurrentTag()), nameof(value));
            }

            int charsLength = value.Length;

            SoapArrayInfo soapArrayInfo = default;

            // Parse backwards to get length first, then optional dimensions, then qname.
            int pos = charsLength - 1;

            // Must end with ]
            if (value[pos] != ']')
            {
                throw new ArgumentException(SR.XmlInvalidArraySyntax, nameof(value));
            }
            pos--;

            // Find [
            while (pos != -1 && value[pos] != '[')
            {
                if (value[pos] == ',')
                    throw new ArgumentException(SR.Format(SR.XmlInvalidArrayDimentions, CurrentTag()), nameof(value));
                pos--;
            }
            if (pos == -1)
            {
                throw new ArgumentException(SR.XmlMismatchedArrayBrackets, nameof(value));
            }

            int len = charsLength - pos - 2;
            if (len > 0)
            {
                ReadOnlySpan<char> lengthStringSpan = value.AsSpan(pos + 1, len);
                if (!int.TryParse(lengthStringSpan, CultureInfo.InvariantCulture, out soapArrayInfo.length))
                    throw new ArgumentException(SR.Format(SR.XmlInvalidArrayLength, new string(lengthStringSpan)), nameof(value));
            }
            else
            {
                soapArrayInfo.length = -1;
            }

            pos--;

            soapArrayInfo.jaggedDimensions = 0;
            while (pos != -1 && value[pos] == ']')
            {
                pos--;
                if (pos < 0)
                    throw new ArgumentException(SR.XmlMismatchedArrayBrackets, nameof(value));
                if (value[pos] == ',')
                    throw new ArgumentException(SR.Format(SR.XmlInvalidArrayDimentions, CurrentTag()), nameof(value));
                else if (value[pos] != '[')
                    throw new ArgumentException(SR.XmlInvalidArraySyntax, nameof(value));
                pos--;
                soapArrayInfo.jaggedDimensions++;
            }

            soapArrayInfo.dimensions = 1;

            // everything else is qname - validation of qnames?
            soapArrayInfo.qname = new string(value.AsSpan(0, pos + 1));
            return soapArrayInfo;
        }

        private static SoapArrayInfo ParseSoap12ArrayType(string? itemType, string? arraySize)
        {
            SoapArrayInfo soapArrayInfo = default;

            if (itemType != null && itemType.Length > 0)
                soapArrayInfo.qname = itemType;
            else
                soapArrayInfo.qname = "";

            string[] dimensions;
            if (arraySize != null && arraySize.Length > 0)
                dimensions = arraySize.Split(null);
            else
                dimensions = Array.Empty<string>();

            soapArrayInfo.dimensions = 0;
            soapArrayInfo.length = -1;
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i].Length > 0)
                {
                    if (dimensions[i] == "*")
                    {
                        soapArrayInfo.dimensions++;
                    }
                    else
                    {
                        try
                        {
                            soapArrayInfo.length = int.Parse(dimensions[i], CultureInfo.InvariantCulture);
                            soapArrayInfo.dimensions++;
                        }
                        catch (Exception e)
                        {
                            if (e is OutOfMemoryException)
                            {
                                throw;
                            }
                            throw new ArgumentException(SR.Format(SR.XmlInvalidArrayLength, dimensions[i]), "value");
                        }
                    }
                }
            }
            if (soapArrayInfo.dimensions == 0)
                soapArrayInfo.dimensions = 1; // default is 1D even if no arraySize is specified

            return soapArrayInfo;
        }

        protected static DateTime ToDateTime(string value)
        {
            return XmlCustomFormatter.ToDateTime(value);
        }

        protected static DateTime ToDate(string value)
        {
            return XmlCustomFormatter.ToDate(value);
        }

        protected static DateTime ToTime(string value)
        {
            return XmlCustomFormatter.ToTime(value);
        }

        protected static char ToChar(string value)
        {
            return XmlCustomFormatter.ToChar(value);
        }

        protected static long ToEnum(string value, Hashtable h, string typeName)
        {
            return XmlCustomFormatter.ToEnum(value, h, typeName, true);
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static string? ToXmlName(string? value)
        {
            return XmlCustomFormatter.ToXmlName(value);
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static string? ToXmlNCName(string? value)
        {
            return XmlCustomFormatter.ToXmlNCName(value);
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static string? ToXmlNmToken(string? value)
        {
            return XmlCustomFormatter.ToXmlNmToken(value);
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected static string? ToXmlNmTokens(string? value)
        {
            return XmlCustomFormatter.ToXmlNmTokens(value);
        }

        protected XmlQualifiedName ToXmlQualifiedName(string? value)
        {
            return ToXmlQualifiedName(value, DecodeName);
        }

        internal XmlQualifiedName ToXmlQualifiedName(string? value, bool decodeName)
        {
            int colon = value == null ? -1 : value.LastIndexOf(':');
            string? prefix = colon < 0 ? null : value!.Substring(0, colon);
            string localName = value!.Substring(colon + 1);

            if (decodeName)
            {
                prefix = XmlConvert.DecodeName(prefix);
                localName = XmlConvert.DecodeName(localName);
            }
            if (string.IsNullOrEmpty(prefix))
            {
                return new XmlQualifiedName(_r.NameTable.Add(value), _r.LookupNamespace(string.Empty));
            }
            else
            {
                string? ns = _r.LookupNamespace(prefix);
                if (ns == null)
                {
                    // Namespace prefix '{0}' is not defined.
                    throw new InvalidOperationException(SR.Format(SR.XmlUndefinedAlias, prefix));
                }
                return new XmlQualifiedName(_r.NameTable.Add(localName), ns);
            }
        }
        protected void UnknownAttribute(object? o, XmlAttribute attr)
        {
            UnknownAttribute(o, attr, null);
        }

        protected void UnknownAttribute(object? o, XmlAttribute attr, string? qnames)
        {
            if (_events.OnUnknownAttribute != null)
            {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlAttributeEventArgs e = new XmlAttributeEventArgs(attr, lineNumber, linePosition, o, qnames);
                _events.OnUnknownAttribute(_events.sender, e);
            }
        }

        protected void UnknownElement(object? o, XmlElement elem)
        {
            UnknownElement(o, elem, null);
        }

        protected void UnknownElement(object? o, XmlElement elem, string? qnames)
        {
            if (_events.OnUnknownElement != null)
            {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlElementEventArgs e = new XmlElementEventArgs(elem, lineNumber, linePosition, o, qnames);
                _events.OnUnknownElement(_events.sender, e);
            }
        }

        protected void UnknownNode(object? o)
        {
            UnknownNode(o, null);
        }

        protected void UnknownNode(object? o, string? qnames)
        {
            if (_r.NodeType == XmlNodeType.None || _r.NodeType == XmlNodeType.Whitespace)
            {
                _r.Read();
                return;
            }
            if (_r.NodeType == XmlNodeType.EndElement)
                return;
            if (_events.OnUnknownNode != null)
            {
                UnknownNode(Document.ReadNode(_r), o, qnames);
            }
            else if (_r.NodeType == XmlNodeType.Attribute && _events.OnUnknownAttribute == null)
            {
                return;
            }
            else if (_r.NodeType == XmlNodeType.Element && _events.OnUnknownElement == null)
            {
                _r.Skip();
                return;
            }
            else
            {
                UnknownNode(Document.ReadNode(_r), o, qnames);
            }
        }

        private void UnknownNode(XmlNode? unknownNode, object? o, string? qnames)
        {
            if (unknownNode == null)
                return;
            if (unknownNode.NodeType != XmlNodeType.None && unknownNode.NodeType != XmlNodeType.Whitespace && _events.OnUnknownNode != null)
            {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlNodeEventArgs e = new XmlNodeEventArgs(unknownNode, lineNumber, linePosition, o);
                _events.OnUnknownNode(_events.sender, e);
            }
            if (unknownNode.NodeType == XmlNodeType.Attribute)
            {
                UnknownAttribute(o, (XmlAttribute)unknownNode, qnames);
            }
            else if (unknownNode.NodeType == XmlNodeType.Element)
            {
                UnknownElement(o, (XmlElement)unknownNode, qnames);
            }
        }

        private void GetCurrentPosition(out int lineNumber, out int linePosition)
        {
            if (Reader is IXmlLineInfo lineInfo)
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
                lineNumber = linePosition = -1;
        }

        protected void UnreferencedObject(string? id, object? o)
        {
            if (_events.OnUnreferencedObject != null)
            {
                UnreferencedObjectEventArgs e = new UnreferencedObjectEventArgs(o, id);
                _events.OnUnreferencedObject(_events.sender, e);
            }
        }

        private string CurrentTag() =>
            _r.NodeType switch
            {
                XmlNodeType.Element => $"<{_r.LocalName} xmlns='{_r.NamespaceURI}'>",
                XmlNodeType.EndElement => ">",
                XmlNodeType.Text => _r.Value,
                XmlNodeType.CDATA => "CDATA",
                XmlNodeType.Comment => "<--",
                XmlNodeType.ProcessingInstruction => "<?",
                _ => "(unknown)",
            };

        protected Exception CreateUnknownTypeException(XmlQualifiedName type)
        {
            return new InvalidOperationException(SR.Format(SR.XmlUnknownType, type.Name, type.Namespace, CurrentTag()));
        }

        protected Exception CreateReadOnlyCollectionException(string name)
        {
            return new InvalidOperationException(SR.Format(SR.XmlReadOnlyCollection, name));
        }

        protected Exception CreateAbstractTypeException(string name, string? ns)
        {
            return new InvalidOperationException(SR.Format(SR.XmlAbstractType, name, ns, CurrentTag()));
        }

        protected Exception CreateInaccessibleConstructorException(string typeName)
        {
            return new InvalidOperationException(SR.Format(SR.XmlConstructorInaccessible, typeName));
        }

        protected Exception CreateCtorHasSecurityException(string typeName)
        {
            return new InvalidOperationException(SR.Format(SR.XmlConstructorHasSecurityAttributes, typeName));
        }

        protected Exception CreateUnknownNodeException()
        {
            return new InvalidOperationException(SR.Format(SR.XmlUnknownNode, CurrentTag()));
        }

        protected Exception CreateUnknownConstantException(string? value, Type enumType)
        {
            return new InvalidOperationException(SR.Format(SR.XmlUnknownConstant, value, enumType.Name));
        }

        protected Exception CreateInvalidCastException(Type type, object? value)
        {
            return CreateInvalidCastException(type, value, null);
        }

        protected Exception CreateInvalidCastException(Type type, object? value, string? id)
        {
            if (value == null)
                return new InvalidCastException(SR.Format(SR.XmlInvalidNullCast, type.FullName));
            else if (id == null)
                return new InvalidCastException(SR.Format(SR.XmlInvalidCast, value.GetType().FullName, type.FullName));
            else
                return new InvalidCastException(SR.Format(SR.XmlInvalidCastWithId, value.GetType().FullName, type.FullName, id));
        }

        protected Exception CreateBadDerivationException(string? xsdDerived, string? nsDerived, string? xsdBase, string? nsBase, string? clrDerived, string? clrBase)
        {
            return new InvalidOperationException(SR.Format(SR.XmlSerializableBadDerivation, xsdDerived, nsDerived, xsdBase, nsBase, clrDerived, clrBase));
        }

        protected Exception CreateMissingIXmlSerializableType(string? name, string? ns, string? clrType)
        {
            return new InvalidOperationException(SR.Format(SR.XmlSerializableMissingClrType, name, ns, nameof(XmlIncludeAttribute), clrType));
            //XmlSerializableMissingClrType= Type '{0}' from namespace '{1}' doesnot have corresponding IXmlSerializable type. Please consider adding {2} to '{3}'.
        }

        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected Array EnsureArrayIndex(Array? a, int index, Type elementType)
        {
            if (a == null) return Array.CreateInstance(elementType, 32);
            if (index < a.Length) return a;
            Array b = Array.CreateInstance(elementType, a.Length * 2);
            Array.Copy(a, b, index);
            return b;
        }

        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected Array? ShrinkArray(Array? a, int length, Type elementType, bool isNullable)
        {
            if (a == null)
            {
                if (isNullable) return null;
                return Array.CreateInstance(elementType, 0);
            }
            if (a.Length == length) return a;
            Array b = Array.CreateInstance(elementType, length);
            Array.Copy(a, b, length);
            return b;
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected string? ReadString(string? value)
        {
            return ReadString(value, false);
        }

        [return: NotNullIfNotNull(nameof(value))]
        protected string? ReadString(string? value, bool trim)
        {
            string str = _r.ReadString();
            if (str != null && trim)
                str = str.Trim();
            if (string.IsNullOrEmpty(value))
                return str;
            return value + str;
        }

        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable)
        {
            return ReadSerializable(serializable, false);
        }

        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable, bool wrappedAny)
        {
            string? name = null;
            string? ns = null;

            if (wrappedAny)
            {
                name = _r.LocalName;
                ns = _r.NamespaceURI;
                _r.Read();
                _r.MoveToContent();
            }
            serializable.ReadXml(_r);

            if (wrappedAny)
            {
                while (_r.NodeType == XmlNodeType.Whitespace) _r.Skip();
                if (_r.NodeType == XmlNodeType.None) _r.Skip();
                if (_r.NodeType == XmlNodeType.EndElement && _r.LocalName == name && _r.NamespaceURI == ns)
                {
                    Reader.Read();
                }
            }
            return serializable;
        }

        protected bool ReadReference([NotNullWhen(true)] out string? fixupReference)
        {
            string? href = _soap12 ? _r.GetAttribute("ref", Soap12.Encoding) : _r.GetAttribute("href");
            if (href == null)
            {
                fixupReference = null;
                return false;
            }
            if (!_soap12)
            {
                // soap 1.1 href starts with '#'; soap 1.2 ref does not
                if (!href.StartsWith('#')) throw new InvalidOperationException(SR.Format(SR.XmlMissingHref, href));
                fixupReference = href.Substring(1);
            }
            else
                fixupReference = href;

            if (_r.IsEmptyElement)
            {
                _r.Skip();
            }
            else
            {
                _r.ReadStartElement();
                ReadEndElement();
            }
            return true;
        }

        protected void AddTarget(string? id, object? o)
        {
            if (id == null)
            {
                _targetsWithoutIds ??= new ArrayList();
                if (o != null)
                    _targetsWithoutIds.Add(o);
            }
            else
            {
                _targets ??= new Hashtable();
                if (!_targets.Contains(id))
                    _targets.Add(id, o);
            }
        }

        protected void AddFixup(Fixup? fixup)
        {
            _fixups ??= new ArrayList();
            _fixups.Add(fixup);
        }

        protected void AddFixup(CollectionFixup? fixup)
        {
            _collectionFixups ??= new ArrayList();
            _collectionFixups.Add(fixup);
        }

        protected object GetTarget(string id)
        {
            object? target = _targets?[id];
            if (target == null)
            {
                throw new InvalidOperationException(SR.Format(SR.XmlInvalidHref, id));
            }
            Referenced(target);
            return target;
        }

        protected void Referenced(object? o)
        {
            if (o == null) return;
            _referencedTargets ??= new Hashtable();
            _referencedTargets[o] = o;
        }

        private void HandleUnreferencedObjects()
        {
            if (_targets != null)
            {
                foreach (DictionaryEntry target in _targets)
                {
                    if (_referencedTargets == null || !_referencedTargets.Contains(target.Value!))
                    {
                        UnreferencedObject((string)target.Key, target.Value!);
                    }
                }
            }
            if (_targetsWithoutIds != null)
            {
                foreach (object o in _targetsWithoutIds)
                {
                    if (_referencedTargets == null || !_referencedTargets.Contains(o))
                    {
                        UnreferencedObject(null, o);
                    }
                }
            }
        }

        private void DoFixups()
        {
            if (_fixups == null) return;
            for (int i = 0; i < _fixups.Count; i++)
            {
                Fixup fixup = (Fixup)_fixups[i]!;
                fixup.Callback(fixup);
            }

            if (_collectionFixups == null) return;
            for (int i = 0; i < _collectionFixups.Count; i++)
            {
                CollectionFixup collectionFixup = (CollectionFixup)_collectionFixups[i]!;
                collectionFixup.Callback(collectionFixup.Collection, collectionFixup.CollectionItems);
            }
        }

        protected void FixupArrayRefs(object fixup)
        {
            Fixup f = (Fixup)fixup;
            Array array = (Array)f.Source!;
            for (int i = 0; i < array.Length; i++)
            {
                string? id = f.Ids![i];
                if (id == null) continue;
                object o = GetTarget(id);
                try
                {
                    array.SetValue(o, i);
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayRef, id, o.GetType().FullName, i.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        [RequiresUnreferencedCode("calls GetArrayElementType")]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        private Array? ReadArray(string? typeName, string? typeNs)
        {
            SoapArrayInfo arrayInfo;
            Type? fallbackElementType = null;
            if (_soap12)
            {
                string? itemType = _r.GetAttribute(_itemTypeID, _soap12NsID);
                string? arraySize = _r.GetAttribute(_arraySizeID, _soap12NsID);
                Type? arrayType = (Type?)_types[new XmlQualifiedName(typeName, typeNs)];
                // no indication that this is an array?
                if (itemType == null && arraySize == null && (arrayType == null || !arrayType.IsArray))
                    return null;

                arrayInfo = ParseSoap12ArrayType(itemType, arraySize);
                if (arrayType != null)
                    fallbackElementType = TypeScope.GetArrayElementType(arrayType, null);
            }
            else
            {
                string? arrayType = _r.GetAttribute(_arrayTypeID, _soapNsID);
                if (arrayType == null)
                    return null;

                arrayInfo = ParseArrayType(arrayType);
            }

            if (arrayInfo.dimensions != 1) throw new InvalidOperationException(SR.Format(SR.XmlInvalidArrayDimentions, CurrentTag()));

            // NOTE: don't use the array size that is specified since an evil client might pass
            // a number larger than the actual number of items in an attempt to harm the server.

            XmlQualifiedName qname;
            bool isPrimitive;
            Type? elementType = null;
            XmlQualifiedName urTypeName = new XmlQualifiedName(_urTypeID, _schemaNsID);
            if (arrayInfo.qname.Length > 0)
            {
                qname = ToXmlQualifiedName(arrayInfo.qname, false);
                elementType = (Type?)_types[qname];
            }
            else
                qname = urTypeName;

            // try again if the best we could come up with was object
            if (_soap12 && elementType == typeof(object))
                elementType = null;

            if (elementType == null)
            {
                if (!_soap12)
                {
                    elementType = GetPrimitiveType(qname, true)!;
                    isPrimitive = true;
                }
                else
                {
                    // try it as a primitive
                    if (qname != urTypeName)
                        elementType = GetPrimitiveType(qname, false);
                    if (elementType != null)
                    {
                        isPrimitive = true;
                    }
                    else
                    {
                        // still nothing: go with fallback type or object
                        if (fallbackElementType == null)
                        {
                            elementType = typeof(object);
                            isPrimitive = false;
                        }
                        else
                        {
                            elementType = fallbackElementType;
                            XmlQualifiedName? newQname = (XmlQualifiedName?)_typesReverse[elementType];
                            if (newQname == null)
                            {
                                newQname = XmlSerializationWriter.GetPrimitiveTypeNameInternal(elementType);
                                isPrimitive = true;
                            }
                            else
                                isPrimitive = elementType.IsPrimitive;
                            if (newQname != null) qname = newQname;
                        }
                    }
                }
            }
            else
                isPrimitive = elementType.IsPrimitive;

            if (!_soap12 && arrayInfo.jaggedDimensions > 0)
            {
                for (int i = 0; i < arrayInfo.jaggedDimensions; i++)
                    elementType = elementType.MakeArrayType();
            }

            if (_r.IsEmptyElement)
            {
                _r.Skip();
                return Array.CreateInstance(elementType, 0);
            }

            _r.ReadStartElement();
            _r.MoveToContent();

            int arrayLength = 0;
            Array? array = null;

            if (elementType.IsValueType)
            {
                if (!isPrimitive && !elementType.IsEnum)
                {
                    throw new NotSupportedException(SR.Format(SR.XmlRpcArrayOfValueTypes, elementType.FullName));
                }
                // CONSIDER, erikc, we could have specialized read functions here
                // for primitives, which would avoid boxing.
                while (_r.NodeType != XmlNodeType.EndElement)
                {
                    array = EnsureArrayIndex(array, arrayLength, elementType);
                    array.SetValue(ReadReferencedElement(qname.Name, qname.Namespace), arrayLength);
                    arrayLength++;
                    _r.MoveToContent();
                }
                array = ShrinkArray(array, arrayLength, elementType, false);
            }
            else
            {
                string type;
                string typens;
                string[]? ids = null;
                int idsLength = 0;

                while (_r.NodeType != XmlNodeType.EndElement)
                {
                    array = EnsureArrayIndex(array, arrayLength, elementType);
                    ids = (string[])EnsureArrayIndex(ids, idsLength, typeof(string));
                    // CONSIDER: i'm not sure it's correct to let item name take precedence over arrayType
                    if (_r.NamespaceURI.Length != 0)
                    {
                        type = _r.LocalName;
                        if (_r.NamespaceURI == _soapNsID)
                            typens = XmlSchema.Namespace;
                        else
                            typens = _r.NamespaceURI;
                    }
                    else
                    {
                        type = qname.Name;
                        typens = qname.Namespace;
                    }
                    array.SetValue(ReadReferencingElement(type, typens, out ids[idsLength]!), arrayLength);
                    arrayLength++;
                    idsLength++;
                    // CONSIDER, erikc, sparse arrays, perhaps?
                    _r.MoveToContent();
                }

                // special case for soap 1.2: try to get a better fit than object[] when no metadata is known
                // this applies in the doc/enc/bare case
                if (_soap12 && elementType == typeof(object))
                {
                    Type? itemType = null;
                    for (int i = 0; i < arrayLength; i++)
                    {
                        object? currItem = array!.GetValue(i);
                        if (currItem != null)
                        {
                            Type currItemType = currItem.GetType();
                            if (currItemType.IsValueType)
                            {
                                itemType = null;
                                break;
                            }
                            if (itemType == null || currItemType.IsAssignableFrom(itemType))
                            {
                                itemType = currItemType;
                            }
                            else if (!itemType.IsAssignableFrom(currItemType))
                            {
                                itemType = null;
                                break;
                            }
                        }
                    }
                    if (itemType != null)
                        elementType = itemType;
                }
                ids = (string[]?)ShrinkArray(ids, idsLength, typeof(string), false);
                array = ShrinkArray(array, arrayLength, elementType, false);
                Fixup fixupArray = new Fixup(array, new XmlSerializationFixupCallback(this.FixupArrayRefs), ids);
                AddFixup(fixupArray);
            }

            // CONSIDER, erikc, check to see if the specified array length was right, perhaps?

            ReadEndElement();
            return array;
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected abstract void InitCallbacks();

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected void ReadReferencedElements()
        {
            _r.MoveToContent();
            while (_r.NodeType != XmlNodeType.EndElement && _r.NodeType != XmlNodeType.None)
            {
                ReadReferencingElement(null, null, true, out _);
                _r.MoveToContent();
            }
            DoFixups();

            HandleUnreferencedObjects();
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected object? ReadReferencedElement()
        {
            return ReadReferencedElement(null, null);
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected object? ReadReferencedElement(string? name, string? ns)
        {
            return ReadReferencingElement(name, ns, out _);
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected object? ReadReferencingElement(out string? fixupReference)
        {
            return ReadReferencingElement(null, null, out fixupReference);
        }

        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected object? ReadReferencingElement(string? name, string? ns, out string? fixupReference)
        {
            return ReadReferencingElement(name, ns, false, out fixupReference);
        }

        [MemberNotNull(nameof(_callbacks))]
        [RequiresUnreferencedCode(XmlSerializer.TrimSerializationWarning)]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        protected object? ReadReferencingElement(string? name, string? ns, bool elementCanBeType, out string? fixupReference)
        {
            object? o;
            EnsureCallbackTables();

            _r.MoveToContent();

            if (ReadReference(out fixupReference)) return null;

            if (ReadNull()) return null;

            string? id = _soap12 ? _r.GetAttribute("id", Soap12.Encoding) : _r.GetAttribute("id", null);

            if ((o = ReadArray(name, ns)) == null)
            {
                XmlQualifiedName? typeId = GetXsiType();
                if (typeId == null)
                {
                    if (name == null)
                        typeId = new XmlQualifiedName(_r.NameTable.Add(_r.LocalName), _r.NameTable.Add(_r.NamespaceURI));
                    else
                        typeId = new XmlQualifiedName(_r.NameTable.Add(name), _r.NameTable.Add(ns!));
                }
                XmlSerializationReadCallback? callback = (XmlSerializationReadCallback?)_callbacks[typeId];
                if (callback != null)
                {
                    o = callback();
                }
                else
                    o = ReadTypedPrimitive(typeId, elementCanBeType);
            }

            AddTarget(id, o);

            return o;
        }

        [MemberNotNull(nameof(_callbacks))]
        [RequiresUnreferencedCode("calls InitCallbacks")]
        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        internal void EnsureCallbackTables()
        {
            if (_callbacks == null)
            {
                _callbacks = new Hashtable();
                _types = new Hashtable();
                XmlQualifiedName urType = new XmlQualifiedName(_urTypeID, _r.NameTable.Add(XmlSchema.Namespace));
                _types.Add(urType, typeof(object));
                _typesReverse = new Hashtable();
                _typesReverse.Add(typeof(object), urType);
                InitCallbacks();
            }
        }

        protected void AddReadCallback(string name, string ns, Type type, XmlSerializationReadCallback read)
        {
            XmlQualifiedName typeName = new XmlQualifiedName(_r.NameTable.Add(name), _r.NameTable.Add(ns));
            _callbacks![typeName] = read;
            _types[typeName] = type;
            _typesReverse[type] = typeName;
        }

        protected void ReadEndElement()
        {
            while (_r.NodeType == XmlNodeType.Whitespace) _r.Skip();
            if (_r.NodeType == XmlNodeType.None) _r.Skip();
            else _r.ReadEndElement();
        }

        private object ReadXmlNodes(bool elementCanBeType)
        {
            var xmlNodeList = new List<XmlNode>();
            string elemLocalName = Reader.LocalName;
            string elemNs = Reader.NamespaceURI;
            string elemName = Reader.Name;
            string? xsiTypeName = null;
            string? xsiTypeNs = null;
            int skippableNodeCount = 0;
            XmlNode? unknownNode;
            if (Reader.NodeType == XmlNodeType.Attribute)
            {
                XmlAttribute attr = Document.CreateAttribute(elemName, elemNs);
                attr.Value = Reader.Value;
                unknownNode = attr;
            }
            else
                unknownNode = Document.CreateElement(elemName, elemNs);
            GetCurrentPosition(out _, out _);
            XmlElement? unknownElement = unknownNode as XmlElement;

            while (Reader.MoveToNextAttribute())
            {
                if (IsXmlnsAttribute(Reader.Name) || (Reader.Name == "id" && (!_soap12 || Reader.NamespaceURI == Soap12.Encoding)))
                    skippableNodeCount++;
                if (Reader.LocalName == _typeID &&
                     (Reader.NamespaceURI == _instanceNsID ||
                       Reader.NamespaceURI == _instanceNs2000ID ||
                       Reader.NamespaceURI == _instanceNs1999ID
                     )
                   )
                {
                    string value = Reader.Value;
                    int colon = value.LastIndexOf(':');
                    xsiTypeName = (colon >= 0) ? value.Substring(colon + 1) : value;
                    xsiTypeNs = Reader.LookupNamespace((colon >= 0) ? value.Substring(0, colon) : "");
                }
                XmlAttribute xmlAttribute = (XmlAttribute)Document.ReadNode(_r)!;
                xmlNodeList.Add(xmlAttribute);
                unknownElement?.SetAttributeNode(xmlAttribute);
            }

            // If the node is referenced (or in case of paramStyle = bare) and if xsi:type is not
            // specified then the element name is used as the type name. Reveal this to the user
            // by adding an extra attribute node "xsi:type" with value as the element name.
            if (elementCanBeType && xsiTypeName == null)
            {
                xsiTypeName = elemLocalName;
                xsiTypeNs = elemNs;
                XmlAttribute xsiTypeAttribute = Document.CreateAttribute(_typeID, _instanceNsID);
                xsiTypeAttribute.Value = elemName;
                xmlNodeList.Add(xsiTypeAttribute);
            }
            if (xsiTypeName == Soap.UrType &&
                (xsiTypeNs == _schemaNsID ||
                  xsiTypeNs == _schemaNs1999ID ||
                  xsiTypeNs == _schemaNs2000ID
                )
               )
                skippableNodeCount++;


            Reader.MoveToElement();
            if (Reader.IsEmptyElement)
            {
                Reader.Skip();
            }
            else
            {
                Reader.ReadStartElement();
                Reader.MoveToContent();
                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    XmlNode xmlNode = Document.ReadNode(_r)!;
                    xmlNodeList.Add(xmlNode);
                    unknownElement?.AppendChild(xmlNode);
                    Reader.MoveToContent();
                }
                ReadEndElement();
            }


            if (xmlNodeList.Count <= skippableNodeCount)
                return new object();

            XmlNode[] childNodes = xmlNodeList.ToArray();

            UnknownNode(unknownNode, null, null);
            return childNodes;
        }

        protected void CheckReaderCount(ref int whileIterations, ref int readerCount)
        {
        }

        ///<internalonly/>
        protected class Fixup
        {
            private readonly XmlSerializationFixupCallback _callback;
            private object? _source;
            private readonly string?[]? _ids;

            public Fixup(object? o, XmlSerializationFixupCallback callback, int count)
                : this(o, callback, new string[count])
            {
            }

            public Fixup(object? o, XmlSerializationFixupCallback callback, string?[]? ids)
            {
                _callback = callback;
                this.Source = o;
                _ids = ids;
            }

            public XmlSerializationFixupCallback Callback
            {
                get { return _callback; }
            }

            public object? Source
            {
                get { return _source; }
                set { _source = value; }
            }

            public string?[]? Ids
            {
                get { return _ids; }
            }
        }

        protected class CollectionFixup
        {
            private readonly XmlSerializationCollectionFixupCallback _callback;
            private readonly object? _collection;
            private readonly object _collectionItems;

            public CollectionFixup(object? collection, XmlSerializationCollectionFixupCallback callback, object collectionItems)
            {
                _callback = callback;
                _collection = collection;
                _collectionItems = collectionItems;
            }

            public XmlSerializationCollectionFixupCallback Callback
            {
                get { return _callback; }
            }

            public object? Collection
            {
                get { return _collection; }
            }

            public object CollectionItems
            {
                get { return _collectionItems; }
            }
        }
    }

    ///<internalonly/>
    public delegate void XmlSerializationFixupCallback(object fixup);


    ///<internalonly/>
    public delegate void XmlSerializationCollectionFixupCallback(object? collection, object? collectionItems);

    ///<internalonly/>
    public delegate object? XmlSerializationReadCallback();
}
