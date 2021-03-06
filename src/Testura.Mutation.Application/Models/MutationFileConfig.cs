﻿using System.Collections.Generic;
using Testura.Mutation.Core;
using Testura.Mutation.Core.Creator.Filter;

namespace Testura.Mutation.Application.Models
{
    public class MutationFileConfig
    {
        public MutationFileConfig()
        {
            NumberOfTestRunInstances = 3;
            MaxTestTimeMin = 5;
            BuildConfiguration = "debug";
            CreateBaseline = true;
            Mutators = new List<string>();
            Filter = new MutationDocumentFilter();
            ProjectMappings = new List<ProjectMapping>();
        }

        public string SolutionPath { get; set; }

        public IList<string> TestProjects { get; set; }

        public IList<string> IgnoredProjects { get; set; }

        public IList<ProjectMapping> ProjectMappings { get; set; }

        public MutationDocumentFilter Filter { get; set; }

        public int NumberOfTestRunInstances { get; set; }

        public string BuildConfiguration { get; set; }

        public int MaxTestTimeMin { get; set; }

        public string TestRunner { get; set; }

        public string DotNetPath { get; set; }

        public List<MutationRunLoggerKind> MutationRunLoggers { get; set; }

        public List<string> Mutators { get; set; }

        public GitInfo Git { get; set; }

        public bool CreateBaseline { get; set; }
    }
}
