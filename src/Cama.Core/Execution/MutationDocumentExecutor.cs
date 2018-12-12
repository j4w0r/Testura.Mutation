﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Anotar.Log4Net;
using Cama.Core.Execution.Compilation;
using Cama.Core.Execution.Result;
using Cama.Core.Execution.Runners;
using Cama.Core.Solution;

namespace Cama.Core.Execution
{
    public class MutationDocumentExecutor
    {
        private readonly IMutationDocumentCompiler _compiler;
        private readonly TestRunnerDependencyFilesHandler _testRunnerDependencyFilesHandler;
        private readonly ITestRunnerFactory _testRunnerFactory;

        public MutationDocumentExecutor(IMutationDocumentCompiler compiler, TestRunnerDependencyFilesHandler testRunnerDependencyFilesHandler, ITestRunnerFactory testRunnerFactory)
        {
            _compiler = compiler;
            _testRunnerDependencyFilesHandler = testRunnerDependencyFilesHandler;
            _testRunnerFactory = testRunnerFactory;
        }

        public async Task<MutationDocumentResult> ExecuteMutationAsync(CamaConfig config, MutationDocument mutationDocument)
        {
            var mustationResult = new MutationDocumentResult
            {
                Id = mutationDocument.Id,
                MutationName = mutationDocument.MutationName,
                ProjectName = mutationDocument.ProjectName,
                FileName = mutationDocument.FileName,
                Location = mutationDocument.MutationDetails.Location,
                Orginal = mutationDocument.MutationDetails.Orginal.ToFullString(),
                FullOrginal = mutationDocument.MutationDetails.FullOrginal.ToFullString(),
                Mutation = mutationDocument.MutationDetails.Mutation.ToFullString(),
                FullMutation = mutationDocument.MutationDetails.FullMutation.ToFullString()
            };

            LogTo.Info($"Running mutation: \"{mutationDocument.MutationName}\"");

            var results = new List<TestSuiteResult>();

            // Create the temporary "head" mutation directory to run all tests
            var mutationDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestRun", mutationDocument.Id.ToString());

            // Where we should save our compiled mutation
            var mutationDllPath = Path.Combine(mutationDirectoryPath, $"{config.MutationProjects.FirstOrDefault(m => m.Name == mutationDocument.ProjectName).OutputFileName}");

            Directory.CreateDirectory(mutationDirectoryPath);

            mustationResult.CompilationResult = await _compiler.CompileAsync(mutationDllPath, mutationDocument);
            if (!mustationResult.CompilationResult.IsSuccess)
            {
                return mustationResult;
            }

            foreach (var testProject in config.TestProjects)
            {
                var baseline = config.BaselineInfos.FirstOrDefault(b => b.TestProjectName.Equals(testProject.Name, StringComparison.OrdinalIgnoreCase));
                var result = await RunTestAsync(config.TestRunner, mutationDirectoryPath, mutationDllPath, testProject, baseline?.GetTestProjectTimeout() ?? TimeSpan.FromMinutes(config.MaxTestTimeMin));
                results.Add(result);

                if (results.Any(r => !r.IsSuccess))
                {
                    break;
                }
            }

            try
            {
                Directory.Delete(mutationDirectoryPath, true);
            }
            catch (Exception ex)
            {
                LogTo.Error($"Failed to delete test directory: {ex.Message}");
            }

            var final = CombineResult(mutationDocument.FileName, results);
            LogTo.Info($"\"{mutationDocument.MutationName}\" done. Ran {final.TestResults.Count} tests and {final.TestResults.Count(t => !t.IsSuccess)} failed.");

            mustationResult.FailedTests = final.TestResults.Where(t => !t.IsSuccess).ToList();
            mustationResult.TestsRunCount = final.TestResults.Count;

            return mustationResult;
        }

        private async Task<TestSuiteResult> RunTestAsync(
            string testRunnerName,
            string mutationDirectoryPath,
            string mutationDllPath,
            SolutionProjectInfo testProject,
            TimeSpan testTimout)
        {
            LogTo.Info($"Starting to run tests in {testProject.OutputFileName}");
            var mutationTestDirectoryPath = Path.Combine(mutationDirectoryPath, Guid.NewGuid().ToString());
            var testDllPath = Path.Combine(mutationTestDirectoryPath, testProject.OutputFileName);
            Directory.CreateDirectory(mutationTestDirectoryPath);

            // Copy all files from the test directory to our own mutation test directory
            _testRunnerDependencyFilesHandler.CopyDependencies(testProject.OutputDirectoryPath, mutationTestDirectoryPath);

            // Copy the mutation to our mutation test directory (and override the orginal file)
            File.Copy(mutationDllPath, Path.Combine(mutationTestDirectoryPath, Path.GetFileName(mutationDllPath)), true);

            var testRunner = _testRunnerFactory.CreateTestRunner(testRunnerName);
            return await testRunner.RunTestsAsync(testDllPath, testTimout);
        }

        private TestSuiteResult CombineResult(string name, IList<TestSuiteResult> testResult)
        {
            var tests = new List<TestResult>();
            var executionTime = TimeSpan.Zero;

            foreach (var testSuiteResult in testResult)
            {
                tests.AddRange(testSuiteResult.TestResults);
                executionTime = executionTime.Add(testSuiteResult.ExecutionTime);
            }

            return new TestSuiteResult(name, tests, null, executionTime);
        }
    }
}