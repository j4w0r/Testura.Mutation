﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Testura.Mutation.Application.Commands.Mutation.CreateMutations;
using Testura.Mutation.Application.Commands.Mutation.ExecuteMutations;
using Testura.Mutation.Application.Commands.Project.OpenProject;
using Testura.Mutation.Application.Commands.Report.Creator;
using Testura.Mutation.Core.Execution.Report;
using Testura.Mutation.Core.Execution.Report.Html;
using Testura.Mutation.Core.Execution.Report.Markdown;
using Testura.Mutation.Core.Execution.Report.Summary;
using Testura.Mutation.Core.Execution.Report.Testura;
using Testura.Mutation.Core.Execution.Report.Trx;

namespace Testura.Mutation.Console
{
    public class ConsoleMutationExecutor
    {
        private readonly IMediator _mediator;

        public ConsoleMutationExecutor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> ExecuteMutationRunner(string configPath, string savePath)
        {
            var config = await _mediator.Send(new OpenProjectCommand(configPath));

            var start = DateTime.Now;
            var mutationDocuments = await _mediator.Send(new CreateMutationsCommand(config));
            var results = await _mediator.Send(new ExecuteMutationsCommand(config, mutationDocuments.ToList(), null));

            var trxSavePath = Path.Combine(savePath, "result.trx");
            var reports = new List<ReportCreator>
            {
                new TrxReportCreator(trxSavePath),
                new MarkdownReportCreator(Path.ChangeExtension(trxSavePath, ".md")),
                new TesturaMutationReportCreator(Path.ChangeExtension(trxSavePath, ".Testura.Mutation")),
                new HtmlOnlyBodyReportCreator(Path.ChangeExtension(trxSavePath, ".html")),
                new TextSummaryReportCreator(Path.ChangeExtension(trxSavePath, ".txt"))
            };

            await _mediator.Send(new CreateReportCommand(results, reports, DateTime.Now - start));

            return !results.Any(r => r.Survived);
        }
    }
}