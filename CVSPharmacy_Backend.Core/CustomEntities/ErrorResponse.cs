using System;
using System.Collections.Generic;
using System.Text;

namespace CVSPharmacy_Backend.Core.CustomEntities
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public string? TraceId { get; set; }
    }
}
