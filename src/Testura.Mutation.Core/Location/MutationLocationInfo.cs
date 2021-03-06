﻿using System;

namespace Testura.Mutation.Core.Location
{
    public class MutationLocationInfo
    {
        public string Line { get; set; }

        public string Where { get; set; }

        public LocationKind Kind { get; set; }

        public int GetLineNumber()
        {
            return int.Parse(Line.Split(new[] { "@(", ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);
        }

        public override string ToString()
        {
            return $"{Where}({Line})";
        }
    }
}
