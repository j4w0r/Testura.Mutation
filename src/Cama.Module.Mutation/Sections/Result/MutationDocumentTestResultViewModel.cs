﻿using System.ComponentModel;
using Cama.Core.Models.Mutation;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace Cama.Module.Mutation.Sections.Result
{
    public class MutationDocumentTestResultViewModel : BindableBase, INotifyPropertyChanged
    {
        public MutationDocumentTestResultViewModel()
        {
            ShowFullCodeCommand = new DelegateCommand<bool?>(ShowFullCode);
        }

        public MutationDocumentResult Result { get; set; }

        public string CodeAfterMutation { get; set; }

        public string CodeBeforeMutation { get; set; }

        public string Title { get; set; }

        public SideBySideDiffModel Diff { get; private set; }

        public DelegateCommand<bool?> ShowFullCodeCommand { get; set; }

        public void SetMutationDocumentTestResult(MutationDocumentResult result)
        {
            Result = result;
            Title = $"Test results for {result.Document.FileName} - {result.Document.MutationInfo.Location}";
            ShowFullCode(false);
        }

        private void ShowFullCode(bool? showFullCode)
        {
            CodeBeforeMutation = showFullCode.Value ? Result.Document.MutationInfo.FullOrginal.ToFullString() : Result.Document.MutationInfo.Orginal.ToFullString();
            CodeAfterMutation = showFullCode.Value ? Result.Document.MutationInfo.FullMutation.ToFullString() : Result.Document.MutationInfo.Mutation.ToFullString();
            var diffBuilder = new SideBySideDiffBuilder(new Differ());
            Diff = diffBuilder.BuildDiffModel(CodeBeforeMutation, CodeAfterMutation);
        }
    }
}