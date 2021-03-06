﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Medallion.Shell;
using Medallion.Shell.Streams;
using Newtonsoft.Json;
using Testura.Mutation.Core.Exceptions;
using Testura.Mutation.Core.Execution.Result;
using Testura.Mutation.Core.Execution.Runners;

namespace Testura.Mutation.Infrastructure
{
    public class TestRunnerConsoleClient : ITestRunnerClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TestRunnerConsoleClient));

        public Task<TestSuiteResult> RunTestsAsync(string runner, string dllPath, string dotNetPath, TimeSpan maxTime, CancellationToken cancellationToken = default(CancellationToken))
        {
            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "");

            return Task.Run(
                () =>
            {
                var allArguments = new List<string>
                {
                    runner,
                    dllPath,
                    maxTime.ToString(),
                    dotNetPath
                };

                try
                {
                    using (var command = Command.Run(
                         Path.Combine(binPath, "Testura.Mutation.TestRunner.Console.exe"),
                        allArguments,
                        o =>
                        {
                            o.StartInfo(si =>
                            {
                                si.CreateNoWindow = true;
                                si.UseShellExecute = false;
                                si.RedirectStandardError = true;
                                si.RedirectStandardInput = true;
                                si.RedirectStandardOutput = true;
                            });
                            o.Timeout(maxTime);
                            o.DisposeOnExit();
                        }))
                    {
                        command.Wait();

                        var error = string.Empty;
                        var success = ReadToEnd(command.StandardOutput, maxTime, cancellationToken, out var output) && ReadToEnd(command.StandardError, maxTime, cancellationToken, out error);

                        if (!success)
                        {
                            command.Kill();
                            throw new MutationDocumentException("We have a problem reading from stream. Killing this mutation");
                        }

                        try
                        {
                            if (!command.Result.Success)
                            {
                                Log.Info($"Message from test client - {output}.");
                                Log.Info($"Error from test client - {error}.");

                                return new TestSuiteResult
                                {
                                    IsSuccess = false,
                                    Name = $"ERROR - {error}",
                                    ExecutionTime = TimeSpan.Zero,
                                    TestResults = new List<TestResult>()
                                };
                            }
                        }
                        catch (TimeoutException)
                        {
                            Log.Info("Test client timed out. Infinite loop?");
                            return TestSuiteResult.Error("TIMEOUT", maxTime);
                        }
                        catch (TaskCanceledException)
                        {
                            Log.Info("Test runner was cancelled by request");
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        return JsonConvert.DeserializeObject<TestSuiteResult>(output);
                    }
                }
                catch (Win32Exception ex)
                {
                    Log.Error("Unknown exception from test client process", ex);
                    throw;
                }
            });
        }

        private bool ReadToEnd(ProcessStreamReader processStream, TimeSpan maxTime, CancellationToken cancellationToken, out string message)
        {
            var readStreamTask = Task.Run(
                () =>
                {
                    var streamMessage = string.Empty;

                    while (processStream.Peek() >= 0)
                    {
                        streamMessage += processStream.ReadLine();
                    }

                    return streamMessage;
                },
                cancellationToken);

            // We also have a max time in the test runner so add a bit of extra here
            // just in case so we don't fail it to early.
             var successful = readStreamTask.Wait((int)maxTime.Add(TimeSpan.FromSeconds(30)).TotalMilliseconds, cancellationToken);

            message = successful ? readStreamTask.Result : "Stuck when reading from stream!";
            return successful;
        }
    }
}
