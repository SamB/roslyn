' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.CodeAnalysis.Collections
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax
Imports TypeKind = Microsoft.CodeAnalysis.TypeKind

Namespace Microsoft.CodeAnalysis.Xml

    Friend Structure XmlNamespaceAndImportsClausePosition
        Public ReadOnly XmlNamespace As String
        Public ReadOnly ImportsClausePosition As Integer

        Public Sub New(xmlNamespace As String, importsClausePosition As Integer)
            Me.XmlNamespace = xmlNamespace
            Me.ImportsClausePosition = importsClausePosition
        End Sub
    End Structure
End Namespace
