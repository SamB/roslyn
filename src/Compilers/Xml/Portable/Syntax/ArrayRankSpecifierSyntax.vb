﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml.Syntax

    Partial Public Class ArrayRankSpecifierSyntax

        ''' <summary>
        ''' Returns the ranks of this array rank specifier.
        ''' </summary>
        Public ReadOnly Property Rank() As Integer
            Get
                Return Me.CommaTokens.Count + 1
            End Get
        End Property

    End Class
End Namespace
