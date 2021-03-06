﻿/*
 * SonarLint for Visual Studio
 * Copyright (C) 2015-2016 SonarSource SA
 * mailto:contact@sonarsource.com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarLint.Common;
using Microsoft.CodeAnalysis.Formatting;
using SonarLint.Helpers;

namespace SonarLint.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class MethodOverrideChangedDefaultValueCodeFixProvider : CodeFixProvider
    {
        public const string Title = "Synchronize default parameter value";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(MethodOverrideChangedDefaultValue.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            var parameter = syntaxNode as ParameterSyntax;
            while(parameter == null && syntaxNode.Parent != null)
            {
                parameter = syntaxNode.Parent as ParameterSyntax;
                syntaxNode = syntaxNode.Parent;
            }

            if (parameter == null)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
            if (parameterSymbol == null)
            {
                return;
            }
            var methodSymbol = parameterSymbol.ContainingSymbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var index = methodSymbol.Parameters.IndexOf(parameterSymbol);
            IMethodSymbol overriddenMember;
            if (index == -1 ||
                !methodSymbol.TryGetOverriddenOrInterfaceMember(out overriddenMember))
            {
                return;
            }

            var overriddenParameter = overriddenMember.Parameters[index];

            ParameterSyntax newParameter;
            if (!TryGetNewParameterSyntax(parameter, overriddenParameter, out newParameter))
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            parameter,
                            newParameter.WithTriviaFrom(parameter));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static bool TryGetNewParameterSyntax(ParameterSyntax parameter, IParameterSymbol overriddenParameter, out ParameterSyntax newParameterSyntax)
        {
            if (!overriddenParameter.HasExplicitDefaultValue)
            {
                newParameterSyntax = parameter.WithDefault(null).WithAdditionalAnnotations(Formatter.Annotation);
                return true;
            }

            var defaultSyntax = (overriddenParameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax)?.Default;
            if (defaultSyntax != null)
            {
                newParameterSyntax = parameter.WithDefault(defaultSyntax.WithoutTrivia().WithAdditionalAnnotations(Formatter.Annotation));
                return true;
            }

            newParameterSyntax = null;
            return false;
        }
    }
}
