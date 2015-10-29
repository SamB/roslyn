' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Globalization
Imports System.Threading
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols.Metadata.PE
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml
    Friend Module PEDocumentationCommentUtils

        Friend Function GetDocumentationComment(
            symbol As Symbol,
            containingPEModule As PEModuleSymbol,
            preferredCulture As CultureInfo,
            cancellationToken As CancellationToken,
            ByRef lazyDocComment As Tuple(Of CultureInfo, String)) As String

            If lazyDocComment Is Nothing Then
                Interlocked.CompareExchange(lazyDocComment,
                    Tuple.Create(
                        preferredCulture,
                        containingPEModule.DocumentationProvider.GetDocumentationForSymbol(
                            symbol.GetDocumentationCommentId(), preferredCulture,
                            cancellationToken)),
                    Nothing)
            End If

            If Object.Equals(lazyDocComment.Item1, preferredCulture) Then
                Return lazyDocComment.Item2
            End If

            Return containingPEModule.DocumentationProvider.GetDocumentationForSymbol(
                symbol.GetDocumentationCommentId(), preferredCulture, cancellationToken)
        End Function

    End Module
End Namespace
