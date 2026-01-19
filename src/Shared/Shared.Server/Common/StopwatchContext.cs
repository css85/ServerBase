using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace SampleGame.Shared.Common
{
    public class StopwatchContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;

        private readonly string _memberName;
        private readonly string _sourceFilePath;
        private readonly int _sourceLineNumber;

        public StopwatchContext(ILogger logger,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
            _memberName = memberName;
            _sourceFilePath = sourceFilePath;
            _sourceLineNumber = sourceLineNumber;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.LogWarning("{SourceFilePath}:{SourceLineNumber}({MemberName}) Stopwatch: {Elapsed}",
                _sourceFilePath, _sourceLineNumber, _memberName, _stopwatch.Elapsed);
        }
    }
    
    public static class StopwatchUtility
    {
        public static StopwatchContext Start(ILogger logger)
        {
            return new StopwatchContext(logger);
        }
    }
}