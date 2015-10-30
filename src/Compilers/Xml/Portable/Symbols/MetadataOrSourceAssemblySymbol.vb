﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Concurrent
Imports System.Collections.Immutable
Imports System.Threading
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml.Symbols

    ''' <summary>
    ''' Represents source or metadata assembly.
    ''' </summary>
    ''' <remarks></remarks>
    Friend MustInherit Class MetadataOrSourceAssemblySymbol
        Inherits NonMissingAssemblySymbol

        ''' <summary>
        ''' An array of cached Cor types defined in this assembly.
        ''' Lazily filled by GetSpecialType method.
        ''' </summary>
        ''' <remarks></remarks>
        Private _lazySpecialTypes() As NamedTypeSymbol

        ''' <summary>
        ''' How many Cor types have we cached so far.
        ''' </summary>
        Private _cachedSpecialTypes As Integer

        ''' <summary>
        ''' Lookup declaration for predefined CorLib type in this Assembly. Only should be
        ''' called if it is know that this is the Cor Library (mscorlib).
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Overrides Function GetDeclaredSpecialType(type As SpecialType) As NamedTypeSymbol
#If DEBUG Then
            For Each [module] In Me.Modules
                Debug.Assert([module].GetReferencedAssemblies().Length = 0)
            Next
#End If

            If _lazySpecialTypes Is Nothing OrElse _lazySpecialTypes(type) Is Nothing Then

                Dim emittedName As MetadataTypeName = MetadataTypeName.FromFullName(SpecialTypes.GetMetadataName(type), useCLSCompliantNameArityEncoding:=True)

                Dim [module] As ModuleSymbol = Me.Modules(0)
                Dim result As NamedTypeSymbol = [module].LookupTopLevelMetadataType(emittedName)
                If result.TypeKind <> TypeKind.Error AndAlso result.DeclaredAccessibility <> Accessibility.Public Then
                    result = New MissingMetadataTypeSymbol.TopLevel([module], emittedName, type)
                End If
                RegisterDeclaredSpecialType(result)
            End If

            Return _lazySpecialTypes(type)

        End Function

        ''' <summary>
        ''' Register declaration of predefined CorLib type in this Assembly.
        ''' </summary>
        ''' <param name="corType"></param>
        Friend Overrides Sub RegisterDeclaredSpecialType(corType As NamedTypeSymbol)
            Dim typeId As SpecialType = corType.SpecialType
            Debug.Assert(typeId <> SpecialType.None)
            Debug.Assert(corType.ContainingAssembly Is Me)
            Debug.Assert(corType.ContainingModule.Ordinal = 0)
            Debug.Assert(Me.CorLibrary Is Me)

            If (_lazySpecialTypes Is Nothing) Then
                Interlocked.CompareExchange(_lazySpecialTypes,
                    New NamedTypeSymbol(SpecialType.Count) {}, Nothing)
            End If

            If (Interlocked.CompareExchange(_lazySpecialTypes(typeId), corType, Nothing) IsNot Nothing) Then
                Debug.Assert(corType Is _lazySpecialTypes(typeId) OrElse
                                        (corType.Kind = SymbolKind.ErrorType AndAlso
                                        _lazySpecialTypes(typeId).Kind = SymbolKind.ErrorType))
            Else
                Interlocked.Increment(_cachedSpecialTypes)
                Debug.Assert(_cachedSpecialTypes > 0 AndAlso _cachedSpecialTypes <= SpecialType.Count)
            End If
        End Sub

        ''' <summary>
        ''' Continue looking for declaration of predefined CorLib type in this Assembly
        ''' while symbols for new type declarations are constructed.
        ''' </summary>
        Friend Overrides ReadOnly Property KeepLookingForDeclaredSpecialTypes As Boolean
            Get
                Return Me.CorLibrary Is Me AndAlso _cachedSpecialTypes < SpecialType.Count
            End Get
        End Property

        Private _lazyTypeNames As ICollection(Of String)
        Private _lazyNamespaceNames As ICollection(Of String)

        Public Overrides ReadOnly Property TypeNames As ICollection(Of String)
            Get
                If _lazyTypeNames Is Nothing Then
                    Interlocked.CompareExchange(_lazyTypeNames, UnionCollection(Of String).Create(Me.Modules, Function(m) m.TypeNames), Nothing)
                End If

                Return _lazyTypeNames
            End Get
        End Property

        Public Overrides ReadOnly Property NamespaceNames As ICollection(Of String)
            Get
                If _lazyNamespaceNames Is Nothing Then
                    Interlocked.CompareExchange(_lazyNamespaceNames, UnionCollection(Of String).Create(Me.Modules, Function(m) m.NamespaceNames), Nothing)
                End If
                Return _lazyNamespaceNames
            End Get
        End Property

        ''' <summary>
        ''' Determine whether this assembly has been granted access to <paramref name="potentialGiverOfAccess"></paramref>.
        ''' Assumes that the public key has been determined. The result will be cached.
        ''' </summary>
        ''' <param name="potentialGiverOfAccess"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function MakeFinalIVTDetermination(potentialGiverOfAccess As AssemblySymbol) As IVTConclusion
            Dim result As IVTConclusion = IVTConclusion.NoRelationshipClaimed
            If AssembliesToWhichInternalAccessHasBeenDetermined.TryGetValue(potentialGiverOfAccess, result) Then
                Return result
            End If

            result = IVTConclusion.NoRelationshipClaimed

            'EDMAURER returns an empty list if there was no IVT attribute at all for the given name
            'A name w/o a key is represented by a list with an entry that is empty
            Dim publicKeys As IEnumerable(Of ImmutableArray(Of Byte)) = potentialGiverOfAccess.GetInternalsVisibleToPublicKeys(Me.Name)

            'EDMAURER look for one that works, if none work, then return the failure for the last one examined.
            For Each key In publicKeys
                If result <> IVTConclusion.Match Then
                    result = PerformIVTCheck(key, potentialGiverOfAccess.Identity)
                End If
            Next

            AssembliesToWhichInternalAccessHasBeenDetermined.TryAdd(potentialGiverOfAccess, result)
            Return result
        End Function

        'EDMAURER This is a cache mapping from assemblies which we have analyzed whether or not they grant
        'internals access to us to the conclusion reached.
        Private _assembliesToWhichInternalAccessHasBeenAnalyzed As ConcurrentDictionary(Of AssemblySymbol, IVTConclusion)

        Private ReadOnly Property AssembliesToWhichInternalAccessHasBeenDetermined As ConcurrentDictionary(Of AssemblySymbol, IVTConclusion)
            Get
                If _assembliesToWhichInternalAccessHasBeenAnalyzed Is Nothing Then
                    Interlocked.CompareExchange(_assembliesToWhichInternalAccessHasBeenAnalyzed, New ConcurrentDictionary(Of AssemblySymbol, IVTConclusion), Nothing)
                End If
                Return _assembliesToWhichInternalAccessHasBeenAnalyzed
            End Get
        End Property

    End Class
End Namespace
