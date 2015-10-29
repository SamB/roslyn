' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml.Symbols

    Friend Enum DeclarationKind As Byte
        [Namespace]
        [Class]
        [Interface]
        [Structure]
        [Enum]
        [Delegate]
        [Module]
        Script
        Submission
        ImplicitClass
        EventSyntheticDelegate
    End Enum

End Namespace
