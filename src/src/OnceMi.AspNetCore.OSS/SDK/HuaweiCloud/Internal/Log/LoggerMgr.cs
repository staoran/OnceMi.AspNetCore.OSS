/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace OBS.Internal.Log
{
    internal static class LoggerMgr
    {
        private static ILogger _logger = NullLogger.Instance;

        internal static ILogger Logger
        {
            get => _logger;
            set => _logger = value ?? NullLogger.Instance;
        }

        // No-op: kept for source compatibility with callers that call Initialize()
        internal static void Initialize() { }

        internal static bool IsDebugEnabled => _logger.IsEnabled(LogLevel.Debug);
        internal static bool IsInfoEnabled  => _logger.IsEnabled(LogLevel.Information);
        internal static bool IsWarnEnabled  => _logger.IsEnabled(LogLevel.Warning);
        internal static bool IsErrorEnabled => _logger.IsEnabled(LogLevel.Error);

        internal static void Debug(string param) => Debug(param, null);
        internal static void Info(string param)  => Info(param, null);
        internal static void Warn(string param)  => Warn(param, null);
        internal static void Error(string param) => Error(param, null);

        internal static void Debug(string param, Exception exception) =>
            _logger.LogDebug(exception, "{Message}", param);

        internal static void Info(string param, Exception exception) =>
            _logger.LogInformation(exception, "{Message}", param);

        internal static void Warn(string param, Exception exception) =>
            _logger.LogWarning(exception, "{Message}", param);

        internal static void Error(string param, Exception exception) =>
            _logger.LogError(exception, "{Message}", param);
    }
}
