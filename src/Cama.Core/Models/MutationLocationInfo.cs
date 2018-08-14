﻿namespace Cama.Core.Models
{
    public class MutationLocationInfo
    {
        public string Line { get; set; }

        public string Where { get; set; }

        public override string ToString()
        {
            return $"{Where}({Line})";
        }
    }
}
