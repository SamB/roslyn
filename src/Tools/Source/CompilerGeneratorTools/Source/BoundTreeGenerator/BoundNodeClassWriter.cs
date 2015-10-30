﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BoundTreeGenerator
{
    internal enum TargetLanguage
    {
        VB,
        CSharp
    }

    internal enum NullHandling
    {
        Allow,
        Disallow,
        Always,
        NotApplicable // for value types
    }

    internal sealed class BoundNodeClassWriter
    {
        private readonly TextWriter _writer;
        private readonly Tree _tree;
        private readonly Dictionary<string, string> _typeMap;
        private Dictionary<string, bool> _valueTypes;
        private readonly TargetLanguage _targetLang;
        private readonly string _nameSpace;

        public string NameSpace
        {
            get {
                if (_nameSpace != null)
                    return _nameSpace;
                switch (_targetLang)
                {
                    case TargetLanguage.VB:
                        return "Microsoft.CodeAnalysis.VisualBasic";
                    case TargetLanguage.CSharp:
                        return "Microsoft.CodeAnalysis.CSharp";
                    default:
                        throw new ApplicationException("Unexpected target language");
                }
            }
        }

        private BoundNodeClassWriter(TextWriter writer, Tree tree, TargetLanguage targetLang)
        {
            _writer = writer;
            _tree = tree;
            _targetLang = targetLang;
            _nameSpace = tree.NameSpace;
            _typeMap = tree.Types.Where(t => !(t is EnumType || t is ValueType)).ToDictionary(n => n.Name, n => n.Base);
            _typeMap.Add(tree.Root, null);

            InitializeValueTypes();
        }

        private void InitializeValueTypes()
        {
            _valueTypes = new Dictionary<string, bool>();
            foreach (ValueType t in _tree.Types.Where(t => t is ValueType))
                _valueTypes.Add(t.Name, true);

            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    _valueTypes.Add("bool", true);
                    _valueTypes.Add("int", true);
                    _valueTypes.Add("uint", true);
                    _valueTypes.Add("short", true);
                    _valueTypes.Add("ushort", true);
                    _valueTypes.Add("long", true);
                    _valueTypes.Add("ulong", true);
                    _valueTypes.Add("byte", true);
                    _valueTypes.Add("sbyte", true);
                    _valueTypes.Add("char", true);
                    _valueTypes.Add("Boolean", true);
                    break;

                case TargetLanguage.VB:
                    _valueTypes.Add("Boolean", true);
                    _valueTypes.Add("Integer", true);
                    _valueTypes.Add("UInteger", true);
                    _valueTypes.Add("Short", true);
                    _valueTypes.Add("UShort", true);
                    _valueTypes.Add("Long", true);
                    _valueTypes.Add("ULong", true);
                    _valueTypes.Add("Byte", true);
                    _valueTypes.Add("SByte", true);
                    _valueTypes.Add("Char", true);
                    break;
            }

            _valueTypes.Add("Int8", true);
            _valueTypes.Add("Int16", true);
            _valueTypes.Add("Int32", true);
            _valueTypes.Add("Int64", true);
            _valueTypes.Add("UInt8", true);
            _valueTypes.Add("UInt16", true);
            _valueTypes.Add("UInt32", true);
            _valueTypes.Add("UInt64", true);
            _valueTypes.Add("ImmutableArray", true);
            _valueTypes.Add("PropertyAccessKind", true);
        }

        public static void Write(TextWriter writer, Tree tree, TargetLanguage targetLang)
        {
            new BoundNodeClassWriter(writer, tree, targetLang).WriteFile();
        }

        private int _indent;
        private bool _needsIndent = true;

        private void Write(string format, params object[] args)
        {
            if (_needsIndent)
            {
                _writer.Write(new string(' ', _indent * 4));
                _needsIndent = false;
            }
            _writer.Write(format, args);
        }

        private void WriteLine(string format, params object[] args)
        {
            Write(format, args);
            _writer.WriteLine();
            _needsIndent = true;
        }

        private void Blank()
        {
            _writer.WriteLine();
            _needsIndent = true;
        }

        private void Brace()
        {
            WriteLine("{{");
            Indent();
        }

        private void Unbrace()
        {
            Outdent();
            WriteLine("}}");
        }

        private void Indent()
        {
            ++_indent;
        }

        private void Outdent()
        {
            --_indent;
        }

        private void WriteFile()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    WriteLine("// <auto-generated />"); break;
                case TargetLanguage.VB:
                    WriteLine("' <auto-generated />"); break;
                default:
                    throw new ApplicationException("Unexpected target language");
            }

            Blank();
            WriteUsing("System");
            WriteUsing("System.Collections");
            WriteUsing("System.Collections.Generic");
            WriteUsing("System.Collections.Immutable");
            WriteUsing("System.Diagnostics");
            WriteUsing("System.Linq");
            WriteUsing("System.Runtime.CompilerServices");
            WriteUsing("System.Threading");
            WriteUsing("System.Text");
            WriteUsing("Microsoft.CodeAnalysis.Collections");
            WriteUsing("Roslyn.Utilities");

            Blank();
            WriteStartNamespace();
            WriteKinds();
            WriteTypes();
            WriteVisitor();
            WriteWalker();
            WriteRewriter();
            WriteTreeDumperNodeProducer();
            WriteEndNamespace();
        }

        private void WriteUsing(string nsName)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    WriteLine("using {0};", nsName); break;
                case TargetLanguage.VB:
                    WriteLine("Imports {0}", nsName); break;
                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteStartNamespace()
        {
            WriteUsing("Microsoft.CodeAnalysis.Text");
            WriteUsing(NameSpace + ".Symbols");
            WriteUsing(NameSpace + ".Syntax");
            Blank();

            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    WriteLine("namespace " + NameSpace);
                    Brace();
                    break;
                case TargetLanguage.VB:
                    WriteLine("Namespace " + NameSpace);
                    Indent();
                    break;
                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteEndNamespace()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    Unbrace();
                    break;
                case TargetLanguage.VB:
                    Outdent();
                    WriteLine("End Namespace");
                    break;
                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteKinds()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    WriteLine("internal enum BoundKind: byte");
                    Brace();
                    foreach (var node in _tree.Types.OfType<Node>())
                        WriteLine("{0},", FixKeyword(StripBound(node.Name)));
                    Unbrace();
                    break;

                case TargetLanguage.VB:
                    WriteLine("Friend Enum BoundKind as Byte");
                    Indent();
                    foreach (var node in _tree.Types.OfType<Node>())
                        WriteLine("{0}", FixKeyword(StripBound(node.Name)));
                    Outdent();
                    WriteLine("End Enum");
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteTypes()
        {
            foreach (var node in _tree.Types.Where(n => !(n is PredefinedNode)))
            {
                Blank();
                WriteType(node);
            }
        }

        private bool CanBeSealed(TreeType node)
        {
            // Is this type the base type of anything?
            return !_typeMap.Values.Contains(node.Name);
        }

        private void WriteClassHeader(TreeType node)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        string abstr = "";
                        if (node is AbstractNode)
                            abstr = "abstract ";
                        else if (CanBeSealed(node))
                            abstr = "sealed ";
                        WriteLine("internal {2}partial class {0} : {1}", node.Name, node.Base, abstr);
                        Brace();
                        break;
                    }

                case TargetLanguage.VB:
                    {
                        string abstr = "";
                        if (node is AbstractNode)
                            abstr = "MustInherit ";
                        else if (CanBeSealed(node))
                            abstr = "NotInheritable ";
                        WriteLine("Friend {1}Partial Class {0}", node.Name, abstr);
                        Indent();
                        WriteLine("Inherits {0}", node.Base);
                        Blank();
                        break;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteClassFooter(TreeType node)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    Unbrace();
                    break;

                case TargetLanguage.VB:
                    Outdent();
                    WriteLine("End Class");
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void Paren()
        {
            Write("(");
        }

        private void UnParen()
        {
            Write(")");
        }

        private void SeparatedList<T>(string separator, IEnumerable<T> items, Func<T, string> func)
        {
            var first = true;
            foreach (T item in items)
            {
                if (!first)
                    _writer.Write(separator);
                first = false;
                _writer.Write(func(item));
            }
        }

        private void Comma<T>(IEnumerable<T> items, Func<T, string> func)
        {
            SeparatedList(", ", items, func);
        }

        private void Or<T>(IEnumerable<T> items, Func<T, string> func)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    SeparatedList(" || ", items, func);
                    break;

                case TargetLanguage.VB:
                    SeparatedList(" OrElse ", items, func);
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void ParenList<T>(IEnumerable<T> items, Func<T, string> func)
        {
            Paren();
            Comma(items, func);
            UnParen();
        }

        private void ParenList(IEnumerable<string> items)
        {
            Paren();
            Comma(items, x => x);
            UnParen();
        }

        private void WriteConstructor(TreeType node, bool isPublic, bool hasChildNodes)
        {
            if (hasChildNodes)
            {
                WriteConstructorWithHasErrors(node, isPublic, hasErrorsIsOptional: true);
            }
            else
            {
                WriteConstructorWithHasErrors(node, isPublic, hasErrorsIsOptional: false);
                WriteConstructorWithoutHasErrors(node, isPublic);
            }
        }

        private void WriteConstructorWithHasErrors(TreeType node, bool isPublic, bool hasErrorsIsOptional)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        // A public constructor does not have an explicit kind parameter.
                        Write("{0} {1}", isPublic ? "public" : "protected", node.Name);
                        IEnumerable<string> fields = isPublic ? new[] { "CSharpSyntaxNode syntax" } : new[] { "BoundKind kind", "CSharpSyntaxNode syntax" };
                        fields = fields.Concat(from field in AllSpecifiableFields(node)
                                               select field.Type + " " + ToCamelCase(field.Name));

                        if (hasErrorsIsOptional)
                            fields = fields.Concat(new[] { "bool hasErrors = false" });
                        else
                            fields = fields.Concat(new[] { "bool hasErrors" });

                        ParenList(fields, x => x);
                        Blank();
                        Indent();
                        Write(": base(");
                        if (isPublic)
                        {
                            // Base call has bound kind, syntax, all fields in base type, plus merged HasErrors.
                            Write(string.Format("BoundKind.{0}", StripBound(node.Name)));
                            Write(", syntax, ");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write("{0}, ", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "null" : ToCamelCase(baseField.Name));
                            Or((new[] { "hasErrors" })
                                .Concat(from field in AllNodeOrNodeListFields(node)
                                        select ToCamelCase(field.Name) + ".HasErrors()"), x => x);
                        }
                        else
                        {
                            // Base call has kind, syntax, and hasErrors. No merging of hasErrors because derived class already did the merge.
                            Write("kind, syntax, ");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write("{0}, ", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "null" : ToCamelCase(baseField.Name));
                            Write("hasErrors");
                        }
                        Write(")");
                        Blank();
                        Outdent();
                        Brace();

                        WriteNullChecks(node);

                        foreach (var field in Fields(node))
                        {
                            if (IsPropertyOverrides(field))
                            {
                                WriteLine("this._{0} = {1};", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "null" : ToCamelCase(field.Name));
                            }
                            else
                            {
                                WriteLine("this.{0} = {1};", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "null" : ToCamelCase(field.Name));
                            }
                        }
                        Unbrace();
                        Blank();
                        break;
                    }

                case TargetLanguage.VB:
                    {
                        // A public constructor does not have an explicit kind parameter.
                        Write("{0} {1}", isPublic ? "Public" : "Protected", "Sub New");
                        IEnumerable<string> fields = isPublic ? new[] { "syntax As VisualBasicSyntaxNode" } : new[] { "kind As BoundKind", "syntax as VisualBasicSyntaxNode" };
                        fields = fields.Concat(from field in AllSpecifiableFields(node)
                                               select ToCamelCase(field.Name) + " As " + field.Type);

                        if (hasErrorsIsOptional)
                            fields = fields.Concat(new[] { "Optional hasErrors As Boolean = False" });
                        else
                            fields = fields.Concat(new[] { "hasErrors As Boolean" });

                        ParenList(fields, x => x);
                        Blank();
                        Indent();
                        Write("MyBase.New(");
                        if (isPublic)
                        {
                            // Base call has bound kind, syntax, all fields in base type, plus merged HasErrors.
                            Write(string.Format("BoundKind.{0}", StripBound(node.Name)));
                            Write(", syntax, ");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write("{0}, ", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(baseField.Name));
                            Or((new[] { "hasErrors" })
                                .Concat(from field in AllNodeOrNodeListFields(node)
                                        select ToCamelCase(field.Name) + ".NonNullAndHasErrors()"), x => x);
                        }
                        else
                        {
                            // Base call has kind, syntax, and hasErrors. No merging of hasErrors because derived class already did the merge.
                            Write("kind, syntax, ");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write("{0}, ", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(baseField.Name));
                            Write("hasErrors");
                        }
                        Write(")");
                        Blank();

                        WriteNullChecks(node);

                        foreach (var field in Fields(node))
                            WriteLine("Me._{0} = {1}", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(field.Name));

                        bool hasValidate = HasValidate(node);

                        if (hasValidate)
                        {
                            Blank();
                            WriteLine("Validate()");
                        }

                        Outdent();
                        WriteLine("End Sub");
                        Blank();

                        if (hasValidate)
                        {
                            WriteLine("Private Partial Sub Validate()");
                            WriteLine("End Sub");
                            Blank();
                        }

                        break;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        // This constructor should only be created if no node or list fields, since it just calls base class constructor
        // without merging hasErrors.
        private void WriteConstructorWithoutHasErrors(TreeType node, bool isPublic)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        // A public constructor does not have an explicit kind parameter.
                        Write("{0} {1}", isPublic ? "public" : "protected", node.Name);
                        IEnumerable<string> fields = isPublic ? new[] { "CSharpSyntaxNode syntax" } : new[] { "BoundKind kind", "CSharpSyntaxNode syntax" };
                        fields = fields.Concat(from field in AllSpecifiableFields(node)
                                               select field.Type + " " + ToCamelCase(field.Name));
                        ParenList(fields, x => x);
                        Blank();
                        Indent();
                        Write(": base(");
                        if (isPublic)
                        {
                            // Base call has bound kind, syntax, fields.
                            Write(string.Format("BoundKind.{0}", StripBound(node.Name)));
                            Write(", syntax");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write(", {0}", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "null" : ToCamelCase(baseField.Name));
                        }
                        else
                        {
                            // Base call has kind, syntax, fields
                            Write("kind, syntax");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write(", {0}", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "null" : ToCamelCase(baseField.Name));
                        }
                        Write(")");
                        Blank();
                        Outdent();
                        Brace();

                        WriteNullChecks(node);

                        foreach (var field in Fields(node))
                        {
                            if (IsPropertyOverrides(field))
                            {
                                WriteLine("this._{0} = {1};", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "null" : ToCamelCase(field.Name));
                            }
                            else
                            {
                                WriteLine("this.{0} = {1};", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "null" : ToCamelCase(field.Name));
                            }
                        }
                        Unbrace();
                        Blank();
                        break;
                    }

                case TargetLanguage.VB:
                    {
                        // A public constructor does not have an explicit kind parameter.
                        Write("{0} {1}", isPublic ? "Public" : "Protected", "Sub New");
                        IEnumerable<string> fields = isPublic ? new[] { "syntax As VisualBasicSyntaxNode" } : new[] { "kind As BoundKind", "syntax as VisualBasicSyntaxNode" };
                        fields = fields.Concat(from field in AllSpecifiableFields(node)
                                               select ToCamelCase(field.Name) + " As " + field.Type);
                        ParenList(fields, x => x);
                        Blank();
                        Indent();
                        Write("MyBase.New(");
                        if (isPublic)
                        {
                            // Base call has bound kind, syntax, fields.
                            Write(string.Format("BoundKind.{0}", StripBound(node.Name)));
                            Write(", syntax");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write(", {0}", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(baseField.Name));
                        }
                        else
                        {
                            // Base call has kind, syntax, fields
                            Write("kind, syntax");
                            foreach (Field baseField in AllSpecifiableFields(BaseType(node)))
                                Write(", {0}", FieldNullHandling(node, baseField.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(baseField.Name));
                        }
                        Write(")");
                        Blank();

                        WriteNullChecks(node);

                        foreach (var field in Fields(node))
                            WriteLine("Me._{0} = {1}", field.Name, FieldNullHandling(node, field.Name) == NullHandling.Always ? "Nothing" : ToCamelCase(field.Name));

                        if (HasValidate(node))
                        {
                            Blank();
                            WriteLine("Validate()");
                        }

                        Outdent();
                        WriteLine("End Sub");
                        Blank();
                        break;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        // Write the null checks for any fields that can't be null.
        private void WriteNullChecks(TreeType node)
        {
            IEnumerable<Field> nullCheckFields = AllFields(node).Where(f => FieldNullHandling(node, f.Name) == NullHandling.Disallow);

            if (nullCheckFields.Any())
            {
                Blank();

                foreach (Field field in nullCheckFields)
                {
                    bool isROArray = (GetGenericType(field.Type) == "ImmutableArray");
                    switch (_targetLang)
                    {
                        case TargetLanguage.CSharp:
                            if (isROArray)
                                WriteLine("Debug.Assert(!{0}.IsDefault, \"Field '{0}' cannot be null (use Null=\\\"allow\\\" in BoundNodes.xml to remove this check)\");", ToCamelCase(field.Name));
                            else
                                WriteLine("Debug.Assert({0} != null, \"Field '{0}' cannot be null (use Null=\\\"allow\\\" in BoundNodes.xml to remove this check)\");", ToCamelCase(field.Name));
                            break;

                        case TargetLanguage.VB:
                            if (isROArray)
                                WriteLine("Debug.Assert(Not ({0}.IsDefault), \"Field '{0}' cannot be null (use Null=\"\"allow\"\" in BoundNodes.xml to remove this check)\")", ToCamelCase(field.Name));
                            else
                                WriteLine("Debug.Assert({0} IsNot Nothing, \"Field '{0}' cannot be null (use Null=\"\"allow\"\" in BoundNodes.xml to remove this check)\")", ToCamelCase(field.Name));
                            break;
                    }
                }

                Blank();
            }
        }

        private static IEnumerable<Field> Fields(TreeType node)
        {
            if (node is Node)
                return from n in ((Node)node).Fields where !n.Override select n;
            if (node is AbstractNode)
                return from n in ((AbstractNode)node).Fields where !n.Override select n;
            return Enumerable.Empty<Field>();
        }

        private static IEnumerable<Field> FieldsIncludingOverrides(TreeType node)
        {
            if (node is Node)
                return ((Node)node).Fields;
            if (node is AbstractNode)
                return ((AbstractNode)node).Fields;
            return Enumerable.Empty<Field>();
        }

        private TreeType BaseType(TreeType node)
        {
            string name = _typeMap[node.Name];
            if (name == _tree.Root)
                return null;
            return _tree.Types.Single(t => t.Name == name);
        }

        private static bool HasValidate(TreeType node)
        {
            return node.HasValidate != null && string.Compare(node.HasValidate, "true", true) == 0;
        }

        private IEnumerable<TreeType> TypeAndBaseTypes(TreeType node)
        {
            var n = node;
            while (n != null)
            {
                yield return n;
                n = BaseType(n);
            }
        }

        private IEnumerable<Field> AllFields(TreeType node)
        {
            if (node == null)
                return Enumerable.Empty<Field>();
            return from t in TypeAndBaseTypes(node)
                   from f in Fields(t)
                   select f;
        }

        // AlwaysNull fields are those that have Null="Always" specified (possibly in an override).
        private IEnumerable<Field> AllAlwaysNullFields(TreeType node)
        {
            return from f in AllFields(node) where FieldNullHandling(node, f.Name) == NullHandling.Always select f;
        }

        // Specifiable fields are those that aren't always null.
        private IEnumerable<Field> AllSpecifiableFields(TreeType node)
        {
            return from f in AllFields(node) where FieldNullHandling(node, f.Name) != NullHandling.Always select f;
        }

        private IEnumerable<Field> AllNodeOrNodeListFields(TreeType node)
        {
            return AllFields(node).Where(field => IsDerivedOrListOfDerived("BoundNode", field.Type));
        }

        private IEnumerable<Field> AllTypeFields(TreeType node)
        {
            return AllFields(node).Where(field => field.Type == "TypeSymbol");
        }

        private NullHandling FieldNullHandling(TreeType node, string fieldName)
        {
            Field f = GetField(node, fieldName);

            if (f.Null != null)
            {
                switch (f.Null.ToUpperInvariant())
                {
                    case "ALLOW":
                        return NullHandling.Allow;
                    case "DISALLOW":
                        return NullHandling.Disallow;
                    case "ALWAYS":
                        return NullHandling.Always;
                    case "NOTAPPLICABLE":
                        return NullHandling.NotApplicable;
                    case "":
                        break;
                    default:
                        throw new ApplicationException(string.Format("Unexpected value for \"Null\" attribute: {0}", f.Null));
                }
            }

            if (f.Override)
                return FieldNullHandling(BaseType(node), fieldName);
            else if (!IsValueType(f.Type) || GetGenericType(f.Type) == "ImmutableArray")
                return NullHandling.Disallow; // default is to disallow nulls.
            else
                return NullHandling.NotApplicable;   // value types can't check nulls.
        }

        private Field GetField(TreeType node, string fieldName)
        {
            var fieldsWithName = from f in FieldsIncludingOverrides(node) where f.Name == fieldName select f;
            if (fieldsWithName.Any())
                return fieldsWithName.Single();
            else if (BaseType(node) != null)
                return GetField(BaseType(node), fieldName);
            else
                throw new ApplicationException(string.Format("Field {0} not found in type {1}", fieldName, node.Name));
        }

        private void WriteField(Field field)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    Blank();
                    if (IsPropertyOverrides(field))
                    {
                        WriteLine("private readonly {0} _{1};", field.Type, field.Name);
                        WriteLine("public override {0}{1} {2} {{ get {{ return _{2};}} }}", (IsNew(field) ? "new " : ""), field.Type, field.Name);
                    }
                    else
                    {
                        WriteLine("public {0}{1} {2} {{ get; }}", (IsNew(field) ? "new " : ""), field.Type, field.Name);
                    }
                    break;

                case TargetLanguage.VB:
                    Blank();
                    WriteLine("Private {0}ReadOnly _{2} As {1}", (IsNew(field) ? "Shadows " : ""), field.Type, field.Name);
                    WriteLine("Public {0}ReadOnly Property {2} As {1}", (IsNew(field) ? "Shadows " : IsPropertyOverrides(field) ? "Overrides " : ""), field.Type, field.Name);
                    Indent();
                    WriteLine("Get");
                    Indent();
                    WriteLine("Return _{0}", field.Name);
                    Outdent();
                    WriteLine("End Get");
                    Outdent();
                    WriteLine("End Property");
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteAccept(string name)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    //Blank();
                    //WriteLine("public override R Accept<A, R>(BoundTreeVisitor<A, R> visitor, A arg)");
                    //Brace();
                    //WriteLine("return visitor.Visit{0}(this, arg);", StripBound(name));
                    //Unbrace();

                    Blank();
                    WriteLine("public override BoundNode Accept(BoundTreeVisitor visitor)");
                    Brace();
                    WriteLine("return visitor.Visit{0}(this);", StripBound(name));
                    Unbrace();
                    break;

                case TargetLanguage.VB:
                    //Blank();
                    //WriteLine("Public Overrides Function Accept(Of A, R)(visitor as BoundTreeVisitor(Of A, R), arg As A) As R");
                    //Indent();
                    //WriteLine("Return visitor.Visit{0}(Me, arg)", StripBound(name));
                    //Outdent();
                    //WriteLine("End Function");

                    Blank();
                    WriteLine("Public Overrides Function Accept(visitor as BoundTreeVisitor) As BoundNode");
                    Indent();
                    WriteLine("Return visitor.Visit{0}(Me)", StripBound(name));
                    Outdent();
                    WriteLine("End Function");

                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteType(TreeType node)
        {
            if (!(node is AbstractNode) && !(node is Node))
                return;
            WriteClassHeader(node);

            bool unsealed = !CanBeSealed(node);
            bool concrete = !(node is AbstractNode);
            bool hasChildNodes = AllNodeOrNodeListFields(node).Any();

            if (unsealed)
            {
                WriteConstructor(node, false, hasChildNodes);
            }
            if (concrete)
            {
                WriteConstructor(node, true, hasChildNodes);
            }

            foreach (var field in Fields(node))
                WriteField(field);
            if (node is Node)
            {
                WriteAccept(node.Name);
                WriteUpdateMethod(node as Node);
            }

            WriteClassFooter(node);
        }

        private void WriteUpdateMethod(Node node)
        {
            if (!AllFields(node).Any())
                return;
            bool emitNew = (!Fields(node).Any()) && !(BaseType(node) is AbstractNode);

            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        Blank();
                        Write("public{1} {0} Update", node.Name, emitNew ? " new" : "");
                        Paren();
                        Comma(AllSpecifiableFields(node), field => string.Format("{0} {1}", field.Type, ToCamelCase(field.Name)));
                        UnParen();
                        Blank();
                        Brace();
                        if (AllSpecifiableFields(node).Any())
                        {
                            Write("if ");
                            Paren();
                            Or(AllSpecifiableFields(node),
                                field => string.Format("{0} != this.{1}", ToCamelCase(field.Name), field.Name));
                            UnParen();
                            Blank();
                            Brace();
                            Write("var result = new {0}", node.Name);
                            var fields = new[] { "this.Syntax" }.Concat(AllSpecifiableFields(node).Select(f => ToCamelCase(f.Name))).Concat(new[] { "this.HasErrors" });
                            ParenList(fields);
                            WriteLine(";");
                            WriteLine("result.WasCompilerGenerated = this.WasCompilerGenerated;");
                            WriteLine("return result;");
                            Unbrace();
                        }
                        WriteLine("return this;");
                        Unbrace();
                        break;
                    }

                case TargetLanguage.VB:
                    {
                        Blank();
                        Write("Public{0} Function Update", emitNew ? " Shadows" : "");
                        Paren();
                        Comma(AllSpecifiableFields(node), field => string.Format("{1} As {0}", field.Type, ToCamelCase(field.Name)));
                        UnParen();
                        WriteLine(" As {0}", node.Name);
                        Indent();

                        if (AllSpecifiableFields(node).Any())
                        {
                            Write("If ");
                            Or(AllSpecifiableFields(node),
                                field => IsValueType(field.Type) ?
                                            string.Format("{0} <> Me.{1}", ToCamelCase(field.Name), field.Name) :
                                            string.Format("{0} IsNot Me.{1}", ToCamelCase(field.Name), field.Name));
                            WriteLine(" Then");
                            Indent();
                            Write("Dim result = New {0}", node.Name);
                            var fields = new[] { "Me.Syntax" }.Concat(AllSpecifiableFields(node).Select(f => ToCamelCase(f.Name))).Concat(new[] { "Me.HasErrors" });
                            ParenList(fields);
                            WriteLine("");
                            WriteLine("");
                            WriteLine("If Me.WasCompilerGenerated Then");
                            Indent();
                            WriteLine("result.SetWasCompilerGenerated()");
                            Outdent();
                            WriteLine("End If");
                            WriteLine("");
                            WriteLine("Return result");
                            Outdent();
                            WriteLine("End If");
                        }
                        WriteLine("Return Me");

                        Outdent();
                        WriteLine("End Function");
                        break;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private string StripBound(string name)
        {
            if (name.StartsWith("Bound", StringComparison.Ordinal))
            {
                name = name.Substring(5);
            }

            return name;
        }

        private void WriteVisitor()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:

                    Blank();
                    WriteLine("internal abstract partial class BoundTreeVisitor<A,R>");
                    Brace();

                    Blank();
                    WriteLine("[MethodImpl(MethodImplOptions.NoInlining)]");
                    WriteLine("internal R VisitInternal(BoundNode node, A arg)");
                    Brace();
                    WriteLine("switch (node.Kind)");
                    Brace();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("case BoundKind.{0}: ", FixKeyword(StripBound(node.Name)));
                        Indent();
                        WriteLine("return Visit{0}(node as {1}, arg);", StripBound(node.Name), node.Name);
                        Outdent();
                    }
                    Unbrace();
                    Blank(); // end switch
                    WriteLine("return default(R);");
                    Unbrace(); // end method
                    Unbrace(); // end class

                    //Blank();
                    //WriteLine("internal abstract partial class BoundTreeVisitor");
                    //Brace();

                    //Blank();
                    //WriteLine("[MethodImpl(MethodImplOptions.NoInlining)]");
                    //WriteLine("internal BoundNode VisitInternal(BoundNode node)");
                    //Brace();
                    //WriteLine("switch (node.Kind)");
                    //Brace();
                    //foreach (var node in tree.Types.OfType<Node>())
                    //{
                    //    WriteLine("case BoundKind.{0}: ", FixKeyword(StripBound(node.Name)));
                    //    Indent();
                    //    WriteLine("return Visit{0}(node as {1});", StripBound(node.Name), node.Name);
                    //    Outdent();
                    //}
                    //Unbrace(); // end switch
                    //Blank();
                    //WriteLine("return null;");
                    //Unbrace(); // end method
                    //Unbrace(); // end class

                    Blank();
                    WriteLine("internal abstract partial class BoundTreeVisitor<A,R>");
                    Brace();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("public virtual R Visit{0}({1} node, A arg)", StripBound(node.Name), node.Name);
                        Brace();
                        WriteLine("return this.DefaultVisit(node, arg);");
                        Unbrace();
                    }
                    Unbrace();

                    Blank();
                    WriteLine("internal abstract partial class BoundTreeVisitor");
                    Brace();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("public virtual BoundNode Visit{0}({1} node)", StripBound(node.Name), node.Name);
                        Brace();
                        WriteLine("return this.DefaultVisit(node);");
                        Unbrace();
                    }
                    Unbrace();
                    break;

                case TargetLanguage.VB:
                    Blank();
                    WriteLine("Friend MustInherit Partial Class BoundTreeVisitor(Of A,R)");
                    Indent();

                    Blank();
                    WriteLine("<MethodImpl(MethodImplOptions.NoInlining)>");
                    WriteLine("Friend Function VisitInternal(node As BoundNode, arg As A) As R");
                    Indent();
                    WriteLine("Select Case node.Kind");
                    Indent();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("Case BoundKind.{0}: ", FixKeyword(StripBound(node.Name)));
                        Indent();
                        WriteLine("Return Visit{0}(CType(node, {1}), arg)", StripBound(node.Name), node.Name);
                        Outdent();
                    }
                    Outdent();
                    WriteLine("End Select");
                    WriteLine("Return DefaultVisit(node, arg)");
                    Outdent();
                    WriteLine("End Function");

                    Blank();
                    Outdent();
                    WriteLine("End Class");

                    //Blank();
                    //WriteLine("Friend MustInherit Partial Class BoundTreeVisitor");
                    //Indent();

                    //Blank();
                    //WriteLine("<MethodImpl(MethodImplOptions.NoInlining)>");
                    //WriteLine("Friend Function VisitInternal(node As BoundNode) As BoundNode");
                    //Indent();
                    //WriteLine("Select Case node.Kind");
                    //Indent();
                    //foreach (var node in tree.Types.OfType<Node>())
                    //{
                    //    WriteLine("Case BoundKind.{0}: ", FixKeyword(StripBound(node.Name)));
                    //    Indent();
                    //    WriteLine("Return Visit{0}(CType(node, {1}))", StripBound(node.Name), node.Name);
                    //    Outdent();
                    //}
                    //Outdent();
                    //WriteLine("End Select");
                    //WriteLine("Return DefaultVisit(node)");
                    //Outdent();
                    //WriteLine("End Function");

                    //Blank();
                    //Outdent();
                    //WriteLine("End Class");

                    Blank();
                    WriteLine("Friend MustInherit Partial Class BoundTreeVisitor(Of A,R)");
                    Indent();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("Public Overridable Function Visit{0}(node As {1}, arg As A) As R", StripBound(node.Name), node.Name);
                        Indent();
                        WriteLine("Return Me.DefaultVisit(node, arg)");
                        Outdent();
                        WriteLine("End Function");
                        Blank();
                    }
                    Outdent();
                    WriteLine("End Class");

                    Blank();
                    WriteLine("Friend MustInherit Partial Class BoundTreeVisitor");
                    Indent();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("Public Overridable Function Visit{0}(node As {1}) As BoundNode", StripBound(node.Name), node.Name);
                        Indent();
                        WriteLine("Return Me.DefaultVisit(node)");
                        Outdent();
                        WriteLine("End Function");
                        Blank();
                    }
                    Outdent();
                    WriteLine("End Class");
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteWalker()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    Blank();
                    WriteLine("internal abstract partial class BoundTreeWalker: BoundTreeVisitor");
                    Brace();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("public override BoundNode Visit{0}({1} node)", StripBound(node.Name), node.Name);
                        Brace();
                        foreach (Field field in AllFields(node).Where(f => IsDerivedOrListOfDerived("BoundNode", f.Type)))
                            WriteLine("this.Visit{1}(node.{0});", field.Name, IsNodeList(field.Type) ? "List" : "");
                        WriteLine("return null;");
                        Unbrace();
                    }
                    Unbrace();


                    break;

                case TargetLanguage.VB:
                    Blank();
                    WriteLine("Friend MustInherit Partial Class BoundTreeWalker");
                    Indent();
                    WriteLine("Inherits BoundTreeVisitor");
                    Blank();

                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("Public Overrides Function Visit{0}(node as {1}) As BoundNode", StripBound(node.Name), node.Name);
                        Indent();
                        foreach (Field field in AllFields(node).Where(f => IsDerivedOrListOfDerived("BoundNode", f.Type) && !SkipInVisitor(f)))
                            WriteLine("Me.Visit{1}(node.{0})", field.Name, IsNodeList(field.Type) ? "List" : "");
                        WriteLine("Return Nothing");
                        Outdent();
                        WriteLine("End Function");
                        Blank();
                    }

                    Outdent();
                    WriteLine("End Class");


                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteTreeDumperNodeProducer()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    Blank();
                    WriteLine("internal sealed class BoundTreeDumperNodeProducer : BoundTreeVisitor<object, TreeDumperNode>");
                    Brace();
                    WriteLine("private BoundTreeDumperNodeProducer()");
                    Brace();
                    Unbrace();
                    WriteLine("public static TreeDumperNode MakeTree(BoundNode node)");
                    Brace();
                    WriteLine("return (new BoundTreeDumperNodeProducer()).Visit(node, null);");
                    Unbrace();
                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("public override TreeDumperNode Visit{0}({1} node, object arg)", StripBound(node.Name), node.Name);
                        Brace();
                        Write("return new TreeDumperNode(\"{0}\", null, ", ToCamelCase(StripBound(node.Name)));
                        var allFields = AllFields(node).ToArray();
                        if (allFields.Length > 0)
                        {
                            WriteLine("new TreeDumperNode[]");
                            Brace();
                            for (int i = 0; i < allFields.Length; ++i)
                            {
                                Field field = allFields[i];
                                if (IsDerivedType("BoundNode", field.Type))
                                    Write("new TreeDumperNode(\"{0}\", null, new TreeDumperNode[] {{ Visit(node.{1}, null) }})", ToCamelCase(field.Name), field.Name);
                                else if (IsListOfDerived("BoundNode", field.Type))
                                {
                                    if (IsImmutableArray(field.Type) && field.Name.EndsWith("Opt", StringComparison.Ordinal))
                                    {
                                        Write("new TreeDumperNode(\"{0}\", null, node.{1}.IsDefault ? SpecializedCollections.EmptyArray<TreeDumperNode>() : from x in node.{1} select Visit(x, null))", ToCamelCase(field.Name), field.Name);
                                    }
                                    else
                                    {
                                        Write("new TreeDumperNode(\"{0}\", null, from x in node.{1} select Visit(x, null))", ToCamelCase(field.Name), field.Name);
                                    }
                                }
                                else
                                    Write("new TreeDumperNode(\"{0}\", node.{1}, null)", ToCamelCase(field.Name), field.Name);

                                if (i == allFields.Length - 1)
                                    WriteLine("");
                                else
                                    WriteLine(",");
                            }
                            Unbrace();
                        }
                        else
                        {
                            WriteLine("SpecializedCollections.EmptyArray<TreeDumperNode>()");
                        }
                        WriteLine(");");
                        Unbrace();
                    }
                    Unbrace();
                    break;

                case TargetLanguage.VB:
                    Blank();
                    WriteLine("Friend NotInheritable Class BoundTreeDumperNodeProducer");
                    Indent();
                    WriteLine("Inherits BoundTreeVisitor(Of Object, TreeDumperNode)");
                    Blank();
                    WriteLine("Private Sub New()");
                    WriteLine("End Sub");
                    Blank();
                    WriteLine("Public Shared Function MakeTree(node As BoundNode) As TreeDumperNode");
                    Indent();
                    WriteLine("Return (New BoundTreeDumperNodeProducer()).Visit(node, Nothing)");
                    Outdent();
                    WriteLine("End Function");
                    Blank();

                    foreach (var node in _tree.Types.OfType<Node>())
                    {
                        WriteLine("Public Overrides Function Visit{0}(node As {1}, arg As Object) As TreeDumperNode", StripBound(node.Name), node.Name);
                        Indent();
                        Write("Return New TreeDumperNode(\"{0}\", Nothing, ", ToCamelCase(StripBound(node.Name)));
                        var allFields = AllFields(node).ToArray();
                        if (allFields.Length > 0)
                        {
                            WriteLine("New TreeDumperNode() {{");
                            Indent();
                            for (int i = 0; i < allFields.Length; ++i)
                            {
                                Field field = allFields[i];
                                if (IsDerivedType("BoundNode", field.Type))
                                    Write("New TreeDumperNode(\"{0}\", Nothing, new TreeDumperNode() {{ Visit(node.{1}, Nothing) }})", ToCamelCase(field.Name), field.Name);
                                else if (IsListOfDerived("BoundNode", field.Type))
                                    Write("New TreeDumperNode(\"{0}\", Nothing, From x In node.{1} Select Visit(x, Nothing))", ToCamelCase(field.Name), field.Name);
                                else
                                    Write("New TreeDumperNode(\"{0}\", node.{1}, Nothing)", ToCamelCase(field.Name), field.Name);

                                if (i == allFields.Length - 1)
                                    WriteLine("");
                                else
                                    WriteLine(",");
                            }
                            Outdent();
                            WriteLine("}})");
                        }
                        else
                        {
                            WriteLine("SpecializedCollections.EmptyArray(Of TreeDumperNode)())");
                        }
                        Outdent();
                        WriteLine("End Function");
                        Blank();
                    }
                    Outdent();
                    WriteLine("End Class");
                    break;

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private void WriteRewriter()
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        Blank();
                        WriteLine("internal abstract partial class BoundTreeRewriter : BoundTreeVisitor");
                        Brace();
                        foreach (var node in _tree.Types.OfType<Node>())
                        {
                            WriteLine("public override BoundNode Visit{0}({1} node)", StripBound(node.Name), node.Name);
                            Brace();
                            bool hadField = false;
                            foreach (Field field in AllNodeOrNodeListFields(node))
                            {
                                hadField = true;
                                WriteLine("{3} {0} = ({3})this.Visit{2}(node.{1});", ToCamelCase(field.Name), field.Name, IsNodeList(field.Type) ? "List" : "", field.Type);
                            }
                            foreach (Field field in AllTypeFields(node))
                            {
                                hadField = true;
                                WriteLine("TypeSymbol {0} = this.VisitType(node.{1});", ToCamelCase(field.Name), field.Name);
                            }
                            if (hadField)
                            {
                                Write("return node.Update");
                                ParenList(AllSpecifiableFields(node), field => IsDerivedOrListOfDerived("BoundNode", field.Type) || field.Type == "TypeSymbol" ? ToCamelCase(field.Name) : string.Format("node.{0}", field.Name));
                                WriteLine(";");
                            }
                            else
                            {
                                WriteLine("return node;");
                            }

                            Unbrace();
                        }
                        Unbrace();
                        break;
                    }

                case TargetLanguage.VB:
                    {
                        Blank();
                        WriteLine("Friend MustInherit Partial Class BoundTreeRewriter");
                        Indent();
                        WriteLine("Inherits BoundTreeVisitor");
                        Blank();
                        foreach (var node in _tree.Types.OfType<Node>())
                        {
                            WriteLine("Public Overrides Function Visit{0}(node As {1}) As BoundNode", StripBound(node.Name), node.Name);
                            Indent();

                            bool hadField = false;
                            foreach (Field field in AllNodeOrNodeListFields(node))
                            {
                                hadField = true;

                                if (SkipInVisitor(field))
                                    WriteLine("Dim {0} As {2} = node.{1}", ToCamelCase(field.Name), field.Name, field.Type);
                                else if (IsNodeList(field.Type))
                                    WriteLine("Dim {0} As {2} = Me.VisitList(node.{1})", ToCamelCase(field.Name), field.Name, field.Type);
                                else
                                    WriteLine("Dim {0} As {2} = DirectCast(Me.Visit(node.{1}), {2})", ToCamelCase(field.Name), field.Name, field.Type);
                            }
                            foreach (Field field in AllTypeFields(node))
                            {
                                hadField = true;
                                WriteLine("Dim {0} as TypeSymbol = Me.VisitType(node.{1})", ToCamelCase(field.Name), field.Name);
                            }

                            if (hadField)
                            {
                                Write("Return node.Update");
                                ParenList(AllSpecifiableFields(node), field => IsDerivedOrListOfDerived("BoundNode", field.Type) || field.Type == "TypeSymbol" ? ToCamelCase(field.Name) : string.Format("node.{0}", field.Name));
                                WriteLine("");
                            }
                            else
                            {
                                WriteLine("Return node");
                            }

                            Outdent();
                            WriteLine("End Function");
                            Blank();
                        }
                        Outdent();
                        WriteLine("End Class");
                        break;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private bool IsDerivedOrListOfDerived(string baseType, string derivedType)
        {
            return IsDerivedType(baseType, derivedType) || IsListOfDerived(baseType, derivedType);
        }

        private bool IsListOfDerived(string baseType, string derivedType)
        {
            return IsNodeList(derivedType) && IsDerivedType(baseType, GetElementType(derivedType));
        }

        private bool IsImmutableArray(string typeName)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    return typeName.StartsWith("ImmutableArray<", StringComparison.Ordinal);
                case TargetLanguage.VB:
                    return typeName.StartsWith("ImmutableArray(Of", StringComparison.OrdinalIgnoreCase);
                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private bool IsNodeList(string typeName)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    return typeName.StartsWith("IList<", StringComparison.Ordinal) ||
                           typeName.StartsWith("ImmutableArray<", StringComparison.Ordinal);
                case TargetLanguage.VB:
                    return typeName.StartsWith("IList(Of", StringComparison.OrdinalIgnoreCase) ||
                           typeName.StartsWith("ImmutableArray(Of", StringComparison.OrdinalIgnoreCase);
                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        public bool IsNodeOrNodeList(string typeName)
        {
            return IsNode(typeName) || IsNodeList(typeName);
        }

        private string GetGenericType(string typeName)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        if (!typeName.Contains("<"))
                            return typeName;
                        int iStart = typeName.IndexOf('<');
                        return typeName.Substring(0, iStart);
                    }

                case TargetLanguage.VB:
                    {
                        int iStart = typeName.IndexOf("(Of", StringComparison.OrdinalIgnoreCase);

                        if (iStart == -1)
                            return typeName;

                        return typeName.Substring(0, iStart);
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private string GetElementType(string typeName)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    {
                        if (!typeName.Contains("<"))
                            return string.Empty;
                        int iStart = typeName.IndexOf('<');
                        int iEnd = typeName.IndexOf('>', iStart + 1);
                        if (iEnd < iStart)
                            return string.Empty;
                        var sub = typeName.Substring(iStart + 1, iEnd - iStart - 1);
                        return sub;
                    }

                case TargetLanguage.VB:
                    {
                        int iStart = typeName.IndexOf("(Of", StringComparison.OrdinalIgnoreCase);

                        if (iStart == -1)
                            return string.Empty;

                        int iEnd = typeName.IndexOf(')', iStart + 3);
                        if (iEnd < iStart)
                            return string.Empty;
                        var sub = typeName.Substring(iStart + 3, iEnd - iStart - 3).Trim();
                        return sub;
                    }

                default:
                    throw new ApplicationException("Unexpected target language");
            }
        }

        private bool IsAnyList(string typeName)
        {
            return IsNodeList(typeName);
        }

        private bool IsValueType(string typeName)
        {
            string genericType = GetGenericType(typeName);

            if (_valueTypes.ContainsKey(genericType))
                return _valueTypes[genericType];
            else
                return false;
        }

        private bool IsDerivedType(string typeName, string derivedTypeName)
        {
            if (typeName == derivedTypeName)
                return true;
            string baseType;
            if (derivedTypeName != null && _typeMap.TryGetValue(derivedTypeName, out baseType))
            {
                return IsDerivedType(typeName, baseType);
            }
            return false;
        }

        private static bool IsRoot(Node n)
        {
            return n.Root != null && string.Compare(n.Root, "true", true) == 0;
        }

        private bool IsNode(string typeName)
        {
            return _typeMap.ContainsKey(typeName);
        }

        private static bool IsNew(Field f)
        {
            return f.New != null && string.Compare(f.New, "true", true) == 0;
        }

        private static bool IsPropertyOverrides(Field f)
        {
            return f.PropertyOverrides != null && string.Compare(f.PropertyOverrides, "true", true) == 0;
        }

        private static bool SkipInVisitor(Field f)
        {
            return f.SkipInVisitor != null && string.Compare(f.SkipInVisitor, "true", true) == 0;
        }

        private string ToCamelCase(string name)
        {
            if (char.IsUpper(name[0]))
            {
                name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
            return FixKeyword(name);
        }

        private string FixKeyword(string name)
        {
            if (IsKeyword(name))
            {
                switch (_targetLang)
                {
                    case TargetLanguage.CSharp:
                        return "@" + name;

                    case TargetLanguage.VB:
                        return "[" + name + "]";

                    default:
                        throw new ApplicationException("unexpected target language");
                }
            }

            return name;
        }

        private bool IsKeyword(string name)
        {
            switch (_targetLang)
            {
                case TargetLanguage.CSharp:
                    return name.IsCSharpKeyword();

                case TargetLanguage.VB:
                    return name.IsVBKeyword();

                default:
                    throw new ApplicationException("unexpected target language");
            }
        }
    }

    internal static class Extensions
    {
        public static bool IsCSharpKeyword(this string name)
        {
            switch (name)
            {
                case "bool":
                case "byte":
                case "sbyte":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "double":
                case "float":
                case "decimal":
                case "string":
                case "char":
                case "object":
                case "typeof":
                case "sizeof":
                case "null":
                case "true":
                case "false":
                case "if":
                case "else":
                case "while":
                case "for":
                case "foreach":
                case "do":
                case "switch":
                case "case":
                case "default":
                case "lock":
                case "try":
                case "throw":
                case "catch":
                case "finally":
                case "goto":
                case "break":
                case "continue":
                case "return":
                case "public":
                case "private":
                case "internal":
                case "protected":
                case "static":
                case "readonly":
                case "sealed":
                case "const":
                case "new":
                case "override":
                case "abstract":
                case "virtual":
                case "partial":
                case "ref":
                case "out":
                case "in":
                case "where":
                case "params":
                case "this":
                case "base":
                case "namespace":
                case "using":
                case "class":
                case "struct":
                case "interface":
                case "delegate":
                case "checked":
                case "get":
                case "set":
                case "add":
                case "remove":
                case "operator":
                case "implicit":
                case "explicit":
                case "fixed":
                case "extern":
                case "event":
                case "enum":
                case "unsafe":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsVBKeyword(this string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "addhandler":
                case "addressof":
                case "alias":
                case "and":
                case "andalso":
                case "as":
                case "boolean":
                case "byref":
                case "byte":
                case "byval":
                case "call":
                case "case":
                case "catch":
                case "cbool":
                case "cbyte":
                case "cchar":
                case "cdate":
                case "cdbl":
                case "cdec":
                case "char":
                case "cint":
                case "class":
                case "clng":
                case "cobj":
                case "const":
                case "continue":
                case "csbyte":
                case "cshort":
                case "csng":
                case "cstr":
                case "ctype":
                case "cuint":
                case "culng":
                case "cushort":
                case "date":
                case "decimal":
                case "declare":
                case "default":
                case "delegate":
                case "dim":
                case "directcast":
                case "do":
                case "double":
                case "each":
                case "else":
                case "elseif":
                case "end":
                case "endif":
                case "enum":
                case "erase":
                case "error":
                case "event":
                case "exit":
                case "false":
                case "finally":
                case "for":
                case "friend":
                case "function":
                case "get":
                case "gettype":
                case "getxmlnamespace":
                case "global":
                case "gosub":
                case "goTo":
                case "handles":
                case "if":
                case "implements":
                case "imports":
                case "in":
                case "inherits":
                case "integer":
                case "interface":
                case "is":
                case "isnot":
                case "let":
                case "lib":
                case "like":
                case "long":
                case "loop":
                case "me":
                case "mod":
                case "module":
                case "mustinherit":
                case "mustoverride":
                case "mybase":
                case "myclass":
                case "namespace":
                case "narrowing":
                case "new":
                case "next":
                case "not":
                case "nothing":
                case "notinheritable":
                case "notoverridable":
                case "object":
                case "of":
                case "on":
                case "operator":
                case "option":
                case "optional":
                case "or":
                case "orelse":
                case "overloads":
                case "overridable":
                case "overrides":
                case "paramArray":
                case "partial":
                case "private":
                case "property":
                case "protected":
                case "public":
                case "raiseevent":
                case "readonly":
                case "redim":
                case "rem":
                case "removehandler":
                case "resume":
                case "return":
                case "sbyte":
                case "select":
                case "set":
                case "shadows":
                case "shared":
                case "short":
                case "single":
                case "static":
                case "step":
                case "stop":
                case "string":
                case "structure":
                case "sub":
                case "synclock":
                case "then":
                case "throw":
                case "to":
                case "true":
                case "try":
                case "trycast":
                case "typeof":
                case "uinteger":
                case "ulong":
                case "ushort":
                case "using":
                case "variant":
                case "wend":
                case "when":
                case "while":
                case "widening":
                case "with":
                case "withevents":
                case "writeonly":
                case "xor":
                    return true;
                default:
                    return false;
            }
        }
    }
}
