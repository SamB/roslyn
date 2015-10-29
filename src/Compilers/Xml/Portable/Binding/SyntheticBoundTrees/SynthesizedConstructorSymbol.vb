' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml.Symbols

    Friend Partial Class SynthesizedConstructorSymbol
        Inherits SynthesizedConstructorBase

        Friend Overrides Function GetBoundMethodBody(diagnostics As DiagnosticBag, Optional ByRef methodBodyBinder As Binder = Nothing) As BoundBlock
            methodBodyBinder = Nothing
            Dim returnStmt = New BoundReturnStatement(Me.Syntax, Nothing, Nothing, Nothing)
            returnStmt.SetWasCompilerGenerated()
            Return New BoundBlock(Me.Syntax, Nothing, ImmutableArray(Of LocalSymbol).Empty, ImmutableArray.Create(Of BoundStatement)(returnStmt))
        End Function

    End Class

End Namespace
