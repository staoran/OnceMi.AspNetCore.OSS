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
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OBS.Internal.Log
{
   
    static internal class LoggerMgr
    {

        private const string LoggerCategoryName = "EasyLink.Storage.HuaweiOBS.SDK";

        private static volatile ILogger logger = NullLogger.Instance;

        internal static void Configure(ILoggerFactory loggerFactory)
        {
            Configure(loggerFactory?.CreateLogger(LoggerCategoryName));
        }

        internal static void Configure(ILogger logger)
        {
            LoggerMgr.logger = logger ?? NullLogger.Instance;
        }

        internal static void Initialize()
        {
        }

        internal static bool IsDebugEnabled
        {
            get
            {
                return logger.IsEnabled(LogLevel.Debug);
            }
        }

        internal static bool IsInfoEnabled
        {
            get
            {
                return logger.IsEnabled(LogLevel.Information);
            }
        }
        internal static bool IsWarnEnabled
        {
            get
            {
                return logger.IsEnabled(LogLevel.Warning);
            }
        }
        internal static bool IsErrorEnabled
        {
            get
            {
                return logger.IsEnabled(LogLevel.Error);
            }
        }


        internal static void Debug(string param)
        {
            Debug(param, null);
        }

        internal static void Error(string param)
        {
            Error(param, null);
        }

        internal static void Info(string param)
        {
            Info(param, null);
        }

        internal static void Warn(string param)
        {
            Warn(param, null);
        }

        internal static void Debug(string param, Exception exception)
        {
            Log(LogLevel.Debug, param, exception);
        }

        internal static void Error(string param, Exception exception)
        {
            Log(LogLevel.Error, param, exception);
        }

        internal static void Info(string param, Exception exception)
        {
            Log(LogLevel.Information, param, exception);
        }

        internal static void Warn(string param, Exception exception)
        {
            Log(LogLevel.Warning, param, exception);
        }

        private static void Log(LogLevel logLevel, string message, Exception exception)
        {
            logger.Log(logLevel, 0, message, exception, (state, ex) => state);
        }

    }
}
