﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundValueTypeMeReference

#If DEBUG Then
        Private Sub Validate()
            Debug.Assert(Me.Type.IsValueType)
            Debug.Assert(Not Me.Type.IsTypeParameter)
        End Sub
#End If

        Public Overrides ReadOnly Property IsLValue As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Namespace

