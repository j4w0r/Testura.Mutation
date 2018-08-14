﻿using System.Collections.Generic;
using System.Linq;
using Cama.Core.Models;
using Cama.Core.Models.Mutation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cama.Core.Mutation.Mutators
{
    public class Mutator : CSharpSyntaxRewriter, IMutator
    {
        public Mutator()
        {
            Replacers = new List<MutationInfo>();
        }

        protected IList<MutationInfo> Replacers { get; }

        public IList<MutatedDocument> GetMutatedDocument(SyntaxNode root, Document document, List<UnitTestInformation> connectedTests)
        {
            Replacers.Clear();
            Visit(root);
            return Replacers.Select(r => new MutatedDocument(document, r, connectedTests)).ToList();
        }

        /*
        protected StatementSyntax GetStatement(ExpressionSyntax binaryExpressionSyntax)
        {
            return binaryExpressionSyntax.FirstAncestorOrSelf<StatementSyntax>();
        }
        */

        protected MutationLocationInfo GetWhere(CSharpSyntaxNode syntaxNode)
        {
            var where = "Unknown";

            var methodDeclaration = syntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (methodDeclaration != null)
            {
                where = $"{methodDeclaration.Identifier.Value}(M)";
            }

            var constructorDeclaration = syntaxNode.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

            if (constructorDeclaration != null)
            {
                where = $"{constructorDeclaration.Identifier.Value}(C)";
            }

            var propertyDeclaration = syntaxNode.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (propertyDeclaration != null)
            {
                where = $"{propertyDeclaration.Identifier.Value}(P)";
            }

            var location = syntaxNode.GetLocation();
            var locationString = "Unknown line";
            var pos = location.GetLineSpan();
            if (pos.Path != null)
            {
                locationString = $"@({pos.StartLinePosition.Line + 1}:{pos.StartLinePosition.Character + 1})";
            }

            return new MutationLocationInfo { Where = where, Line = locationString };
        }
    }
}
