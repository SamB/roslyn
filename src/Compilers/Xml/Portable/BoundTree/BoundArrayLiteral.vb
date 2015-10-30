﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundArrayLiteral
        Inherits BoundExpression

        Public ReadOnly Property IsEmptyArrayLiteral As Boolean
            Get
                Return InferredType.Rank = 1 AndAlso Initializer.Initializers.Length = 0
            End Get
        End Property

    End Class

End Namespace