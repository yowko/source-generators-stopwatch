// See https://aka.ms/new-console-template for more information

using SourceGeneratorsDemo;

var stopwatchBenchmark = new StopwatchBenchmark();
stopwatchBenchmark.Execute_New_GetTimestamp();
stopwatchBenchmark.Execute_Old_StartNew();
stopwatchBenchmark.Receiver_New_GetTimestamp();
stopwatchBenchmark.Receiver_Old_StartNew();