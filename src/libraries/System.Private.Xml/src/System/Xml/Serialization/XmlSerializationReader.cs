// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization.CodeGenerations;
using System.Xml.Serialization.Environments;
using System.Xml.Serialization.Environments.Comparers;
using System.Xml.Serialization.Environments.Schemas;
using System.Xml.Serialization.Environments.Types;
using System.Xml.Serialization.Environments.Values;
using System.Xml.Serialization.Mappings;
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
        private FrozenDictionary<XmlQualifiedName, Mapper> TypeMappers { get; set; } = null!;
        private XmlEnvironment Environment { get; set; } = null!;

        private string _wsdlNsID = null!;
        private string _wsdlArrayTypeID = null!;
        private string _typeID = null!;
        private string _arrayTypeID = null!;
        private string _itemTypeID = null!;
        private string _arraySizeID = null!;
        private string _arrayID = null!;
        private string _urTypeID = null!;

        protected abstract void InitIDs();

        // this method must be called before any generated deserialization methods are called
        internal void Init(XmlReader r, XmlDeserializationEvents events, string? encodingStyle)
        {
            _events = events;
            _r = r;
            _d = null;
            _soap12 = (encodingStyle == Soap12.Encoding);

            r.NameTable.Add("schema");
            _wsdlNsID = r.NameTable.Add(Wsdl.Namespace);
            _wsdlArrayTypeID = r.NameTable.Add(Wsdl.ArrayType);
            _typeID = r.NameTable.Add("type");
            _arrayTypeID = r.NameTable.Add("arrayType");
            _itemTypeID = r.NameTable.Add("itemType");
            _arraySizeID = r.NameTable.Add("arraySize");
            _arrayID = r.NameTable.Add("Array");
            _urTypeID = r.NameTable.Add(Soap.UrType);
            InitIDs();

            Environment ??= PrepareEnvironment();
            TypeMappers ??= PrepareTypeMappers();
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

        private PrimitiveTypesEnvironment PreparePrimitiveTypes()
        {
            PrimitiveTypesEnvironment returnValue;

            returnValue = new PrimitiveTypesEnvironment
            {
                AnyUriId = _r.NameTable.Add(DataTypeNames.AnyUri),
                Base64Id = _r.NameTable.Add(DataTypeNames.Base64),
                BooleanId = _r.NameTable.Add(DataTypeNames.Boolean),
                ByteArrayBase64Id = _r.NameTable.Add(DataTypeNames.ByteArrayBase64),
                ByteArrayHexId = _r.NameTable.Add(DataTypeNames.ByteArrayHex),
                ByteId = _r.NameTable.Add(DataTypeNames.Byte),
                CharId = _r.NameTable.Add(DataTypeNames.Char),
                DateId = _r.NameTable.Add(DataTypeNames.Date),
                DateTimeId = _r.NameTable.Add(DataTypeNames.DateTime),
                DateTimeOffsetId = _r.NameTable.Add(DataTypeNames.DateTimeOffset),
                DecimalId = _r.NameTable.Add(DataTypeNames.Decimal),
                DoubleId = _r.NameTable.Add(DataTypeNames.Double),
                DurationId = _r.NameTable.Add(DataTypeNames.Duration),
                EntitiesId = _r.NameTable.Add(DataTypeNames.Entities),
                EntityId = _r.NameTable.Add(DataTypeNames.Entity),
                GDayId = _r.NameTable.Add(DataTypeNames.GDay),
                GMonthDayId = _r.NameTable.Add(DataTypeNames.GMonthDay),
                GMonthId = _r.NameTable.Add(DataTypeNames.GMonth),
                GuidId = _r.NameTable.Add(DataTypeNames.Guid),
                GYearId = _r.NameTable.Add(DataTypeNames.GYear),
                GYearMonthId = _r.NameTable.Add(DataTypeNames.GYearMonth),
                IdId = _r.NameTable.Add(DataTypeNames.Id),
                IdRefId = _r.NameTable.Add(DataTypeNames.IdRef),
                IdRefsId = _r.NameTable.Add(DataTypeNames.IdRefs),
                Int16Id = _r.NameTable.Add(DataTypeNames.Int16),
                Int32Id = _r.NameTable.Add(DataTypeNames.Int32),
                Int64Id = _r.NameTable.Add(DataTypeNames.Int64),
                IntegerId = _r.NameTable.Add(DataTypeNames.Integer),
                LanguageId = _r.NameTable.Add(DataTypeNames.Language),
                NegativeIntegerId = _r.NameTable.Add(DataTypeNames.NegativeInteger),
                NoncolonizedNameId = _r.NameTable.Add(DataTypeNames.NoncolonizedName),
                NonNegativeIntegerId = _r.NameTable.Add(DataTypeNames.NonNegativeInteger),
                NonPositiveIntegerId = _r.NameTable.Add(DataTypeNames.NonPositiveInteger),
                NormalizedStringId = _r.NameTable.Add(DataTypeNames.NormalizedString),
                NotationId = _r.NameTable.Add(DataTypeNames.Notation),
                OldTimeInstantId = _r.NameTable.Add(DataTypeNames.OldTimeInstant),
                PositiveIntegerId = _r.NameTable.Add(DataTypeNames.PositiveInteger),
                SByteId = _r.NameTable.Add(DataTypeNames.SByte),
                SingleId = _r.NameTable.Add(DataTypeNames.Single),
                StringId = _r.NameTable.Add(DataTypeNames.String),
                TimeId = _r.NameTable.Add(DataTypeNames.Time),
                TimeSpanId = _r.NameTable.Add(DataTypeNames.TimeSpan),
                TokenId = _r.NameTable.Add(DataTypeNames.Token),
                UInt16Id = _r.NameTable.Add(DataTypeNames.UInt16),
                UInt32Id = _r.NameTable.Add(DataTypeNames.UInt32),
                UInt64Id = _r.NameTable.Add(DataTypeNames.UInt64),
                XmlNameId = _r.NameTable.Add(DataTypeNames.XmlName),
                XmlNmTokenId = _r.NameTable.Add(DataTypeNames.XmlNmToken),
                XmlNmTokensId = _r.NameTable.Add(DataTypeNames.XmlNmTokens),
                XmlQualifiedNameId = _r.NameTable.Add(DataTypeNames.XmlQualifiedName),
            };

            return returnValue;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName? GetXsiType()
        {
            string? type = _r.GetAttribute(_typeID, Environment.Schemas.Instances.NamespaceId);
            if (type == null)
            {
                type = _r.GetAttribute(_typeID, Environment.Schemas.Instances.Namespace2000Id);
                if (type == null)
                {
                    type = _r.GetAttribute(_typeID, Environment.Schemas.Instances.Namespace1999Id);
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
            Type? returnValue;

            if (TypeMappers.TryGetValue(typeName, out Mapper? mapper))
            {
                returnValue = mapper.PrimitiveType;
            }
            else
            {
                if ((typeName.Namespace == Environment.Schemas.NamespaceId)
                    || (typeName.Namespace == Environment.Schemas.SoapNamespaceId)
                    || (typeName.Namespace == Environment.Schemas.Soap12NamespaceId)
                    || (typeName.Namespace == Environment.Schemas.Namespace2000Id)
                    || (typeName.Namespace == Environment.Schemas.Namespace1999Id))
                {
                    throw CreateUnknownTypeException(typeName);
                }
                else
                {
                    if (throwOnUnknown)
                    {
                        throw CreateUnknownTypeException(typeName);
                    }
                    else
                    {
                        returnValue = null;
                    }
                }
            }

            return returnValue;
        }

        private bool IsPrimitiveNamespace(string ns)
        {
            return Environment.Schemas.PrimitiveNamespaceIds.Value.Contains(ns);
        }

        internal XmlEnvironment PrepareEnvironment()
        {
            InstancesEnvironment instances;
            SchemasEnvironment schemas;
            TypesEnvironment types;
            ValuesEnvironment values;
            XmlEnvironment returnValue;

            instances = new InstancesEnvironment
            {
                NamespaceId = _r.NameTable.Add(XmlSchema.InstanceNamespace),
                Namespace1999Id = _r.NameTable.Add(XmlReservedNs.InstanceNamespace1999),
                Namespace2000Id = _r.NameTable.Add(XmlReservedNs.InstanceNamespace2000)
            };

            schemas = new SchemasEnvironment
            {
                Instances = instances,
                NamespaceId = _r.NameTable.Add(XmlSchema.Namespace),
                Namespace1999Id = _r.NameTable.Add(XmlReservedNs.Namespace1999),
                Namespace2000Id = _r.NameTable.Add(XmlReservedNs.Namespace2000),
                NonXsdTypesNamespaceId = _r.NameTable.Add(UrtTypes.Namespace),
                SoapNamespaceId = _r.NameTable.Add(Soap.Encoding),
                Soap12NamespaceId = _r.NameTable.Add(Soap12.Encoding)
            };

            types = new TypesEnvironment
            {
                PrimitiveTypes = PreparePrimitiveTypes()
            };

            values = new ValuesEnvironment
            {
                NilId = _r.NameTable.Add("nil"),
                NullId = _r.NameTable.Add("null"),
            };

            returnValue = new XmlEnvironment(schemas, types, values);

            return returnValue;
        }

        internal FrozenDictionary<XmlQualifiedName, Mapper> PrepareTypeMappers()
        {
            Mapper mapperForAnyUriId,
                mapperForBase64Id,
                mapperForBooleanId,
                mapperForByteArrayBase64Id,
                mapperForByteArrayHexId,
                mapperForByteId,
                mapperForCharId,
                mapperForDateId,
                mapperForDateTimeId,
                mapperForDateTimeOffsetId,
                mapperForDecimalId,
                mapperForDoubleId,
                mapperForDurationId,
                mapperForEntitiesId,
                mapperForEntityId,
                mapperForGDayId,
                mapperForGMonthDayId,
                mapperForGMonthId,
                mapperForGuidId,
                mapperForGYearId,
                mapperForGYearMonthId,
                mapperForIdId,
                mapperForIdRefId,
                mapperForIdRefsId,
                mapperForInt16Id,
                mapperForInt32Id,
                mapperForInt64Id,
                mapperForIntegerId,
                mapperForLanguageId,
                mapperForNegativeIntegerId,
                mapperForNoncolonizedNameId,
                mapperForNonNegativeIntegerId,
                mapperForNonPositiveIntegerId,
                mapperForNormalizedStringId,
                mapperForNotationId,
                mapperForOldTimeInstantId,
                mapperForPositiveIntegerId,
                mapperForSByteId,
                mapperForSingleId,
                mapperForStringId,
                mapperForTimeId,
                mapperForTimeSpanId,
                mapperForTokenId,
                mapperForUInt16Id,
                mapperForUInt32Id,
                mapperForUInt64Id,
                mapperForXmlNameId,
                mapperForXmlNmTokenId,
                mapperForXmlNmTokensId,
                mapperForXmlQualifiedNameId;
            IReadOnlyList<KeyValuePair<XmlQualifiedName, Mapper>> rawTypeMappers;
            FrozenDictionary<XmlQualifiedName, Mapper> returnValue;

            mapperForAnyUriId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForBase64Id = new Mapper(typeof(byte[]), () => ReadByteArray(true));
            mapperForBooleanId = new Mapper(typeof(bool), () => XmlConvert.ToBoolean(ReadStringValue()));
            mapperForByteArrayBase64Id = new Mapper(typeof(byte[]), () => ReadByteArray(true));
            mapperForByteArrayHexId = new Mapper(typeof(byte[]), () => ReadByteArray(false));
            mapperForByteId = new Mapper(typeof(byte), () => XmlConvert.ToByte(ReadStringValue()));
            mapperForCharId = new Mapper(typeof(char), () => XmlCustomFormatter.ToChar(ReadStringValue()));
            mapperForDateId = new Mapper(typeof(DateTime), () => XmlCustomFormatter.ToDate(ReadStringValue()));
            mapperForDateTimeId = new Mapper(typeof(DateTime), () => XmlCustomFormatter.ToDateTime(ReadStringValue()));
            mapperForDateTimeOffsetId = new Mapper(typeof(DateTimeOffset), () => XmlConvert.ToDateTimeOffset(ReadStringValue()));
            mapperForDecimalId = new Mapper(typeof(decimal), () => XmlConvert.ToDecimal(ReadStringValue()));
            mapperForDoubleId = new Mapper(typeof(double), () => XmlConvert.ToDouble(ReadStringValue()));
            mapperForDurationId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForEntitiesId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForEntityId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForGDayId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForGMonthDayId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForGMonthId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForGuidId = new Mapper(typeof(Guid), () => new Guid(CollapseWhitespace(ReadStringValue())));
            mapperForGYearId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForGYearMonthId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForIdId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForIdRefId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForIdRefsId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForInt16Id = new Mapper(typeof(short), () => XmlConvert.ToInt16(ReadStringValue()));
            mapperForInt32Id = new Mapper(typeof(int), () => XmlConvert.ToInt32(ReadStringValue()));
            mapperForInt64Id = new Mapper(typeof(long), () => XmlConvert.ToInt64(ReadStringValue()));
            mapperForIntegerId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForLanguageId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForNegativeIntegerId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForNoncolonizedNameId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForNonNegativeIntegerId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForNonPositiveIntegerId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForNormalizedStringId = new Mapper(typeof(string), ReadStringValue);
            mapperForNotationId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForOldTimeInstantId = new Mapper(typeof(DateTime), () => XmlCustomFormatter.ToDateTime(ReadStringValue()));
            mapperForPositiveIntegerId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForSByteId = new Mapper(typeof(sbyte), () => XmlConvert.ToSByte(ReadStringValue()));
            mapperForSingleId = new Mapper(typeof(float), () => XmlConvert.ToSingle(ReadStringValue()));
            mapperForStringId = new Mapper(typeof(string), ReadStringValue);
            mapperForTimeId = new Mapper(typeof(DateTime), () => XmlCustomFormatter.ToTime(ReadStringValue()));
            mapperForTimeSpanId = new Mapper(typeof(TimeSpan), () => XmlConvert.ToTimeSpan(ReadStringValue()));
            mapperForTokenId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForUInt16Id = new Mapper(typeof(ushort), () => XmlConvert.ToUInt16(ReadStringValue()));
            mapperForUInt32Id = new Mapper(typeof(uint), () => XmlConvert.ToUInt32(ReadStringValue()));
            mapperForUInt64Id = new Mapper(typeof(ulong), () => XmlConvert.ToUInt64(ReadStringValue()));
            mapperForXmlNameId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForXmlNmTokenId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForXmlNmTokensId = new Mapper(typeof(string), () => CollapseWhitespace(ReadStringValue()));
            mapperForXmlQualifiedNameId = new Mapper(typeof(XmlQualifiedName), ReadXmlQualifiedName);

            rawTypeMappers = new List<KeyValuePair<XmlQualifiedName, Mapper>>
            {
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.AnyUriId, Environment.Schemas.NamespaceId), mapperForAnyUriId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.AnyUriId, Environment.Schemas.Namespace1999Id), mapperForAnyUriId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.AnyUriId, Environment.Schemas.Namespace2000Id), mapperForAnyUriId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.AnyUriId, Environment.Schemas.SoapNamespaceId), mapperForAnyUriId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.AnyUriId, Environment.Schemas.Soap12NamespaceId), mapperForAnyUriId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Base64Id, Environment.Schemas.SoapNamespaceId), mapperForBase64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Base64Id, Environment.Schemas.Soap12NamespaceId), mapperForBase64Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.BooleanId, Environment.Schemas.NamespaceId), mapperForBooleanId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.BooleanId, Environment.Schemas.Namespace1999Id), mapperForBooleanId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.BooleanId, Environment.Schemas.Namespace2000Id), mapperForBooleanId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.BooleanId, Environment.Schemas.SoapNamespaceId), mapperForBooleanId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.BooleanId, Environment.Schemas.Soap12NamespaceId), mapperForBooleanId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayBase64Id, Environment.Schemas.NamespaceId), mapperForByteArrayBase64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayBase64Id, Environment.Schemas.SoapNamespaceId), mapperForByteArrayBase64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayBase64Id, Environment.Schemas.Soap12NamespaceId), mapperForByteArrayBase64Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayHexId, Environment.Schemas.NamespaceId), mapperForByteArrayHexId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayHexId, Environment.Schemas.Namespace1999Id), mapperForByteArrayHexId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayHexId, Environment.Schemas.Namespace2000Id), mapperForByteArrayHexId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayHexId, Environment.Schemas.SoapNamespaceId), mapperForByteArrayHexId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteArrayHexId, Environment.Schemas.Soap12NamespaceId), mapperForByteArrayHexId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteId, Environment.Schemas.NamespaceId), mapperForByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteId, Environment.Schemas.SoapNamespaceId), mapperForByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.ByteId, Environment.Schemas.Soap12NamespaceId), mapperForByteId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.CharId, Environment.Schemas.NonXsdTypesNamespaceId), mapperForCharId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateId, Environment.Schemas.NamespaceId), mapperForDateId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateId, Environment.Schemas.Namespace1999Id), mapperForDateId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateId, Environment.Schemas.Namespace2000Id), mapperForDateId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateId, Environment.Schemas.SoapNamespaceId), mapperForDateId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateId, Environment.Schemas.Soap12NamespaceId), mapperForDateId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateTimeId, Environment.Schemas.NamespaceId), mapperForDateTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateTimeId, Environment.Schemas.SoapNamespaceId), mapperForDateTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateTimeId, Environment.Schemas.Soap12NamespaceId), mapperForDateTimeId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DateTimeOffsetId, Environment.Schemas.NonXsdTypesNamespaceId), mapperForDateTimeOffsetId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DecimalId, Environment.Schemas.NamespaceId), mapperForDecimalId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DecimalId, Environment.Schemas.Namespace1999Id), mapperForDecimalId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DecimalId, Environment.Schemas.Namespace2000Id), mapperForDecimalId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DecimalId, Environment.Schemas.SoapNamespaceId), mapperForDecimalId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DecimalId, Environment.Schemas.Soap12NamespaceId), mapperForDecimalId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DoubleId, Environment.Schemas.NamespaceId), mapperForDoubleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DoubleId, Environment.Schemas.Namespace1999Id), mapperForDoubleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DoubleId, Environment.Schemas.Namespace2000Id), mapperForDoubleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DoubleId, Environment.Schemas.SoapNamespaceId), mapperForDoubleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DoubleId, Environment.Schemas.Soap12NamespaceId), mapperForDoubleId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DurationId, Environment.Schemas.NamespaceId), mapperForDurationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DurationId, Environment.Schemas.Namespace1999Id), mapperForDurationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DurationId, Environment.Schemas.Namespace2000Id), mapperForDurationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DurationId, Environment.Schemas.SoapNamespaceId), mapperForDurationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.DurationId, Environment.Schemas.Soap12NamespaceId), mapperForDurationId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntitiesId, Environment.Schemas.NamespaceId), mapperForEntitiesId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntitiesId, Environment.Schemas.Namespace1999Id), mapperForEntitiesId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntitiesId, Environment.Schemas.Namespace2000Id), mapperForEntitiesId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntitiesId, Environment.Schemas.SoapNamespaceId), mapperForEntitiesId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntitiesId, Environment.Schemas.Soap12NamespaceId), mapperForEntitiesId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntityId, Environment.Schemas.NamespaceId), mapperForEntityId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntityId, Environment.Schemas.Namespace1999Id), mapperForEntityId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntityId, Environment.Schemas.Namespace2000Id), mapperForEntityId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntityId, Environment.Schemas.SoapNamespaceId), mapperForEntityId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.EntityId, Environment.Schemas.Soap12NamespaceId), mapperForEntityId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GDayId, Environment.Schemas.NamespaceId), mapperForGDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GDayId, Environment.Schemas.Namespace1999Id), mapperForGDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GDayId, Environment.Schemas.Namespace2000Id), mapperForGDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GDayId, Environment.Schemas.SoapNamespaceId), mapperForGDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GDayId, Environment.Schemas.Soap12NamespaceId), mapperForGDayId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthDayId, Environment.Schemas.NamespaceId), mapperForGMonthDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthDayId, Environment.Schemas.Namespace1999Id), mapperForGMonthDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthDayId, Environment.Schemas.Namespace2000Id), mapperForGMonthDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthDayId, Environment.Schemas.SoapNamespaceId), mapperForGMonthDayId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthDayId, Environment.Schemas.Soap12NamespaceId), mapperForGMonthDayId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthId, Environment.Schemas.NamespaceId), mapperForGMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthId, Environment.Schemas.Namespace1999Id), mapperForGMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthId, Environment.Schemas.Namespace2000Id), mapperForGMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthId, Environment.Schemas.SoapNamespaceId), mapperForGMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GMonthId, Environment.Schemas.Soap12NamespaceId), mapperForGMonthId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GuidId, Environment.Schemas.NonXsdTypesNamespaceId), mapperForGuidId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearId, Environment.Schemas.NamespaceId), mapperForGYearId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearId, Environment.Schemas.Namespace1999Id), mapperForGYearId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearId, Environment.Schemas.Namespace2000Id), mapperForGYearId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearId, Environment.Schemas.SoapNamespaceId), mapperForGYearId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearId, Environment.Schemas.Soap12NamespaceId), mapperForGYearId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearMonthId, Environment.Schemas.NamespaceId), mapperForGYearMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearMonthId, Environment.Schemas.Namespace1999Id), mapperForGYearMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearMonthId, Environment.Schemas.Namespace2000Id), mapperForGYearMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearMonthId, Environment.Schemas.SoapNamespaceId), mapperForGYearMonthId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.GYearMonthId, Environment.Schemas.Soap12NamespaceId), mapperForGYearMonthId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdId, Environment.Schemas.NamespaceId), mapperForIdId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdId, Environment.Schemas.Namespace1999Id), mapperForIdId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdId, Environment.Schemas.Namespace2000Id), mapperForIdId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdId, Environment.Schemas.SoapNamespaceId), mapperForIdId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdId, Environment.Schemas.Soap12NamespaceId), mapperForIdId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefId, Environment.Schemas.NamespaceId), mapperForIdRefId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefId, Environment.Schemas.Namespace1999Id), mapperForIdRefId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefId, Environment.Schemas.Namespace2000Id), mapperForIdRefId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefId, Environment.Schemas.SoapNamespaceId), mapperForIdRefId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefId, Environment.Schemas.Soap12NamespaceId), mapperForIdRefId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefsId, Environment.Schemas.NamespaceId), mapperForIdRefsId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefsId, Environment.Schemas.Namespace1999Id), mapperForIdRefsId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefsId, Environment.Schemas.Namespace2000Id), mapperForIdRefsId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefsId, Environment.Schemas.SoapNamespaceId), mapperForIdRefsId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IdRefsId, Environment.Schemas.Soap12NamespaceId), mapperForIdRefsId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int16Id, Environment.Schemas.NamespaceId), mapperForInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int16Id, Environment.Schemas.Namespace1999Id), mapperForInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int16Id, Environment.Schemas.Namespace2000Id), mapperForInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int16Id, Environment.Schemas.SoapNamespaceId), mapperForInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int16Id, Environment.Schemas.Soap12NamespaceId), mapperForInt16Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int32Id, Environment.Schemas.NamespaceId), mapperForInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int32Id, Environment.Schemas.Namespace1999Id), mapperForInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int32Id, Environment.Schemas.Namespace2000Id), mapperForInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int32Id, Environment.Schemas.SoapNamespaceId), mapperForInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int32Id, Environment.Schemas.Soap12NamespaceId), mapperForInt32Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int64Id, Environment.Schemas.NamespaceId), mapperForInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int64Id, Environment.Schemas.Namespace1999Id), mapperForInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int64Id, Environment.Schemas.Namespace2000Id), mapperForInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int64Id, Environment.Schemas.SoapNamespaceId), mapperForInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.Int64Id, Environment.Schemas.Soap12NamespaceId), mapperForInt64Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IntegerId, Environment.Schemas.NamespaceId), mapperForIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IntegerId, Environment.Schemas.Namespace1999Id), mapperForIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IntegerId, Environment.Schemas.Namespace2000Id), mapperForIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IntegerId, Environment.Schemas.SoapNamespaceId), mapperForIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.IntegerId, Environment.Schemas.Soap12NamespaceId), mapperForIntegerId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.LanguageId, Environment.Schemas.NamespaceId), mapperForLanguageId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.LanguageId, Environment.Schemas.Namespace1999Id), mapperForLanguageId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.LanguageId, Environment.Schemas.Namespace2000Id), mapperForLanguageId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.LanguageId, Environment.Schemas.SoapNamespaceId), mapperForLanguageId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.LanguageId, Environment.Schemas.Soap12NamespaceId), mapperForLanguageId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NegativeIntegerId, Environment.Schemas.NamespaceId), mapperForNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NegativeIntegerId, Environment.Schemas.Namespace1999Id), mapperForNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NegativeIntegerId, Environment.Schemas.Namespace2000Id), mapperForNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NegativeIntegerId, Environment.Schemas.SoapNamespaceId), mapperForNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NegativeIntegerId, Environment.Schemas.Soap12NamespaceId), mapperForNegativeIntegerId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NoncolonizedNameId, Environment.Schemas.NamespaceId), mapperForNoncolonizedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NoncolonizedNameId, Environment.Schemas.Namespace1999Id), mapperForNoncolonizedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NoncolonizedNameId, Environment.Schemas.Namespace2000Id), mapperForNoncolonizedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NoncolonizedNameId, Environment.Schemas.SoapNamespaceId), mapperForNoncolonizedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NoncolonizedNameId, Environment.Schemas.Soap12NamespaceId), mapperForNoncolonizedNameId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonNegativeIntegerId, Environment.Schemas.NamespaceId), mapperForNonNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonNegativeIntegerId, Environment.Schemas.Namespace1999Id), mapperForNonNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonNegativeIntegerId, Environment.Schemas.Namespace2000Id), mapperForNonNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonNegativeIntegerId, Environment.Schemas.SoapNamespaceId), mapperForNonNegativeIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonNegativeIntegerId, Environment.Schemas.Soap12NamespaceId), mapperForNonNegativeIntegerId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonPositiveIntegerId, Environment.Schemas.NamespaceId), mapperForNonPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonPositiveIntegerId, Environment.Schemas.Namespace1999Id), mapperForNonPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonPositiveIntegerId, Environment.Schemas.Namespace2000Id), mapperForNonPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonPositiveIntegerId, Environment.Schemas.SoapNamespaceId), mapperForNonPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NonPositiveIntegerId, Environment.Schemas.Soap12NamespaceId), mapperForNonPositiveIntegerId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NormalizedStringId, Environment.Schemas.NamespaceId), mapperForNormalizedStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NormalizedStringId, Environment.Schemas.Namespace1999Id), mapperForNormalizedStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NormalizedStringId, Environment.Schemas.Namespace2000Id), mapperForNormalizedStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NormalizedStringId, Environment.Schemas.SoapNamespaceId), mapperForNormalizedStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NormalizedStringId, Environment.Schemas.Soap12NamespaceId), mapperForNormalizedStringId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NotationId, Environment.Schemas.NamespaceId), mapperForNotationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NotationId, Environment.Schemas.Namespace1999Id), mapperForNotationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NotationId, Environment.Schemas.Namespace2000Id), mapperForNotationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NotationId, Environment.Schemas.SoapNamespaceId), mapperForNotationId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.NotationId, Environment.Schemas.Soap12NamespaceId), mapperForNotationId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.OldTimeInstantId, Environment.Schemas.Namespace1999Id), mapperForOldTimeInstantId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.OldTimeInstantId, Environment.Schemas.Namespace2000Id), mapperForOldTimeInstantId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.PositiveIntegerId, Environment.Schemas.NamespaceId), mapperForPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.PositiveIntegerId, Environment.Schemas.Namespace1999Id), mapperForPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.PositiveIntegerId, Environment.Schemas.Namespace2000Id), mapperForPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.PositiveIntegerId, Environment.Schemas.SoapNamespaceId), mapperForPositiveIntegerId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.PositiveIntegerId, Environment.Schemas.Soap12NamespaceId), mapperForPositiveIntegerId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SByteId, Environment.Schemas.NamespaceId), mapperForSByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SByteId, Environment.Schemas.Namespace1999Id), mapperForSByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SByteId, Environment.Schemas.Namespace2000Id), mapperForSByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SByteId, Environment.Schemas.SoapNamespaceId), mapperForSByteId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SByteId, Environment.Schemas.Soap12NamespaceId), mapperForSByteId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SingleId, Environment.Schemas.NamespaceId), mapperForSingleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SingleId, Environment.Schemas.Namespace1999Id), mapperForSingleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SingleId, Environment.Schemas.Namespace2000Id), mapperForSingleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SingleId, Environment.Schemas.SoapNamespaceId), mapperForSingleId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.SingleId, Environment.Schemas.Soap12NamespaceId), mapperForSingleId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.StringId, Environment.Schemas.NamespaceId), mapperForStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.StringId, Environment.Schemas.Namespace1999Id), mapperForStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.StringId, Environment.Schemas.Namespace2000Id), mapperForStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.StringId, Environment.Schemas.SoapNamespaceId), mapperForStringId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.StringId, Environment.Schemas.Soap12NamespaceId), mapperForStringId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeId, Environment.Schemas.NamespaceId), mapperForTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeId, Environment.Schemas.Namespace1999Id), mapperForTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeId, Environment.Schemas.Namespace2000Id), mapperForTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeId, Environment.Schemas.SoapNamespaceId), mapperForTimeId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeId, Environment.Schemas.Soap12NamespaceId), mapperForTimeId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TimeSpanId, Environment.Schemas.NonXsdTypesNamespaceId), mapperForTimeSpanId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TokenId, Environment.Schemas.NamespaceId), mapperForTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TokenId, Environment.Schemas.Namespace1999Id), mapperForTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TokenId, Environment.Schemas.Namespace2000Id), mapperForTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TokenId, Environment.Schemas.SoapNamespaceId), mapperForTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.TokenId, Environment.Schemas.Soap12NamespaceId), mapperForTokenId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt16Id, Environment.Schemas.NamespaceId), mapperForUInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt16Id, Environment.Schemas.Namespace1999Id), mapperForUInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt16Id, Environment.Schemas.Namespace2000Id), mapperForUInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt16Id, Environment.Schemas.SoapNamespaceId), mapperForUInt16Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt16Id, Environment.Schemas.Soap12NamespaceId), mapperForUInt16Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt32Id, Environment.Schemas.NamespaceId), mapperForUInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt32Id, Environment.Schemas.Namespace1999Id), mapperForUInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt32Id, Environment.Schemas.Namespace2000Id), mapperForUInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt32Id, Environment.Schemas.SoapNamespaceId), mapperForUInt32Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt32Id, Environment.Schemas.Soap12NamespaceId), mapperForUInt32Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt64Id, Environment.Schemas.NamespaceId), mapperForUInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt64Id, Environment.Schemas.Namespace1999Id), mapperForUInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt64Id, Environment.Schemas.Namespace2000Id), mapperForUInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt64Id, Environment.Schemas.SoapNamespaceId), mapperForUInt64Id),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.UInt64Id, Environment.Schemas.Soap12NamespaceId), mapperForUInt64Id),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNameId, Environment.Schemas.NamespaceId), mapperForXmlNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNameId, Environment.Schemas.Namespace1999Id), mapperForXmlNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNameId, Environment.Schemas.Namespace2000Id), mapperForXmlNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNameId, Environment.Schemas.SoapNamespaceId), mapperForXmlNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNameId, Environment.Schemas.Soap12NamespaceId), mapperForXmlNameId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokenId, Environment.Schemas.NamespaceId), mapperForXmlNmTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokenId, Environment.Schemas.Namespace1999Id), mapperForXmlNmTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokenId, Environment.Schemas.Namespace2000Id), mapperForXmlNmTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokenId, Environment.Schemas.SoapNamespaceId), mapperForXmlNmTokenId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokenId, Environment.Schemas.Soap12NamespaceId), mapperForXmlNmTokenId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokensId, Environment.Schemas.NamespaceId), mapperForXmlNmTokensId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokensId, Environment.Schemas.Namespace1999Id), mapperForXmlNmTokensId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokensId, Environment.Schemas.Namespace2000Id), mapperForXmlNmTokensId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokensId, Environment.Schemas.SoapNamespaceId), mapperForXmlNmTokensId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlNmTokensId, Environment.Schemas.Soap12NamespaceId), mapperForXmlNmTokensId),

                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlQualifiedNameId, Environment.Schemas.NamespaceId), mapperForXmlQualifiedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlQualifiedNameId, Environment.Schemas.Namespace1999Id), mapperForXmlQualifiedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlQualifiedNameId, Environment.Schemas.Namespace2000Id), mapperForXmlQualifiedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlQualifiedNameId, Environment.Schemas.SoapNamespaceId), mapperForXmlQualifiedNameId),
                new KeyValuePair<XmlQualifiedName, Mapper>(new XmlQualifiedName(Environment.Types.PrimitiveTypes.XmlQualifiedNameId, Environment.Schemas.Soap12NamespaceId), mapperForXmlQualifiedNameId),
            };

            returnValue = FrozenDictionary.ToFrozenDictionary(rawTypeMappers, new XmlQualifiedNameComparer());

            return returnValue;
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
            object? value;
            if (!IsPrimitiveNamespace(type.Namespace) || type.Name == _urTypeID)
            {
                return ReadXmlNodes(elementCanBeType);
            }

            if (TypeMappers.TryGetValue(type, out Mapper? mapper))
            {
                value = mapper.ReadPrimitiveValue();
            }
            else
            {
                value = ReadXmlNodes(elementCanBeType);
            }

            return value;
        }

        protected object? ReadTypedNull(XmlQualifiedName type)
        {
            object? value;
            if (!IsPrimitiveNamespace(type.Namespace) || type.Name == _urTypeID)
            {
                return null;
            }

            if (type.Namespace == Environment.Schemas.NamespaceId || type.Namespace == Environment.Schemas.SoapNamespaceId || type.Namespace == Environment.Schemas.Soap12NamespaceId)
            {
                if (type.Name == Environment.Types.PrimitiveTypes.StringId ||
                    type.Name == Environment.Types.PrimitiveTypes.AnyUriId ||
                    type.Name == Environment.Types.PrimitiveTypes.DurationId ||
                    type.Name == Environment.Types.PrimitiveTypes.EntityId ||
                    type.Name == Environment.Types.PrimitiveTypes.EntitiesId ||
                    type.Name == Environment.Types.PrimitiveTypes.GDayId ||
                    type.Name == Environment.Types.PrimitiveTypes.GMonthId ||
                    type.Name == Environment.Types.PrimitiveTypes.GMonthDayId ||
                    type.Name == Environment.Types.PrimitiveTypes.GYearId ||
                    type.Name == Environment.Types.PrimitiveTypes.GYearMonthId ||
                    type.Name == Environment.Types.PrimitiveTypes.IdId ||
                    type.Name == Environment.Types.PrimitiveTypes.IdRefId ||
                    type.Name == Environment.Types.PrimitiveTypes.IdRefsId ||
                    type.Name == Environment.Types.PrimitiveTypes.IntegerId ||
                    type.Name == Environment.Types.PrimitiveTypes.LanguageId ||
                    type.Name == Environment.Types.PrimitiveTypes.XmlNameId ||
                    type.Name == Environment.Types.PrimitiveTypes.NoncolonizedNameId ||
                    type.Name == Environment.Types.PrimitiveTypes.XmlNmTokenId ||
                    type.Name == Environment.Types.PrimitiveTypes.XmlNmTokensId ||
                    type.Name == Environment.Types.PrimitiveTypes.NegativeIntegerId ||
                    type.Name == Environment.Types.PrimitiveTypes.NonPositiveIntegerId ||
                    type.Name == Environment.Types.PrimitiveTypes.NonNegativeIntegerId ||
                    type.Name == Environment.Types.PrimitiveTypes.NormalizedStringId ||
                    type.Name == Environment.Types.PrimitiveTypes.NotationId ||
                    type.Name == Environment.Types.PrimitiveTypes.PositiveIntegerId ||
                    type.Name == Environment.Types.PrimitiveTypes.TokenId)
                    value = null;
                else if (type.Name == Environment.Types.PrimitiveTypes.Int32Id)
                {
                    value = default(Nullable<int>);
                }
                else if (type.Name == Environment.Types.PrimitiveTypes.BooleanId)
                    value = default(Nullable<bool>);
                else if (type.Name == Environment.Types.PrimitiveTypes.Int16Id)
                    value = default(Nullable<short>);
                else if (type.Name == Environment.Types.PrimitiveTypes.Int64Id)
                    value = default(Nullable<long>);
                else if (type.Name == Environment.Types.PrimitiveTypes.SingleId)
                    value = default(Nullable<float>);
                else if (type.Name == Environment.Types.PrimitiveTypes.DoubleId)
                    value = default(Nullable<double>);
                else if (type.Name == Environment.Types.PrimitiveTypes.DecimalId)
                    value = default(Nullable<decimal>);
                else if (type.Name == Environment.Types.PrimitiveTypes.DateTimeId)
                    value = default(Nullable<DateTime>);
                else if (type.Name == Environment.Types.PrimitiveTypes.XmlQualifiedNameId)
                    value = null;
                else if (type.Name == Environment.Types.PrimitiveTypes.DateId)
                    value = default(Nullable<DateTime>);
                else if (type.Name == Environment.Types.PrimitiveTypes.TimeId)
                    value = default(Nullable<DateTime>);
                else if (type.Name == Environment.Types.PrimitiveTypes.ByteId)
                    value = default(Nullable<byte>);
                else if (type.Name == Environment.Types.PrimitiveTypes.SByteId)
                    value = default(Nullable<sbyte>);
                else if (type.Name == Environment.Types.PrimitiveTypes.UInt16Id)
                    value = default(Nullable<ushort>);
                else if (type.Name == Environment.Types.PrimitiveTypes.UInt32Id)
                    value = default(Nullable<uint>);
                else if (type.Name == Environment.Types.PrimitiveTypes.UInt64Id)
                    value = default(Nullable<ulong>);
                else if (type.Name == Environment.Types.PrimitiveTypes.ByteArrayHexId)
                    value = null;
                else if (type.Name == Environment.Types.PrimitiveTypes.ByteArrayBase64Id)
                    value = null;
                else if (type.Name == Environment.Types.PrimitiveTypes.Base64Id && (type.Namespace == Environment.Schemas.SoapNamespaceId || type.Namespace == Environment.Schemas.Soap12NamespaceId))
                    value = null;
                else
                    value = null;
            }
            else if (type.Namespace == Environment.Schemas.NonXsdTypesNamespaceId)
            {
                if (type.Name == Environment.Types.PrimitiveTypes.CharId)
                    value = default(Nullable<char>);
                else if (type.Name == Environment.Types.PrimitiveTypes.GuidId)
                    value = default(Nullable<Guid>);
                else if (type.Name == Environment.Types.PrimitiveTypes.TimeSpanId)
                    value = default(Nullable<TimeSpan>);
                else if (type.Name == Environment.Types.PrimitiveTypes.DateTimeOffsetId)
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
                _r.GetAttribute(Environment.Values.NilId, Environment.Schemas.Instances.NamespaceId) ??
                _r.GetAttribute(Environment.Values.NullId, Environment.Schemas.Instances.NamespaceId) ??
                _r.GetAttribute(Environment.Values.NullId, Environment.Schemas.Instances.Namespace2000Id) ??
                _r.GetAttribute(Environment.Values.NullId, Environment.Schemas.Instances.Namespace1999Id);

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
            string? arrayType = _r.GetAttribute(_arrayTypeID, Environment.Schemas.SoapNamespaceId);
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
                string? itemType = _r.GetAttribute(_itemTypeID, Environment.Schemas.Soap12NamespaceId);
                string? arraySize = _r.GetAttribute(_arraySizeID, Environment.Schemas.Soap12NamespaceId);
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
                string? arrayType = _r.GetAttribute(_arrayTypeID, Environment.Schemas.SoapNamespaceId);
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
            XmlQualifiedName urTypeName = new XmlQualifiedName(_urTypeID, Environment.Schemas.NamespaceId);
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
                        if (_r.NamespaceURI == Environment.Schemas.SoapNamespaceId)
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

            if (ReadReference(out fixupReference))
            {
                return null;
            }

            if (ReadNull())
            {
                return null;
            }

            string? id = _soap12 ? _r.GetAttribute("id", Soap12.Encoding) : _r.GetAttribute("id", null);

            if ((o = ReadArray(name, ns)) == null)
            {
                XmlQualifiedName? typeId = GetXsiType();
                if (typeId == null)
                {
                    if (name == null)
                    {
                        typeId = new XmlQualifiedName(_r.NameTable.Add(_r.LocalName), _r.NameTable.Add(_r.NamespaceURI));
                    }
                    else
                    {
                        typeId = new XmlQualifiedName(_r.NameTable.Add(name), _r.NameTable.Add(ns!));
                    }
                }

                XmlSerializationReadCallback? callback = (XmlSerializationReadCallback?)_callbacks[typeId];

                if (callback != null)
                {
                    o = callback();
                }
                else
                {
                    o = ReadTypedPrimitive(typeId, elementCanBeType);
                }
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
                     (Reader.NamespaceURI == Environment.Schemas.Instances.NamespaceId ||
                       Reader.NamespaceURI == Environment.Schemas.Instances.Namespace2000Id ||
                       Reader.NamespaceURI == Environment.Schemas.Instances.Namespace1999Id
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
                XmlAttribute xsiTypeAttribute = Document.CreateAttribute(_typeID, Environment.Schemas.Instances.NamespaceId);
                xsiTypeAttribute.Value = elemName;
                xmlNodeList.Add(xsiTypeAttribute);
            }
            if (xsiTypeName == Soap.UrType &&
                (xsiTypeNs == Environment.Schemas.NamespaceId ||
                  xsiTypeNs == Environment.Schemas.Namespace1999Id ||
                  xsiTypeNs == Environment.Schemas.Namespace2000Id
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
