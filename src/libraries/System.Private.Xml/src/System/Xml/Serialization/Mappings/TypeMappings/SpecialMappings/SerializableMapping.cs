// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Xml.Schema;

namespace System.Xml.Serialization.Mappings.TypeMappings.SpecialMappings
{
    internal sealed class SerializableMapping : SpecialMapping
    {
        private XmlSchema? _schema;
        private bool _needSchema = true;

        // new implementation of the IXmlSerializable
        private readonly MethodInfo? _getSchemaMethod;
        private XmlQualifiedName? _xsiType;
        private XmlSchemaType? _xsdType;
        private XmlSchemaSet? _schemas;
        private bool _any;
        private string? _namespaces;

        private SerializableMapping? _baseMapping;

        internal SerializableMapping() { }
        internal SerializableMapping(MethodInfo getSchemaMethod, bool any, string? ns)
        {
            _getSchemaMethod = getSchemaMethod;
            _any = any;
            this.Namespace = ns;
            _needSchema = getSchemaMethod != null;
        }

        internal SerializableMapping(XmlQualifiedName xsiType, XmlSchemaSet schemas)
        {
            _xsiType = xsiType;
            _schemas = schemas;
            this.TypeName = xsiType.Name;
            this.Namespace = xsiType.Namespace;
            _needSchema = false;
        }

        internal void SetBaseMapping(SerializableMapping? mapping)
        {
            _baseMapping = mapping;
            if (_baseMapping != null)
            {
                NextDerivedMapping = _baseMapping.DerivedMappings;
                _baseMapping.DerivedMappings = this;
                if (this == NextDerivedMapping)
                {
                    throw new InvalidOperationException(SR.Format(SR.XmlCircularDerivation, TypeDesc!.FullName));
                }
            }
        }

        internal bool IsAny
        {
            get
            {
                if (_any)
                {
                    return true;
                }

                if (_getSchemaMethod == null)
                {
                    return false;
                }

                if (_needSchema && typeof(XmlSchemaType).IsAssignableFrom(_getSchemaMethod.ReturnType))
                {
                    return false;
                }

                RetrieveSerializableSchema();
                return _any;
            }
        }

        internal string NamespaceList
        {
            get
            {
                RetrieveSerializableSchema();
                if (_namespaces == null)
                {
                    if (_schemas != null)
                    {
                        StringBuilder anyNamespaces = new StringBuilder();
                        foreach (XmlSchema s in _schemas.Schemas())
                        {
                            if (s.TargetNamespace != null && s.TargetNamespace.Length > 0)
                            {
                                if (anyNamespaces.Length > 0)
                                {
                                    anyNamespaces.Append(' ');
                                }
                                anyNamespaces.Append(s.TargetNamespace);
                            }
                        }
                        _namespaces = anyNamespaces.ToString();
                    }
                    else
                    {
                        _namespaces = string.Empty;
                    }
                }
                return _namespaces;
            }
        }

        internal SerializableMapping? DerivedMappings { get; private set; }

        internal SerializableMapping? NextDerivedMapping { get; private set; }

        internal SerializableMapping? Next { get; set; }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        internal Type? Type { get; set; }

        internal XmlSchemaSet? Schemas
        {
            get
            {
                RetrieveSerializableSchema();
                return _schemas;
            }
        }

        internal XmlSchema? Schema
        {
            get
            {
                RetrieveSerializableSchema();
                return _schema;
            }
        }

        internal XmlQualifiedName? XsiType
        {
            get
            {
                if (!_needSchema)
                {
                    return _xsiType;
                }

                if (_getSchemaMethod == null)
                {
                    return null;
                }

                if (typeof(XmlSchemaType).IsAssignableFrom(_getSchemaMethod.ReturnType))
                {
                    return null;
                }

                RetrieveSerializableSchema();
                return _xsiType;
            }
        }

        internal XmlSchemaType? XsdType
        {
            get
            {
                RetrieveSerializableSchema();
                return _xsdType;
            }
        }

        internal static void ValidationCallbackWithErrorCode(object? sender, ValidationEventArgs args)
        {
            // CONSIDER: need the real type name
            if (args.Severity == XmlSeverityType.Error)
                throw new InvalidOperationException(SR.Format(SR.XmlSerializableSchemaError, nameof(IXmlSerializable), args.Message));
        }

        internal void CheckDuplicateElement(XmlSchemaElement? element, string? elementNs)
        {
            if (element == null)
                return;

            // only check duplicate definitions for top-level element
            if (element.Parent == null || !(element.Parent is XmlSchema))
                return;

            XmlSchemaObjectTable? elements;
            if (Schema != null && Schema.TargetNamespace == elementNs)
            {
                XmlSchemas.Preprocess(Schema);
                elements = Schema.Elements;
            }
            else if (Schemas != null)
            {
                elements = Schemas.GlobalElements;
            }
            else
            {
                return;
            }
            foreach (XmlSchemaElement e in elements.Values)
            {
                if (e.Name == element.Name && e.QualifiedName.Namespace == elementNs)
                {
                    if (Match(e, element))
                        return;
                    // XmlSerializableRootDupName=Cannot reconcile schema for '{0}'. Please use [XmlRoot] attribute to change name or namepace of the top-level element to avoid duplicate element declarations: element name='{1} namespace='{2}'.
                    throw new InvalidOperationException(SR.Format(SR.XmlSerializableRootDupName, _getSchemaMethod!.DeclaringType!.FullName, e.Name, elementNs));
                }
            }
        }

        private static bool Match(XmlSchemaElement e1, XmlSchemaElement e2)
        {
            if (e1.IsNillable != e2.IsNillable)
                return false;
            if (e1.RefName != e2.RefName)
                return false;
            if (e1.SchemaType != e2.SchemaType)
                return false;
            if (e1.SchemaTypeName != e2.SchemaTypeName)
                return false;
            if (e1.MinOccurs != e2.MinOccurs)
                return false;
            if (e1.MaxOccurs != e2.MaxOccurs)
                return false;
            if (e1.IsAbstract != e2.IsAbstract)
                return false;
            if (e1.DefaultValue != e2.DefaultValue)
                return false;
            if (e1.SubstitutionGroup != e2.SubstitutionGroup)
                return false;
            return true;
        }

        private void RetrieveSerializableSchema()
        {
            if (_needSchema)
            {
                _needSchema = false;
                if (_getSchemaMethod != null)
                {
                    // get the type info
                    _schemas ??= new XmlSchemaSet();
                    object? typeInfo = _getSchemaMethod.Invoke(null, new object[] { _schemas });
                    _xsiType = XmlQualifiedName.Empty;

                    if (typeInfo != null)
                    {
                        if (typeof(XmlSchemaType).IsAssignableFrom(_getSchemaMethod.ReturnType))
                        {
                            _xsdType = (XmlSchemaType)typeInfo;
                            // check if type is named
                            _xsiType = _xsdType.QualifiedName;
                        }
                        else if (typeof(XmlQualifiedName).IsAssignableFrom(_getSchemaMethod.ReturnType))
                        {
                            _xsiType = (XmlQualifiedName)typeInfo;
                            if (_xsiType.IsEmpty)
                            {
                                throw new InvalidOperationException(SR.Format(SR.XmlGetSchemaEmptyTypeName, Type!.FullName, _getSchemaMethod.Name));
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.Format(SR.XmlGetSchemaMethodReturnType, Type!.Name, _getSchemaMethod.Name, nameof(XmlSchemaProviderAttribute), typeof(XmlQualifiedName).FullName));
                        }
                    }
                    else
                    {
                        _any = true;
                    }

                    // make sure that user-specified schemas are valid
                    _schemas.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackWithErrorCode);
                    _schemas.Compile();

                    // at this point we verified that the information returned by the IXmlSerializable is valid
                    // Now check to see if the type was referenced before:
                    // UNDONE check for the duplcate types
                    if (!_xsiType.IsEmpty)
                    {
                        // try to find the type in the schemas collection
                        if (_xsiType.Namespace != XmlSchema.Namespace)
                        {
                            ArrayList srcSchemas = (ArrayList)_schemas.Schemas(_xsiType.Namespace);

                            if (srcSchemas.Count == 0)
                            {
                                throw new InvalidOperationException(SR.Format(SR.XmlMissingSchema, _xsiType.Namespace));
                            }
                            if (srcSchemas.Count > 1)
                            {
                                throw new InvalidOperationException(SR.Format(SR.XmlGetSchemaInclude, _xsiType.Namespace, _getSchemaMethod.DeclaringType!.FullName, _getSchemaMethod.Name));
                            }
                            XmlSchema? s = (XmlSchema?)srcSchemas[0];
                            if (s == null)
                            {
                                throw new InvalidOperationException(SR.Format(SR.XmlMissingSchema, _xsiType.Namespace));
                            }
                            _xsdType = (XmlSchemaType?)s.SchemaTypes[_xsiType];
                            if (_xsdType == null)
                            {
                                throw new InvalidOperationException(SR.Format(SR.XmlGetSchemaTypeMissing, _getSchemaMethod.DeclaringType!.FullName, _getSchemaMethod.Name, _xsiType.Name, _xsiType.Namespace));
                            }
                            _xsdType = _xsdType.Redefined ?? _xsdType;
                        }
                    }
                }
                else
                {
                    IXmlSerializable serializable = (IXmlSerializable)Activator.CreateInstance(Type!)!;
                    _schema = serializable.GetSchema();

                    if (_schema != null)
                    {
                        if (string.IsNullOrEmpty(_schema.Id)) throw new InvalidOperationException(SR.Format(SR.XmlSerializableNameMissing1, Type!.FullName));
                    }
                }
            }
        }
    }
}
