//******************************************************************************************************
//  RealTimeLightningEngine.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  09/27/2021 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Threading;
using Renci.SshNet;
using XDARTL.Configuration;
using CancellationToken = System.Threading.CancellationToken;

namespace XDARTL
{
    public class RealTimeLightningEngine
    {
        #region [ Members ]

        // Nested Types
        private class LightningInfo
        {
            #region [ Constructors ]

            public LightningInfo() =>
                ProcessingActions = InitializeProcessingActions();

            #endregion

            #region [ Properties ]

            public int UALFVersion
            {
                get => _Version;
                private set
                {
                    if (value != 0 && value != 1)
                        throw new ArgumentOutOfRangeException("UALF version must be either 0 or 1");
                    _Version = value;
                }
            }

            public int Year
            {
                get => _Year;
                private set
                {
                    const int MinYear = 1970;
                    const int MaxYear = 2032;
                    if (!(MinYear <= value && value <= MaxYear))
                        throw new ArgumentOutOfRangeException($"Year must be between {MinYear} and {MaxYear}");
                    _Year = value;
                }
            }

            public int Month
            {
                get => _Month;
                private set
                {
                    const int MinMonth = 1;
                    const int MaxMonth = 12;
                    if (!(MinMonth <= value && value <= MaxMonth))
                        throw new ArgumentOutOfRangeException($"Month must be between {MinMonth} and {MaxMonth}");
                    _Month = value;
                }
            }

            public int Day
            {
                get => _Day;
                private set
                {
                    const int MinDay = 1;
                    const int MaxDay = 31;
                    if (!(MinDay <= value && value <= MaxDay))
                        throw new ArgumentOutOfRangeException($"Day must be between {MinDay} and {MaxDay}");
                    _Day = value;
                }
            }

            public int Hour
            {
                get => _Hour;
                private set
                {
                    const int MinHour = 0;
                    const int MaxHour = 23;
                    if (!(MinHour <= value && value <= MaxHour))
                        throw new ArgumentOutOfRangeException($"Hour must be between {MinHour} and {MaxHour}");
                    _Hour = value;
                }
            }

            public int Minute
            {
                get => _Minute;
                private set
                {
                    const int MinMinute = 0;
                    const int MaxMinute = 59;
                    if (!(MinMinute <= value && value <= MaxMinute))
                        throw new ArgumentOutOfRangeException($"Minute must be between {MinMinute} and {MaxMinute}");
                    _Minute = value;
                }
            }

            public int Second
            {
                get => _Second;
                private set
                {
                    const int MinSecond = 0;
                    const int MaxSecond = 60;
                    if (!(MinSecond <= value && value <= MaxSecond))
                        throw new ArgumentOutOfRangeException($"Second must be between {MinSecond} and {MaxSecond}");
                    _Second = value;
                }
            }

            public int Nanosecond
            {
                get => _Nanosecond;
                private set
                {
                    const int MinNanosecond = 0;
                    const int MaxNanosecond = 999999999;
                    if (!(MinNanosecond <= value && value <= MaxNanosecond))
                        throw new ArgumentOutOfRangeException($"Nanosecond must be between {MinNanosecond} and {MaxNanosecond}");
                    _Nanosecond = value;
                }
            }

            public double Latitude
            {
                get => _Latitude;
                private set
                {
                    const double MinLatitude = -90.0D;
                    const double MaxLatitude = 90.0D;
                    if (!(MinLatitude <= value && value <= MaxLatitude))
                        throw new ArgumentOutOfRangeException($"Latitude must be between {MinLatitude} and {MaxLatitude}");
                    _Latitude = value;
                }
            }

            public double Longitude
            {
                get => _Longitude;
                private set
                {
                    const double MinLongitude = -180.0D;
                    const double MaxLongitude = 180.0D;
                    if (!(MinLongitude <= value && value <= MaxLongitude))
                        throw new ArgumentOutOfRangeException($"Longitude must be between {MinLongitude} and {MaxLongitude}");
                    _Longitude = value;
                }
            }

            public int PeakCurrent
            {
                get => _PeakCurrent;
                private set
                {
                    const int MinPeakCurrent = -9999;
                    const int MaxPeakCurrent = 9999;
                    if (!(MinPeakCurrent <= value && value <= MaxPeakCurrent))
                        throw new ArgumentOutOfRangeException($"Peak Current must be between {MinPeakCurrent} and {MaxPeakCurrent}");
                    _PeakCurrent = value;
                }
            }

            public int FlashMultiplicity
            {
                get => _FlashMultiplicity;
                private set
                {
                    const int MinFlashMultiplicity = 0;
                    const int MaxFlashMultiplicity = 99;
                    if (!(MinFlashMultiplicity <= value && value <= MaxFlashMultiplicity))
                        throw new ArgumentOutOfRangeException($"Flash Multiplicity must be between {MinFlashMultiplicity} and {MaxFlashMultiplicity}");
                    _FlashMultiplicity = value;
                }
            }

            public int ParticipatingSensors
            {
                get => _ParticipatingSensors;
                private set
                {
                    const int MinParticipatingSensors = 2;
                    const int MaxParticipatingSensors = 99;
                    if (!(MinParticipatingSensors <= value && value <= MaxParticipatingSensors))
                        throw new ArgumentOutOfRangeException($"Participating Sensors must be between {MinParticipatingSensors} and {MaxParticipatingSensors}");
                    _ParticipatingSensors = value;
                }
            }

            public int DegreesOfFreedom
            {
                get => _DegreesOfFreedom;
                private set
                {
                    const int MinDegreesOfFreedom = 0;
                    const int MaxDegreesOfFreedom = 99;
                    if (!(MinDegreesOfFreedom <= value && value <= MaxDegreesOfFreedom))
                        throw new ArgumentOutOfRangeException($"Degrees of Freedom must be between {MinDegreesOfFreedom} and {MaxDegreesOfFreedom}");
                    _DegreesOfFreedom = value;
                }
            }

            public double EllipseAngle
            {
                get => _EllipseAngle;
                private set
                {
                    const double MinEllipseAngle = 0.0D;
                    const double MaxEllipseAngle = 180.0D;
                    if (!(MinEllipseAngle <= value && value <= MaxEllipseAngle))
                        throw new ArgumentOutOfRangeException($"Ellipse Angle must be between {MinEllipseAngle} and {MaxEllipseAngle}");
                    _EllipseAngle = value;
                }
            }

            public double SemiMajorAxisLength
            {
                get => _SemiMajorAxisLength;
                private set
                {
                    const double MinSemiMajorAxisLength = 0.0D;
                    const double MaxSemiMajorAxisLength = 50.0D;
                    if (!(MinSemiMajorAxisLength <= value && value <= MaxSemiMajorAxisLength))
                        throw new ArgumentOutOfRangeException($"Semi-major Axis Length must be between {MinSemiMajorAxisLength} and {MaxSemiMajorAxisLength}");
                    _SemiMajorAxisLength = value;
                }
            }

            public double SemiMinorAxisLength
            {
                get => _SemiMinorAxisLength;
                private set
                {
                    const double MinSemiMinorAxisLength = 0.0D;
                    const double MaxSemiMinorAxisLength = 50.0D;
                    if (!(MinSemiMinorAxisLength <= value && value <= MaxSemiMinorAxisLength))
                        throw new ArgumentOutOfRangeException($"Semi-minor Axis Length must be between {MinSemiMinorAxisLength} and {MaxSemiMinorAxisLength}");
                    _SemiMinorAxisLength = value;
                }
            }

            public double ChiSquared
            {
                get => _ChiSquared;
                private set
                {
                    const double MinChiSquared = 0.0D;
                    const double MaxChiSquared = 999.99D;
                    if (!(MinChiSquared <= value && value <= MaxChiSquared))
                        throw new ArgumentOutOfRangeException($"Chi-squared must be between {MinChiSquared} and {MaxChiSquared}");
                    _ChiSquared = value;
                }
            }

            public double Risetime
            {
                get => _Risetime;
                private set
                {
                    const double MinRisetime = 0.0D;
                    const double MaxRisetime = 99.9D;
                    if (!(MinRisetime <= value && value <= MaxRisetime))
                        throw new ArgumentOutOfRangeException($"Risetime must be between {MinRisetime} and {MaxRisetime}");
                    _Risetime = value;
                }
            }

            public double PeakToZeroTime
            {
                get => _PeakToZeroTime;
                private set
                {
                    const double MinPeakToZeroTime = 0.0D;
                    const double MaxPeakToZeroTime = 999.9D;
                    if (!(MinPeakToZeroTime <= value && value <= MaxPeakToZeroTime))
                        throw new ArgumentOutOfRangeException($"Peak-to-zero Time must be between {MinPeakToZeroTime} and {MaxPeakToZeroTime}");
                    _PeakToZeroTime = value;
                }
            }

            public double MaximumRateOfRise
            {
                get => _MaximumRateOfRise;
                private set
                {
                    // Contrary to the UALF definition, it seems this value can be negative
                    const double MinMaximumRateOfRise = -999.9D;
                    const double MaxMaximumRateOfRise = 999.9D;
                    if (!(MinMaximumRateOfRise <= value && value <= MaxMaximumRateOfRise))
                        throw new ArgumentOutOfRangeException($"Maximum Rate-of-rise must be between {MinMaximumRateOfRise} and {MaxMaximumRateOfRise}");
                    _MaximumRateOfRise = value;
                }
            }

            public bool CloudIndicator { get; set; }
            public bool AngleIndicator { get; set; }
            public bool SignalIndicator { get; set; }
            public bool TimingIndicator { get; set; }

            public bool ProcessingComplete =>
                ProcessingActions.Count == 0;

            private int _Version { get; set; }
            private int _Year { get; set; }
            private int _Month { get; set; }
            private int _Day { get; set; }
            private int _Hour { get; set; }
            private int _Minute { get; set; }
            private int _Second { get; set; }
            private int _Nanosecond { get; set; }
            private double _Latitude { get; set; }
            private double _Longitude { get; set; }
            private int _PeakCurrent { get; set; }
            private int _FlashMultiplicity { get; set; }
            private int _ParticipatingSensors { get; set; }
            private int _DegreesOfFreedom { get; set; }
            private double _EllipseAngle { get; set; }
            private double _SemiMajorAxisLength { get; set; }
            private double _SemiMinorAxisLength { get; set; }
            private double _ChiSquared { get; set; }
            private double _Risetime { get; set; }
            private double _PeakToZeroTime { get; set; }
            private double _MaximumRateOfRise { get; set; }

            private Queue<Action<string>> ProcessingActions { get; }

            #endregion

            #region [ Methods ]

            public void Process(string field)
            {
                if (ProcessingComplete)
                    return;
                Action<string> processingAction = ProcessingActions.Dequeue();
                processingAction(field);
            }

            private int ProcessInteger(string name, string field) =>
                int.TryParse(field, out int num)
                    ? num
                    : throw new FormatException($"Error processing field {name} as integer (value: {field})");

            private double ProcessDouble(string name, string field) =>
                double.TryParse(field, out double num)
                    ? num
                    : throw new FormatException($"Error processing field {name} as double (value: {field})");

            private bool ProcessBoolean(string name, string field) =>
                int.TryParse(field, out int num) && (num == 0 || num == 1)
                    ? num != 0
                    : throw new FormatException($"Error processing field {name} as Boolean (value: {field})");

            private Queue<Action<string>> InitializeProcessingActions()
            {
                Queue<Action<string>> processingActions = new Queue<Action<string>>(25);
                processingActions.Enqueue(field => UALFVersion = ProcessInteger(nameof(UALFVersion), field));
                processingActions.Enqueue(field => Year = ProcessInteger(nameof(Year), field));
                processingActions.Enqueue(field => Month = ProcessInteger(nameof(Month), field));
                processingActions.Enqueue(field => Day = ProcessInteger(nameof(Day), field));
                processingActions.Enqueue(field => Hour = ProcessInteger(nameof(Hour), field));
                processingActions.Enqueue(field => Minute = ProcessInteger(nameof(Minute), field));
                processingActions.Enqueue(field => Second = ProcessInteger(nameof(Second), field));
                processingActions.Enqueue(field => Nanosecond = ProcessInteger(nameof(Nanosecond), field));
                processingActions.Enqueue(field => Latitude = ProcessDouble(nameof(Latitude), field));
                processingActions.Enqueue(field => Longitude = ProcessDouble(nameof(Longitude), field));
                processingActions.Enqueue(field => PeakCurrent = ProcessInteger(nameof(PeakCurrent), field));
                processingActions.Enqueue(field => FlashMultiplicity = ProcessInteger(nameof(FlashMultiplicity), field));
                processingActions.Enqueue(field => ParticipatingSensors = ProcessInteger(nameof(ParticipatingSensors), field));
                processingActions.Enqueue(field => DegreesOfFreedom = ProcessInteger(nameof(DegreesOfFreedom), field));
                processingActions.Enqueue(field => EllipseAngle = ProcessDouble(nameof(EllipseAngle), field));
                processingActions.Enqueue(field => SemiMajorAxisLength = ProcessDouble(nameof(SemiMajorAxisLength), field));
                processingActions.Enqueue(field => SemiMinorAxisLength = ProcessDouble(nameof(SemiMinorAxisLength), field));
                processingActions.Enqueue(field => ChiSquared = ProcessDouble(nameof(ChiSquared), field));
                processingActions.Enqueue(field => Risetime = ProcessDouble(nameof(Risetime), field));
                processingActions.Enqueue(field => PeakToZeroTime = ProcessDouble(nameof(PeakToZeroTime), field));
                processingActions.Enqueue(field => MaximumRateOfRise = ProcessDouble(nameof(MaximumRateOfRise), field));
                processingActions.Enqueue(field => CloudIndicator = ProcessBoolean(nameof(CloudIndicator), field));
                processingActions.Enqueue(field => AngleIndicator = ProcessBoolean(nameof(AngleIndicator), field));
                processingActions.Enqueue(field => SignalIndicator = ProcessBoolean(nameof(SignalIndicator), field));
                processingActions.Enqueue(field => TimingIndicator = ProcessBoolean(nameof(TimingIndicator), field));
                return processingActions;
            }

            #endregion
        }

        private class DbValue
        {
            public DbValue(object value, DbType dbType, byte? precision = null, byte? scale = null)
            {
                Value = value;
                DbType = dbType;
                Precision = precision;
                Scale = scale;
            }

            public object Value { get; }
            public DbType DbType { get; }
            public byte? Precision { get; }
            public byte? Scale { get; }
        }

        // Delegates
        private delegate Task AsyncLightningHandler(LightningInfo lightningInfo, CancellationToken cancellationToken);
        private delegate Task AsyncBufferHandler(byte[] buffer, int length, CancellationToken cancellationToken);

        // Events
        public event EventHandler<EventArgs<string>> MessageLogged;
        public event EventHandler<EventArgs<string>> WarningLogged;
        public event EventHandler<EventArgs<Exception>> TunnelException;
        public event EventHandler<EventArgs<Exception>> LightningException;
        public event EventHandler<EventArgs<Exception>> PurgeException;

        #endregion

        #region [ Methods ]

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            DoubleBufferedQueue<LightningInfo> lightningDataQueue = new DoubleBufferedQueue<LightningInfo>();

            Task HandleLightningDataAsync(LightningInfo lightningInfo, CancellationToken lightningCancellationToken)
            {
                IEnumerable<LightningInfo> lightningData = Enumerable.Repeat(lightningInfo, 1);
                lightningDataQueue.Enqueue(lightningData);
                return Task.CompletedTask;
            }

            XDocument configuration = XDocument.Load(@"xda-rtl-config.xml");
            DbInfo dbInfo = new DbInfo(configuration);
            SshInfo sshInfo = new SshInfo(configuration);
            TunnelInfo tunnelInfo = new TunnelInfo(configuration);
            Func<AsyncBufferHandler> bufferHandlerFactory = () => ToBufferHandler(HandleLightningDataAsync);

            using (CancellationTokenSource engineCancellationTokenSource = new CancellationTokenSource())
            using (cancellationToken.Register(() => engineCancellationTokenSource.Cancel()))
            {
                CancellationToken engineCancellationToken = engineCancellationTokenSource.Token;
                Task pollDataStreamTask = PollDataStreamAsync(sshInfo, tunnelInfo, bufferHandlerFactory, engineCancellationToken);
                Task pollLightningDataTask = PollQueueAsync(dbInfo, lightningDataQueue, engineCancellationToken);
                Task purgeOldRecordsTask = PurgeOldRecordsAsync(dbInfo, engineCancellationToken);

                try { await Task.WhenAny(pollDataStreamTask, pollLightningDataTask, purgeOldRecordsTask); }
                catch { /* The error will be raised again in a moment */ }
                finally { engineCancellationTokenSource.Cancel(); }

                await Task.WhenAll(pollDataStreamTask, pollLightningDataTask, purgeOldRecordsTask);
            }
        }

        private async Task PollQueueAsync(DbInfo dbInfo, DoubleBufferedQueue<LightningInfo> queue, CancellationToken cancellationToken)
        {
            const int ParameterCount = 20;
            const int GroupLimit = 1000 / ParameterCount;
            const int Delay = 5000;

            int strikeCount = 0;
            TaskSynchronizedOperation logOperation = new TaskSynchronizedOperation(DelayAndLogAsync);

            IEnumerable<DbValue> ToParameters(LightningInfo lightningInfo)
            {
                int year = lightningInfo.Year;
                int month = lightningInfo.Month;
                int day = lightningInfo.Day;
                int hour = lightningInfo.Hour;
                int minute = lightningInfo.Minute;
                int second = lightningInfo.Second;
                int nanosecond = lightningInfo.Nanosecond;

                DateTime strikeTime = new DateTime(year, month, day, hour, minute, second)
                    .AddTicks(nanosecond / 100);

                nanosecond %= 100;

                yield return new DbValue(lightningInfo.UALFVersion, DbType.Byte);
                yield return new DbValue(strikeTime, DbType.DateTime2);
                yield return new DbValue(nanosecond, DbType.Byte);
                yield return new DbValue(lightningInfo.Latitude, DbType.Decimal, 6, 4);
                yield return new DbValue(lightningInfo.Longitude, DbType.Decimal, 7, 4);
                yield return new DbValue(lightningInfo.PeakCurrent, DbType.Int16);
                yield return new DbValue(lightningInfo.FlashMultiplicity, DbType.Byte);
                yield return new DbValue(lightningInfo.ParticipatingSensors, DbType.Byte);
                yield return new DbValue(lightningInfo.DegreesOfFreedom, DbType.Byte);
                yield return new DbValue(lightningInfo.EllipseAngle, DbType.Double);
                yield return new DbValue(lightningInfo.SemiMajorAxisLength, DbType.Double);
                yield return new DbValue(lightningInfo.SemiMinorAxisLength, DbType.Double);
                yield return new DbValue(lightningInfo.ChiSquared, DbType.Double);
                yield return new DbValue(lightningInfo.Risetime, DbType.Double);
                yield return new DbValue(lightningInfo.PeakToZeroTime, DbType.Double);
                yield return new DbValue(lightningInfo.MaximumRateOfRise, DbType.Double);
                yield return new DbValue(lightningInfo.CloudIndicator, DbType.Boolean);
                yield return new DbValue(lightningInfo.AngleIndicator, DbType.Boolean);
                yield return new DbValue(lightningInfo.SignalIndicator, DbType.Boolean);
                yield return new DbValue(lightningInfo.TimingIndicator, DbType.Boolean);
            }

            void AddParameter(IDbCommand command, string name, DbValue dbValue)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = dbValue.Value;
                parameter.DbType = dbValue.DbType;
                if (!(dbValue.Precision is null))
                    parameter.Precision = dbValue.Precision.GetValueOrDefault();
                if (!(dbValue.Scale is null))
                    parameter.Scale = dbValue.Scale.GetValueOrDefault();
                command.Parameters.Add(parameter);
            }

            void ConfigureParameters(IDbCommand command, IEnumerable<DbValue> dbValues)
            {
                command.Parameters.Clear();

                int i = 0;
                foreach (DbValue dbValue in dbValues)
                {
                    string parameterName = $"@p{i}";
                    AddParameter(command, parameterName, dbValue);
                    i++;
                }
            }

            void BulkInsert(IDbConnection connection, IEnumerable<LightningInfo> lightningData)
            {
                string GetValuesList(int index)
                {
                    IEnumerable<string> parameterNames = Enumerable
                        .Range(index * ParameterCount, ParameterCount)
                        .Select(i => $"@p{i}");

                    string commaSeparatedList = string.Join(",", parameterNames);
                    return $"({commaSeparatedList})";
                }

                IEnumerable<DbValue> dbValues = lightningData
                    .SelectMany(ToParameters);

                IEnumerable<string> valuesLists = lightningData
                    .Select((_, index) => GetValuesList(index));

                string commaSeparatedValuesLists = string.Join(",", valuesLists);
                string query = $"INSERT INTO RTLightningStrike VALUES {commaSeparatedValuesLists}";

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    ConfigureParameters(command, dbValues);
                    command.ExecuteNonQuery();
                }
            }

            void InsertEach(IDbConnection connection, IEnumerable<LightningInfo> lightningData)
            {
                void HandleSqlException(SqlException ex)
                {
                    // Ignore duplicate key violations
                    if (ex.Number == 2627)
                        return;

                    OnLightningException(ex);
                }

                IEnumerable<string> parameterNames = Enumerable
                    .Range(0, ParameterCount)
                    .Select(i => $"@p{i}");

                string commaSeparatedList = string.Join(",", parameterNames);
                string query = $"INSERT INTO RTLightningStrike VALUES ({commaSeparatedList})";

                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (LightningInfo lightningInfo in lightningData)
                    {
                        IEnumerable<DbValue> dbValues = ToParameters(lightningInfo);
                        ConfigureParameters(command, dbValues);
                        try { command.ExecuteNonQuery(); }
                        catch (SqlException ex) { HandleSqlException(ex); }
                        catch (Exception ex) { OnLightningException(ex); }
                    }
                }
            }

            void UpdateStrikeCount(int count)
            {
                Interlocked.Add(ref strikeCount, count);
                logOperation.TryRunOnceAsync();
            }

            void UploadLightningData(IList<LightningInfo> lightningBuffer)
            {
                var groupings = lightningBuffer
                    .Select((Info, Index) => new { Info, Index })
                    .GroupBy(obj => obj.Index / GroupLimit, obj => obj.Info);

                using (IDbConnection connection = new SqlConnection(dbInfo.ConnectionString))
                {
                    connection.Open();

                    foreach (var grouping in groupings)
                    {
                        try { BulkInsert(connection, grouping); }
                        catch { InsertEach(connection, grouping); }
                    }
                }
            }

            async Task DelayAndLogAsync()
            {
                try { await Task.Delay(15000, cancellationToken); }
                catch (TaskCanceledException) { return; }

                int count = Interlocked.Exchange(ref strikeCount, 0);

                if (count == 0)
                    OnMessageLogged($"{count} strikes received in the last 15 seconds");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try { await Task.Delay(Delay, cancellationToken); }
                catch (TaskCanceledException) { break; }

                IList<LightningInfo> lightningBuffer = queue.Dequeue();

                if (!lightningBuffer.Any())
                    continue;

                UpdateStrikeCount(lightningBuffer.Count);
                try { UploadLightningData(lightningBuffer); }
                catch (Exception ex) { OnLightningException(ex); }
            }
        }

        private async Task PollDataStreamAsync(SshInfo sshInfo, TunnelInfo tunnelInfo, Func<AsyncBufferHandler> callbackFactory, CancellationToken cancellationToken)
        {
            async Task RunReceiverAsync(AsyncBufferHandler callback, CancellationToken receiverCancellationToken)
            {
                TcpClient tcpClient = null;
                NetworkStream stream = null;
                CancellationTokenRegistration registration = default;

                try
                {
                    tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(tunnelInfo.LocalHost, tunnelInfo.LocalPort);

                    stream = tcpClient.GetStream();
                    registration = receiverCancellationToken.Register(() => stream.Close());

                    byte[] buffer = new byte[1024];
                    while (!receiverCancellationToken.IsCancellationRequested)
                    {
                        int bytesRead = 0;
                        try { bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); }
                        catch { if (!receiverCancellationToken.IsCancellationRequested) throw; }

                        if (receiverCancellationToken.IsCancellationRequested)
                            break;

                        if (bytesRead == 0)
                            throw new System.IO.EndOfStreamException();

                        await callback(buffer, bytesRead, receiverCancellationToken);
                    }
                }
                finally
                {
                    registration.Dispose();
                    stream?.Dispose();
                    tcpClient?.Dispose();
                }
            }

            async Task RunShell(SshClient sshClient, CancellationToken shellCancellationToken)
            {
                using (ShellStream shellStream = sshClient.CreateShellStream("test", 80, 5, 560, 68, 1024))
                {
                    TimeSpan readTimeout = TimeSpan.FromMilliseconds(100);
                    TimeSpan connectionTimeout = TimeSpan.FromMinutes(1);
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    while (!shellCancellationToken.IsCancellationRequested)
                    {
                        string line = shellStream.ReadLine(readTimeout);

                        if (line is null)
                        {
                            if (stopwatch.Elapsed > connectionTimeout)
                                throw new TimeoutException("Timed out waiting for shell message from server");

                            try { await Task.Delay(5000, shellCancellationToken); }
                            catch (TaskCanceledException) { break; }
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine(line.Trim());
                        stopwatch.Restart();
                    }
                }
            }

            async Task RunTunnelAsync(CancellationToken tunnelCancellationToken)
            {
                // Use a task completion source to propagate exceptions from the SSH exception handlers
                TaskCreationOptions runAsync = TaskCreationOptions.RunContinuationsAsynchronously;
                TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>(runAsync);

                // Use this to control access to the cancellation token
                // source so it can be disposed at the appropriate time
                CancellationTokenSource interlockedRef = null;

                CancellationTokenSource CreateCancellationTokenSource()
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    Interlocked.Exchange(ref interlockedRef, cancellationTokenSource);
                    return cancellationTokenSource;
                }

                void HandleCancellation()
                {
                    CancellationTokenSource cancellationTokenSource = Interlocked.Exchange(ref interlockedRef, null);

                    // This logic differs from the exception handler to avoid
                    // masking exceptions in certain subtle race conditions
                    if (cancellationTokenSource is null)
                        return;

                    cancellationTokenSource.Cancel();
                    taskCompletionSource.TrySetResult(null);
                }

                void HandleSshException(Exception ex)
                {
                    CancellationTokenSource cancellationTokenSource = Interlocked.Exchange(ref interlockedRef, null);
                    cancellationTokenSource?.Cancel();
                    taskCompletionSource.TrySetException(ex);
                }

                using (PrivateKeyFile privateKeyFile = new PrivateKeyFile(sshInfo.KeyFile, sshInfo.PassPhrase))
                using (SshClient sshClient = new SshClient(sshInfo.Host, sshInfo.User, privateKeyFile))
                using (ForwardedPortLocal tunnel = new ForwardedPortLocal(tunnelInfo.LocalHost, tunnelInfo.LocalPort, tunnelInfo.RemoteHost, tunnelInfo.RemotePort))
                using (CancellationTokenSource receiverCancellationTokenSource = CreateCancellationTokenSource())
                using (CancellationTokenRegistration chainRegistration = tunnelCancellationToken.Register(HandleCancellation))
                {
                    CancellationToken receiverCancellationToken = receiverCancellationTokenSource.Token;

                    sshClient.ConnectionInfo.AuthenticationBanner += (sender, args) => System.Diagnostics.Debug.WriteLine(args.BannerMessage);
                    sshClient.HostKeyReceived += (sender, args) => System.Diagnostics.Debug.WriteLine("HostKeyReceived");
                    sshClient.ErrorOccurred += (_, args) => HandleSshException(args.Exception);
                    tunnel.Exception += (_, args) => HandleSshException(args.Exception);

                    sshClient.Connect();
                    sshClient.AddForwardedPort(tunnel);
                    tunnel.Start();

                    Task shellTask = Task.Run(async () =>
                    {
                        try { await RunShell(sshClient, receiverCancellationToken); }
                        catch (Exception ex) { HandleSshException(ex); }
                    });

                    while (!receiverCancellationToken.IsCancellationRequested)
                    {
                        // Wait a few seconds for before the first attempt
                        try { await Task.Delay(5000, receiverCancellationToken); }
                        catch (TaskCanceledException) { break; }

                        OnMessageLogged("Polling TCP data stream...");

                        // Recreate the callback each time the TCP polling is
                        // restarted in order to reset UTF8 decoder state
                        AsyncBufferHandler callback = callbackFactory();

                        try { await RunReceiverAsync(callback, receiverCancellationToken); }
                        catch (Exception ex) { OnLightningException(ex); }
                    }

                    await shellTask;
                    await taskCompletionSource.Task;
                }
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                OnMessageLogged("Starting SSH tunnel...");
                try { await RunTunnelAsync(cancellationToken); }
                catch (Exception ex) { OnTunnelException(ex); }

                try { await Task.Delay(5000, cancellationToken); }
                catch (TaskCanceledException) { break; }
            }
        }

        private async Task PurgeOldRecordsAsync(DbInfo dbInfo, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                void RunPurgeQuery()
                {
                    DateTime threshold = DateTime.UtcNow.AddDays(-14);

                    using (AdoDataConnection connection = new AdoDataConnection(dbInfo.ConnectionString, typeof(SqlConnection), typeof(SqlDataAdapter)))
                        connection.ExecuteNonQuery("DELETE FROM RTLightningStrike WHERE StrikeTime < {0}", threshold);
                }

                TimeSpan interval = TimeSpan.FromHours(4);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try { RunPurgeQuery(); }
                    catch (Exception ex) { OnPurgeException(ex); }

                    try { await Task.Delay(interval, cancellationToken); }
                    catch (TaskCanceledException) { break; }
                }
            });
        }

        private AsyncBufferHandler ToBufferHandler(AsyncLightningHandler callback)
        {
            Decoder decoder = Encoding.UTF8.GetDecoder();
            char[] text = null;

            StringBuilder fieldBuilder = new StringBuilder();
            LightningInfo lightningInfo = null;
            bool isOutOfSync = false;

            return async (buffer, length, cancellationToken) =>
            {
                int charCount = decoder.GetCharCount(buffer, 0, length);
                if (text is null || text.Length < charCount)
                    Array.Resize(ref text, charCount);

                int chars = decoder.GetChars(buffer, 0, length, text, 0);

                for (int i = 0; i < chars; i++)
                {
                    char c = text[i];
                    bool isLineSeparator = c == '\r' || c == '\n';

                    if (isOutOfSync)
                    {
                        if (isLineSeparator)
                        {
                            OnWarningLogged("Found line separator, resuming data processing");
                            isOutOfSync = false;
                        }

                        continue;
                    }

                    if (!isLineSeparator && c != '\t')
                    {
                        fieldBuilder.Append(c);
                        continue;
                    }

                    // Process the field before checking error conditions
                    if (fieldBuilder.Length != 0)
                    {
                        if (lightningInfo is null)
                            lightningInfo = new LightningInfo();
                        lightningInfo.Process(fieldBuilder.ToString());
                        fieldBuilder.Clear();
                    }

                    // Normal case when encountering \r\n
                    if (lightningInfo is null)
                        continue;

                    // In this case, will need to wait for a line separator to sync back up
                    if (!isLineSeparator && lightningInfo.ProcessingComplete)
                        isOutOfSync = true;

                    bool isError = isOutOfSync || (isLineSeparator && !lightningInfo.ProcessingComplete);

                    if (isError)
                    {
                        string message = isOutOfSync
                            ? "Data processing out of sync with data stream, waiting for line separator..."
                            : "Unexpected line separator, skipping the current line";
                        OnWarningLogged(message);
                    }

                    if (!isError && lightningInfo.ProcessingComplete)
                        await callback(lightningInfo, cancellationToken);

                    if (isError || lightningInfo.ProcessingComplete)
                        lightningInfo = null;
                }
            };
        }

        private void OnMessageLogged(string message) =>
            MessageLogged?.Invoke(this, new EventArgs<string>(message + Environment.NewLine));

        private void OnWarningLogged(string message) =>
            WarningLogged?.Invoke(this, new EventArgs<string>(message + Environment.NewLine));

        private void OnTunnelException(Exception ex) =>
            TunnelException?.Invoke(this, new EventArgs<Exception>(ex));

        private void OnLightningException(Exception ex) =>
            LightningException?.Invoke(this, new EventArgs<Exception>(ex));

        private void OnPurgeException(Exception ex) =>
            PurgeException?.Invoke(this, new EventArgs<Exception>(ex));

        #endregion
    }
}
