﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cama.Core.Models;
using Cama.Core.Models.Mutation;
using Cama.Core.Services;
using Cama.Infrastructure;
using Cama.Infrastructure.Tabs;
using Cama.Module.Mutation.Models;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Prism.Commands;
using Prism.Mvvm;

namespace Cama.Module.Mutation.Sections.Overview
{
    public class MutationOverviewViewModel : BindableBase
    {
        private readonly SomeService _someService;
        private readonly IMutationModuleTabOpener _tabOpener;
        private readonly ILoadingDisplayer _loadingDisplayer;

        public MutationOverviewViewModel(SomeService someService, IMutationModuleTabOpener tabOpener, ILoadingDisplayer loadingDisplayer)
        {
            _someService = someService;
            _someService.MutationDocumentStatus += SomeServiceOnMutationDocumentStatus;
            _tabOpener = tabOpener;
            _loadingDisplayer = loadingDisplayer;
            Documents = new ObservableCollection<DocumentRowModel>();
            CreateDocumentsCommand = new DelegateCommand(CreateDocuments);
            DocumentSelectedCommand = new DelegateCommand<DocumentRowModel>(DocumentSelected);
            RunAllMutationsCommand = new DelegateCommand(RunAllMutations);

            ILog myLogger = LogManager.GetLogger("Audit");

            var auditAppender = new EventLogAppender()
            {
                Name = "AuditAppender",
                
                Layout = new PatternLayout()
                {
                    ConversionPattern = "%newline %date %-5level %newline%message%newline",
                },
            };

            auditAppender.

            ((PatternLayout)auditAppender.Layout).ActivateOptions();
            auditAppender.ActivateOptions();

            log4net.Repository.Hierarchy.Logger l = (log4net.Repository.Hierarchy.Logger)myLogger.Logger;
            l.AddAppender(auditAppender);
            l.Repository.Configured = true;
        }

        public DelegateCommand CreateDocumentsCommand { get; set; }

        public DelegateCommand RunAllMutationsCommand { get; set; }

        public DelegateCommand<DocumentRowModel> DocumentSelectedCommand { get; set; }

        public ObservableCollection<DocumentRowModel> Documents { get; set; }

        public bool IsMutationDocumentsLoaded => Documents.Any();

        private async void CreateDocuments()
        {
            _loadingDisplayer.ShowLoading("Creating mutation documents..");
            var result = await Task.Run(() => _someService.DoSomeWorkAsync(@"D:\Programmering\Testura\Testura.Code\Testura.Code.sln", "Testura.Code", "Testura.Code.Tests"));
            foreach (var mutatedDocument in result)
            {
                Documents.Add(new DocumentRowModel { Document = mutatedDocument, Status = "Not run" });
            }

            _loadingDisplayer.HideLoading();
        }

        private void RunAllMutations()
        {
            _tabOpener.OpenTestRunTab(Documents.Select(d => d.Document).ToList());
        }

        private void DocumentSelected(DocumentRowModel documentRow)
        {
            _tabOpener.OpenDocumentDetailsTab(documentRow.Document);
        }

        private void SomeServiceOnMutationDocumentStatus(object sender, CreateMutationDocumentStatus createMutationDocumentStatus)
        {
            _loadingDisplayer.ShowLoading($"Creating mutation documents.. {createMutationDocumentStatus.CurrentDocument} ({createMutationDocumentStatus.PercentageDone}%)");
        }
    }
}