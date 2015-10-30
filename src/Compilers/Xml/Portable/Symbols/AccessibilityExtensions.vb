﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Text
Imports System.Threading
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax
Imports System.Runtime.CompilerServices
Imports Roslyn.Utilities

Namespace Microsoft.CodeAnalysis.Xml.Symbols
    Friend Module AccessibilityExtensions
        <Extension()>
        Friend Function ToDiagnosticString(a As Accessibility) As String
            Select Case a
                Case Accessibility.Public
                    Return "Public"
                Case Accessibility.Friend
                    Return "Friend"
                Case Accessibility.Private
                    Return "Private"
                Case Accessibility.Protected
                    Return "Protected"
                Case Accessibility.ProtectedOrFriend
                    Return "Protected Friend"
                Case Else
                    Throw ExceptionUtilities.UnexpectedValue(a)
            End Select
        End Function
    End Module
End Namespace