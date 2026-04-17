using System;

namespace DRED
{
    /// <summary>
    /// Represents a single record matching the shared schema across all 4 tables.
    /// </summary>
    public class RecordData
    {
        public string? OpCo2 { get; set; }
        public string? Status { get; set; }
        public string? MFR { get; set; }
        public string? DevCode { get; set; }
        public string? BegSer { get; set; }
        public string? EndSer { get; set; }
        public int? Qty { get; set; }
        public DateTime? PODate { get; set; }
        public string? Vintage { get; set; }
        public string? PONumber { get; set; }
        public DateTime? RecvDate { get; set; }
        public decimal? UnitCost { get; set; }
        public string? CID { get; set; }
        public string? MENumber { get; set; }
        public string? PurCode { get; set; }
        public bool Est { get; set; }
        public bool TextFile { get; set; }
        public string? Comments { get; set; }
        public string? OOSSerials { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
