﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System
Imports System.Collections.Immutable
Imports System.Diagnostics
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml
    Friend Structure BoundTypeOrValueData
        Implements IEquatable(Of BoundTypeOrValueData)

        Private ReadOnly _valueExpression As BoundExpression
        Public ReadOnly Property ValueExpression As BoundExpression
            Get
                Return Me._valueExpression
            End Get
        End Property

        Private ReadOnly _valueDiagnostics As DiagnosticBag
        Public ReadOnly Property ValueDiagnostics As DiagnosticBag
            Get
                Return Me._valueDiagnostics
            End Get
        End Property

        Private ReadOnly _typeExpression As BoundExpression
        Public ReadOnly Property TypeExpression As BoundExpression
            Get
                Return Me._typeExpression
            End Get
        End Property

        Private ReadOnly _typeDiagnostics As DiagnosticBag
        Public ReadOnly Property TypeDiagnostics As DiagnosticBag
            Get
                Return Me._typeDiagnostics
            End Get
        End Property

        Public Sub New(valueExpression As BoundExpression, valueDiagnostics As DiagnosticBag, typeExpression As BoundExpression, typeDiagnostics As DiagnosticBag)
            Debug.Assert(valueExpression IsNot Nothing, "Field 'valueExpression' cannot be null (use Null=""allow"" in BoundNodes.xml to remove this check)")
            Debug.Assert(valueDiagnostics IsNot Nothing, "Field 'valueDiagnostics' cannot be null (use Null=""allow"" in BoundNodes.xml to remove this check)")
            Debug.Assert(typeExpression IsNot Nothing, "Field 'typeExpression' cannot be null (use Null=""allow"" in BoundNodes.xml to remove this check)")
            Debug.Assert(typeDiagnostics IsNot Nothing, "Field 'typeDiagnostics' cannot be null (use Null=""allow"" in BoundNodes.xml to remove this check)")

            Me._valueExpression = valueExpression
            Me._valueDiagnostics = valueDiagnostics
            Me._typeExpression = typeExpression
            Me._typeDiagnostics = typeDiagnostics
        End Sub

        ' Operator=, Operator<>, GetHashCode, and Equals are needed by the generated bound tree.

        Public Shared Operator =(a As BoundTypeOrValueData, b As BoundTypeOrValueData) As Boolean
            Return a.ValueExpression Is b.ValueExpression AndAlso
                a.ValueDiagnostics Is b.ValueDiagnostics AndAlso
                a.TypeExpression Is b.TypeExpression AndAlso
                a.TypeDiagnostics Is b.TypeDiagnostics
        End Operator

        Public Shared Operator <>(a As BoundTypeOrValueData, b As BoundTypeOrValueData) As Boolean
            Return Not (a = b)
        End Operator

        Public Overrides Function Equals(obj As Object) As Boolean
            Return TypeOf obj Is BoundTypeOrValueData AndAlso DirectCast(obj, BoundTypeOrValueData) = Me
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Hash.Combine(ValueExpression.GetHashCode(),
                Hash.Combine(ValueDiagnostics.GetHashCode(),
                Hash.Combine(TypeExpression.GetHashCode(), TypeDiagnostics.GetHashCode())))
        End Function

        Private Overloads Function Equals(b As BoundTypeOrValueData) As Boolean Implements IEquatable(Of BoundTypeOrValueData).Equals
            Return b = Me
        End Function
    End Structure
End Namespace
