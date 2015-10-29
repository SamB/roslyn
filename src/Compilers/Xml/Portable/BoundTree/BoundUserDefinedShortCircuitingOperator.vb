' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Partial Class BoundUserDefinedShortCircuitingOperator

#If DEBUG Then
        Private Sub Validate()
            Debug.Assert(LeftTest Is Nothing OrElse LeftTest.Type.IsBooleanType())
            Debug.Assert((LeftOperand Is Nothing) = (LeftOperandPlaceholder Is Nothing))
            Debug.Assert(LeftOperand IsNot Nothing OrElse HasErrors)

            If LeftTest IsNot Nothing Then
                Debug.Assert(LeftTest.Kind = BoundKind.UserDefinedUnaryOperator OrElse
                             (LeftTest.Kind = BoundKind.NullableIsTrueOperator AndAlso
                              DirectCast(LeftTest, BoundNullableIsTrueOperator).Operand.Kind = BoundKind.UserDefinedUnaryOperator))
            End If
        End Sub
#End If

    End Class

End Namespace
