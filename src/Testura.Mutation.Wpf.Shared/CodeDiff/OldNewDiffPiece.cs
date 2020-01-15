﻿using DiffPlex.DiffBuilder.Model;

namespace Testura.Mutation.Wpf.Shared.CodeDiff
{
    public class OldNewDiffPiece
    {
        public DiffPiece Old { get; set; }

        public DiffPiece New { get; set; }

        public int Length { get; set; }
    }
}