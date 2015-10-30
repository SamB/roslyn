﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml.Symbols

    ''' <summary>
    ''' Represents a compiler generated field used to implement static locals.
    ''' There are two kind of fields: the one, that holds the value, and the one, that holds initialization "flag".
    ''' </summary>
    Friend NotInheritable Class SynthesizedStaticLocalBackingField
        Inherits SynthesizedFieldSymbol

        Public ReadOnly IsValueField As Boolean

        Private ReadOnly _reportErrorForLongNames As Boolean

        Public Sub New(
            implicitlyDefinedBy As LocalSymbol,
            isValueField As Boolean,
            reportErrorForLongNames As Boolean
        )
            MyBase.New(implicitlyDefinedBy.ContainingType,
                       implicitlyDefinedBy,
                       If(isValueField,
                          implicitlyDefinedBy.Type,
                          implicitlyDefinedBy.DeclaringCompilation.GetWellKnownType(WellKnownType.Microsoft_VisualBasic_CompilerServices_StaticLocalInitFlag)),
                       If(isValueField, implicitlyDefinedBy.Name, implicitlyDefinedBy.Name & "$Init"),
                       isShared:=implicitlyDefinedBy.ContainingSymbol.IsShared,
                       isSpecialNameAndRuntimeSpecial:=True)

            Debug.Assert(implicitlyDefinedBy.IsStatic)

            Me.IsValueField = isValueField
            Me._reportErrorForLongNames = reportErrorForLongNames
        End Sub

        Friend Overloads ReadOnly Property ImplicitlyDefinedBy As LocalSymbol
            Get
                Return DirectCast(_implicitlyDefinedBy, LocalSymbol)
            End Get
        End Property

        Friend Overrides ReadOnly Property HasRuntimeSpecialName As Boolean
            Get
                Return False
            End Get
        End Property

        Friend Overrides Sub AddSynthesizedAttributes(compilationState as ModuleCompilationState, ByRef attributes As ArrayBuilder(Of SynthesizedAttributeData))
            ' no attributes on static backing fields - Dev12 behavior
        End Sub
    End Class

End Namespace