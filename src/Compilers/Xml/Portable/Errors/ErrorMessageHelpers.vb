' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml
    Friend Module ErrorMessageHelpers
        <Extension()>
        Public Function ToDisplay(access As Accessibility) As String
            Select Case access
                Case Accessibility.NotApplicable
                    Return ""
                Case Accessibility.Private
                    Return "Private"
                Case Accessibility.Protected
                    Return "Protected"
                Case Accessibility.ProtectedOrFriend
                    Return "Protected Friend"
                Case Accessibility.ProtectedAndFriend
                    Return "Friend" ' TODO: This protection level has no equivalent in the language.
                Case Accessibility.Friend
                    Return "Friend"
                Case Accessibility.Public
                    Return "Public"
                Case Else
                    Throw ExceptionUtilities.UnexpectedValue(access)
            End Select
        End Function

    End Module
End Namespace
