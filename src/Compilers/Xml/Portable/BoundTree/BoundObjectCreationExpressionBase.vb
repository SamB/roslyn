﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundObjectCreationExpressionBase

#If DEBUG Then
        Private Sub Validate()
            Debug.Assert(InitializerOpt Is Nothing OrElse InitializerOpt.Type = Type)
        End Sub
#End If

    End Class

End Namespace