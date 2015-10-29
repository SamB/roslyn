' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundCollectionInitializerExpression

#If DEBUG Then
        Private Sub Validate()
            If Not Me.HasErrors Then
                For Each initializer In Me.Initializers
                    Debug.Assert(initializer.Kind = BoundKind.Call)
                Next
            End If
        End Sub
#End If

    End Class

End Namespace
