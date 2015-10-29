' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax
Namespace Microsoft.CodeAnalysis.Xml.Symbols

    Friend Module SourceSymbolHelpers

        Public Function GetAsClauseLocation(identifier As SyntaxToken, asClauseOpt As AsClauseSyntax) As SyntaxNodeOrToken
            If asClauseOpt IsNot Nothing AndAlso
                (asClauseOpt.Kind <> SyntaxKind.AsNewClause OrElse
                (DirectCast(asClauseOpt, AsNewClauseSyntax).NewExpression.Kind <> SyntaxKind.AnonymousObjectCreationExpression)) Then
                Return asClauseOpt.Type
            Else
                Return identifier
            End If
        End Function

    End Module

End Namespace
