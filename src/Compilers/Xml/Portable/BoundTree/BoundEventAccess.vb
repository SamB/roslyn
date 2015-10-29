' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundEventAccess
        Public Overrides ReadOnly Property ExpressionSymbol As Symbol
            Get
                Return Me.EventSymbol
            End Get
        End Property
    End Class

End Namespace
