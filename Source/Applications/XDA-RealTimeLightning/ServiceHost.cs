//******************************************************************************************************
//  ServiceHost.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/18/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Configuration;
using GSF.IO;
using GSF.ServiceProcess;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using XDARTL.Logging;

namespace XDARTL
{
    public partial class ServiceHost : ServiceBase
    {
        #region [ Members ]

        // Fields
        private Func<Task> m_stopEngineFunc;

        #endregion

        #region [ Constructors ]

        public ServiceHost()
        {
            // Make sure default service settings exist
            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("DefaultCulture", "en-US", "Default culture to use for language, country/region and calendar formats.");

            // Attempt to set default culture
            string defaultCulture = systemSettings["DefaultCulture"].ValueAs("en-US");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture(defaultCulture);     // Defaults for date formatting, etc.
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture(defaultCulture);   // Culture for resource strings, etc.

            InitializeComponent();

            // Register event handlers.
            m_serviceHelper.ServiceStarted += ServiceHelper_ServiceStarted;
            m_serviceHelper.ServiceStopping += ServiceHelper_ServiceStopping;
        }

        #endregion

        #region [ Methods ]

        private void ServiceHelper_ServiceStarted(object sender, EventArgs e)
        {
            ServiceHelperAppender serviceHelperAppender;
            RollingFileAppender debugLogAppender;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Set current working directory to fix relative paths
            Directory.SetCurrentDirectory(FilePath.GetAbsolutePath(""));

            // Set up logging
            serviceHelperAppender = new ServiceHelperAppender(m_serviceHelper);

            debugLogAppender = new RollingFileAppender();
            debugLogAppender.StaticLogFileName = false;
            debugLogAppender.AppendToFile = true;
            debugLogAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            debugLogAppender.MaxSizeRollBackups = 10;
            debugLogAppender.PreserveLogFileNameExtension = true;
            debugLogAppender.MaximumFileSize = "1MB";
            debugLogAppender.Layout = new PatternLayout("%date [%thread] %-5level %logger - %message%newline");

            try
            {
                if (!Directory.Exists("Debug"))
                    Directory.CreateDirectory("Debug");

                debugLogAppender.File = @"Debug\XDA-RTL.log";
            }
            catch (Exception ex)
            {
                debugLogAppender.File = "XDA-RTL.log";
                m_serviceHelper.ErrorLogger.Log(ex);
            }

            debugLogAppender.ActivateOptions();
            BasicConfigurator.Configure(serviceHelperAppender, debugLogAppender);

            // Set up heartbeat and client request handlers
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("RestartEngine", "Restarts the real-time lightning data processing engine", RestartEngineRequestHandler));

            // Set up the real-time lightning data processing engine
            _ = RestartEngineAsync();
        }

        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            StopEngineAsync().GetAwaiter().GetResult();

            // Save updated settings to the configuration file
            ConfigurationFile.Current.Save();

            Dispose();
        }

        private async Task RestartEngineAsync()
        {
            TaskCreationOptions runAsync = TaskCreationOptions.RunContinuationsAsynchronously;
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>(runAsync);

            // Use this to control access to the cancellation token
            // source so it cannot be used after it is disposed
            CancellationTokenSource interlockedRef = null;

            async Task StopEngineAsync()
            {
                CancellationTokenSource cancellationTokenSource = Interlocked.Exchange(ref interlockedRef, null);
                cancellationTokenSource?.Cancel();
                await taskCompletionSource.Task;
            }

            void WrapException(string message, Exception ex)
            {
                string wrappedMessage = $"{message}: {ex.Message}";
                Exception wrapper = new Exception(wrappedMessage, ex);
                HandleException(wrapper);
            }

            async Task StartEngineAsync(CancellationTokenSource cancellationTokenSource)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    return;

                RealTimeLightningEngine engine = new RealTimeLightningEngine();

                engine.MessageLogged += (_, args) => HandleLogMessage(args.Argument);
                engine.WarningLogged += (_, args) => HandleWarningMessage(args.Argument);

                engine.TunnelException += (_, args) =>
                    WrapException("Exception encountered establishing SSH tunnel", args.Argument);

                engine.LightningException += (_, args) =>
                    WrapException("Exception encountered processing real-time lightning data", args.Argument);

                try { await engine.RunAsync(cancellationTokenSource.Token); }
                catch (Exception ex) { WrapException("Exception encountered when running the engine", ex); }
            }

            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                Interlocked.Exchange(ref interlockedRef, cancellationTokenSource);

                Func<Task> stopEngineFunc = Interlocked.Exchange(ref m_stopEngineFunc, StopEngineAsync);
                Task stopEngineTask = stopEngineFunc?.Invoke() ?? Task.CompletedTask;
                await stopEngineTask;

                try { await StartEngineAsync(cancellationTokenSource); }
                finally { taskCompletionSource.SetResult(null); }
            }
        }

        private async Task StopEngineAsync()
        {
            Func<Task> stopEngineFunc = Interlocked.CompareExchange(ref m_stopEngineFunc, null, null);
            Task stopEngineTask = stopEngineFunc?.Invoke() ?? Task.CompletedTask;
            await stopEngineTask;
        }

        // Restarts the real-time lightning data processing engine.
        private void RestartEngineRequestHandler(ClientRequestInfo requestInfo)
        {
            _ = RestartEngineAsync();
            SendResponse(requestInfo, true);
        }

        // Send the message to the service helper
        public void HandleLogMessage(string message) =>
            m_serviceHelper.UpdateStatus(UpdateType.Information, "{0}", message);

        // Send the message to the service helper
        public void HandleWarningMessage(string message) =>
            m_serviceHelper.UpdateStatus(UpdateType.Warning, "{0}", message);

        // Send the error to the service helper, error logger
        public void HandleException(Exception ex)
        {
            string newLines = string.Format("{0}{0}", Environment.NewLine);
            m_serviceHelper.ErrorLogger.Log(ex);
            m_serviceHelper.UpdateStatus(UpdateType.Alarm, "{0}", ex.Message + newLines);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (Exception ex in e.Exception.Flatten().InnerExceptions)
                HandleException(ex);

            e.SetObserved();
        }

        #region [ Broadcast Message Handling ]

        /// <summary>
        /// Sends an actionable response to client.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success)
        {
            SendResponseWithAttachment(requestInfo, success, null, null);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message and attachment.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="attachment">Attachment to send with response.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponseWithAttachment(ClientRequestInfo requestInfo, bool success, object attachment, string status, params object[] args)
        {
            try
            {
                // Send actionable response
                m_serviceHelper.SendActionableResponse(requestInfo, success, attachment, status, args);

                // Log details of client request as well as response
                if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
                {
                    string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
                    string arguments = requestInfo.Request.Arguments.ToString();
                    string message = responseType + (string.IsNullOrWhiteSpace(arguments) ? "" : "(" + arguments + ")");

                    if (status != null)
                    {
                        if (args.Length == 0)
                            message += " - " + status;
                        else
                            message += " - " + string.Format(status, args);
                    }

                    m_serviceHelper.StatusLog.WriteTimestampedLine(message);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to send client response due to an exception: {0}", ex.Message);
                HandleException(new InvalidOperationException(message, ex));
            }
        }

        /// <summary>
        /// Displays a response message to client requestor.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status)
        {
            DisplayResponseMessage(requestInfo, "{0}", status);
        }

        /// <summary>
        /// Displays a response message to client requestor.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            try
            {
                m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, string.Format("{0}{1}{1}", status, Environment.NewLine), args);
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to update client status \"{0}\" due to an exception: {1}", status.ToNonNullString(), ex.Message);
                HandleException(new InvalidOperationException(message, ex));
            }
        }

        #endregion

        #endregion
    }
}
