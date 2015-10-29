' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.Cci
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Xml.Symbols
Imports Microsoft.CodeAnalysis.Xml.Syntax

Namespace Microsoft.CodeAnalysis.Xml

    Friend Module StateMachineStates
        Public ReadOnly FinishedStateMachine As Integer = -2
        Public ReadOnly NotStartedStateMachine As Integer = -1
        Public ReadOnly FirstUnusedState As Integer = 0
    End Module

End Namespace
