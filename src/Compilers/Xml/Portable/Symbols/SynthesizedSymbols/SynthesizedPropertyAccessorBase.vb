' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax
Namespace Microsoft.CodeAnalysis.Xml.Symbols
    Partial Friend MustInherit Class SynthesizedPropertyAccessorBase(Of T As PropertySymbol)
        Inherits SynthesizedAccessor(Of T)

        Protected Sub New(container As NamedTypeSymbol, [property] As T)
            MyBase.New(container, [property])
        End Sub

        Friend MustOverride ReadOnly Property BackingFieldSymbol As FieldSymbol
    End Class
End Namespace
