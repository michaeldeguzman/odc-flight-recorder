using System;
using System.Collections.Generic;
using OutSystems.ExternalLibraries.SDK;

namespace ODCFlightRecorder {

    [OSStructure(Description = "Container for a complete trace session")]
    public struct TraceBatch_ST {
        public TraceSession_ST SessionData;
        public List<LogWithDetail_ST> LogsWithDetails;
    }

    [OSStructure]
    public struct LogWithDetail_ST {
        public TraceLog_ST LogData;
        public TraceDetail_ST DetailData;
    }

    [OSStructure]
    public struct TraceSession_ST {
        public string Id;
        public string MethodName; // Keeping your ODC typo for now so it maps!
        public DateTime StartTime;
        public int DurationMs;
        public bool IsError;
        public string CorrelationId;
    }

    [OSStructure]
    public struct TraceLog_ST {
        public string Id;
        public string TraceSessionId;
        public DateTime Timestamp;
        public int OffsetMs;
        public string StepName;
        public string Type;
    }

    [OSStructure]
    public struct TraceDetail_ST {
        public string Id;
        public string Payload;
    }
}