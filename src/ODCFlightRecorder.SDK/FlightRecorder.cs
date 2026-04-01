using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging; // Required for native ODC logging
using ODCFlightRecorder; 

namespace ODCFlightRecorder.SDK {
    public class FlightRecorder {
        private TraceBatch_ST _batch;
        private Stopwatch _timer = new Stopwatch();
        private readonly ILogger _logger; // The optional ODC logger
        private readonly string _methodName;

        public FlightRecorder(string methodName, string correlationId, string existingSessionId = null, ILogger logger = null) {
            this._timer.Start(); 
            this._methodName = methodName;
            this._logger = logger; // Stored for use in AddStep

            string sessionId = string.IsNullOrWhiteSpace(existingSessionId) 
                ? Guid.NewGuid().ToString() 
                : existingSessionId;
            
            _batch = new TraceBatch_ST {
                SessionData = new TraceSession_ST {
                    Id = sessionId,
                    MethodName = methodName, 
                    CorrelationId = correlationId,
                    StartTime = DateTime.UtcNow,
                    IsError = false
                },
                LogsWithDetails = new List<LogWithDetail_ST>()
            };

            // Grab the native ODC trace ID to link your JSON to the portal logs
            var nativeTraceId = Activity.Current?.Id ?? "no-native-trace";
            AddStep("Trace Started", "START", $"NativeTrace: {nativeTraceId}");
        }

        public void AddStep(string name, string type = "INFO", string payload = "") {
            var logId = Guid.NewGuid().ToString();
            var offset = (int)_timer.ElapsedMilliseconds;
            
            // 1. Existing logic: Build the internal JSON batch
            var step = new LogWithDetail_ST {
                LogData = new TraceLog_ST {
                    Id = logId,
                    TraceSessionId = _batch.SessionData.Id,
                    StepName = name,
                    Type = type,
                    Timestamp = DateTime.UtcNow,
                    OffsetMs = offset
                },
                DetailData = new TraceDetail_ST {
                    Id = logId, 
                    Payload = payload
                }
            };
            _batch.LogsWithDetails.Add(step);

            // 2. Hybrid logic: Log to the ODC Portal (Logs tab) if logger is provided
            _logger?.LogInformation("[{Method}] {Step}: {Payload}", _methodName, name, payload);

            // 3. Hybrid logic: Add an event to the native ODC Distributed Trace
            Activity.Current?.AddEvent(new ActivityEvent(name, tags: new ActivityTagsCollection 
            { 
                { "step.type", type }, 
                { "step.payload", payload ?? "" },
                { "step.offset_ms", offset }
            }));
        }

        public TraceBatch_ST FinalizeBatch(bool hasError = false) {
            _timer.Stop();
            _batch.SessionData.DurationMs = (int)_timer.ElapsedMilliseconds;
            _batch.SessionData.IsError = hasError;
            
            AddStep("Trace Ended", "END");
            
            return _batch;
        }

        public string FinalizeBatchAsJson(bool hasError = false) {
            var batch = this.FinalizeBatch(hasError);
            return JsonConvert.SerializeObject(batch);
        }
    }
}