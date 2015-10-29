﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Class SyntaxTriviaFunctions
        Friend Shared ReadOnly Skipped As Func(Of SyntaxTrivia, Boolean) = Function(t) t.Kind = SyntaxKind.SkippedTokensTrivia
    End Class

End Namespace
