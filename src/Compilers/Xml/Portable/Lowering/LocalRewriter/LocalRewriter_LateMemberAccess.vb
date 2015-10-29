' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax
Imports TypeKind = Microsoft.CodeAnalysis.TypeKind

Namespace Microsoft.CodeAnalysis.Xml
    Partial Friend NotInheritable Class LocalRewriter
        Public Overrides Function VisitLateMemberAccess(memberAccess As BoundLateMemberAccess) As BoundNode
            If _inExpressionLambda Then
                ' just preserve the node to report an error in ExpressionLambdaRewriter
                Return MyBase.VisitLateMemberAccess(memberAccess)
            End If

            ' standalone late member access is a LateGet.
            Dim rewrittenReceiver As BoundExpression = VisitExpressionNode(memberAccess.ReceiverOpt)
            Return LateCallOrGet(memberAccess,
                                 rewrittenReceiver,
                                 argExpressions:=Nothing,
                                 assignmentArguments:=Nothing,
                                 argNames:=Nothing,
                                 useLateCall:=memberAccess.AccessKind = LateBoundAccessKind.Call)
        End Function
    End Class
End Namespace
