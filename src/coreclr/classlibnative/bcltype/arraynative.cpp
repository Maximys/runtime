// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// File: ArrayNative.cpp
//

//
// This file contains the native methods that support the Array class
//


#include "common.h"
#include "arraynative.h"
#include "excep.h"
#include "field.h"
#include "invokeutil.h"

#include "arraynative.inl"

// Returns a bool to indicate if the array is of primitive types or not.
FCIMPL1(INT32, ArrayNative::GetCorElementTypeOfElementType, ArrayBase* arrayUNSAFE)
{
    FCALL_CONTRACT;

    _ASSERTE(arrayUNSAFE != NULL);

    return arrayUNSAFE->GetArrayElementTypeHandle().GetVerifierCorElementType();
}
FCIMPLEND

extern "C" PCODE QCALLTYPE Array_GetElementConstructorEntrypoint(QCall::TypeHandle pArrayTypeHnd)
{
    QCALL_CONTRACT;

    PCODE ctorEntrypoint = (PCODE)NULL;

    BEGIN_QCALL;

    TypeHandle th = pArrayTypeHnd.AsTypeHandle();
    MethodTable* pElemMT = th.GetArrayElementTypeHandle().AsMethodTable();
    ctorEntrypoint = pElemMT->GetDefaultConstructor()->GetMultiCallableAddrOfCode();

    pElemMT->EnsureInstanceActive();

    END_QCALL;

    return ctorEntrypoint;
}

//
// This is a GC safe variant of the memmove intrinsic. It sets the cards, and guarantees that the object references in the GC heap are
// updated atomically.
//
// The CRT version of memmove does not always guarantee that updates of aligned fields stay atomic (e.g. it is using "rep movsb" in some cases).
// Type safety guarantees and background GC scanning requires object references in GC heap to be updated atomically.
//
void memmoveGCRefs(void *dest, const void *src, size_t len)
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    _ASSERTE(dest != nullptr);
    _ASSERTE(src != nullptr);

    // Make sure everything is pointer aligned
    _ASSERTE(IS_ALIGNED(dest, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(src, sizeof(SIZE_T)));
    _ASSERTE(IS_ALIGNED(len, sizeof(SIZE_T)));

    _ASSERTE(CheckPointer(dest));
    _ASSERTE(CheckPointer(src));

    if (len != 0 && dest != src)
    {
        InlinedMemmoveGCRefsHelper(dest, src, len);
    }
}


// Check we're allowed to create an array with the given element type.
static void CheckElementType(TypeHandle elementType)
{
    // Check for simple types first.
    if (!elementType.IsTypeDesc())
    {
        MethodTable *pMT = elementType.AsMethodTable();

        // Check for byref-like types.
        if (pMT->IsByRefLike())
            COMPlusThrow(kNotSupportedException, W("NotSupported_ByRefLikeArray"));

        // Check for open generic types.
        if (pMT->ContainsGenericVariables())
            COMPlusThrow(kNotSupportedException, W("NotSupported_OpenType"));

        // Check for Void.
        if (elementType.GetSignatureCorElementType() == ELEMENT_TYPE_VOID)
            COMPlusThrow(kNotSupportedException, W("NotSupported_VoidArray"));
    }
    else
    {
        // ByRefs and generic type variables are never allowed.
        if (elementType.IsByRef() || elementType.IsGenericVariable())
            COMPlusThrow(kNotSupportedException, W("NotSupported_Type"));
    }
}

extern "C" void QCALLTYPE Array_CreateInstance(QCall::TypeHandle pTypeHnd, INT32 rank, INT32* pLengths, INT32* pLowerBounds, BOOL createFromArrayType, QCall::ObjectHandleOnStack retArray)
{
    CONTRACTL {
        QCALL_CHECK;
        PRECONDITION(rank > 0);
        PRECONDITION(CheckPointer(pLengths));
        PRECONDITION(CheckPointer(pLowerBounds, NULL_OK));
    } CONTRACTL_END;

    BEGIN_QCALL;

    TypeHandle typeHnd = pTypeHnd.AsTypeHandle();

    if (createFromArrayType)
    {
        _ASSERTE((INT32)typeHnd.GetRank() == rank);
        _ASSERTE(typeHnd.IsArray());

        if (typeHnd.GetArrayElementTypeHandle().ContainsGenericVariables())
            COMPlusThrow(kNotSupportedException, W("NotSupported_OpenType"));

        if (!typeHnd.AsMethodTable()->IsMultiDimArray())
        {
            _ASSERTE(pLowerBounds == NULL || pLowerBounds[0] == 0);

            GCX_COOP();
            retArray.Set(AllocateSzArray(typeHnd, pLengths[0]));
            goto Done;
        }
    }
    else
    {
        CheckElementType(typeHnd);

        // Is it ELEMENT_TYPE_SZARRAY array?
        if (rank == 1 && (pLowerBounds == NULL || pLowerBounds[0] == 0))
        {
            CorElementType corType = typeHnd.GetSignatureCorElementType();

            // Shortcut for common cases
            if (CorTypeInfo::IsPrimitiveType(corType))
            {
                GCX_COOP();
                retArray.Set(AllocatePrimitiveArray(corType, pLengths[0]));
                goto Done;
            }

            typeHnd = ClassLoader::LoadArrayTypeThrowing(typeHnd);

            {
                GCX_COOP();
                retArray.Set(AllocateSzArray(typeHnd, pLengths[0]));
                goto Done;
            }
        }

        // Find the Array class...
        typeHnd = ClassLoader::LoadArrayTypeThrowing(typeHnd, ELEMENT_TYPE_ARRAY, rank);
    }

    {
        _ASSERTE(rank <= MAX_RANK); // Ensures that the stack buffer size allocations below won't overflow

        DWORD boundsSize = 0;
        INT32* bounds;
        if (pLowerBounds != NULL)
        {
            boundsSize = 2 * rank;
            bounds = (INT32*) _alloca(boundsSize * sizeof(INT32));

            for (int i=0;i<rank;i++) {
                bounds[2*i] = pLowerBounds[i];
                bounds[2*i+1] = pLengths[i];
            }
        }
        else
        {
            boundsSize = rank;
            bounds = (INT32*) _alloca(boundsSize * sizeof(INT32));

            // We need to create a private copy of pLengths to avoid holes caused
            // by caller mutating the array
            for (int i=0; i < rank; i++)
                bounds[i] = pLengths[i];
        }

        {
            GCX_COOP();
            retArray.Set(AllocateArrayEx(typeHnd, bounds, boundsSize));
        }
    }

Done: ;
    END_QCALL;
}

extern "C" void QCALLTYPE Array_CreateInstanceMDArray(EnregisteredTypeHandle typeHandle, UINT32 dwNumArgs, INT32* pArgList, QCall::ObjectHandleOnStack retArray)
{
    QCALL_CONTRACT;

    BEGIN_QCALL;

    GCX_COOP();

    TypeHandle typeHnd = TypeHandle::FromPtr(typeHandle);
    _ASSERTE(typeHnd.IsFullyLoaded());
    _ASSERTE(typeHnd.GetMethodTable()->IsArray());

    retArray.Set(AllocateArrayEx(typeHnd, pArgList, dwNumArgs));

    END_QCALL;
}
