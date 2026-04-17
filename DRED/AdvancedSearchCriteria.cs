using System;

namespace DRED
{
    /// <summary>
    /// Represents optional field-level filters for advanced record searches.
    /// </summary>
    public class AdvancedSearchCriteria
    {
        public string? OpCo2 { get; set; }
        public string? Status { get; set; }
        public string? MFR { get; set; }
        public string? DevCode { get; set; }
        public string? BegSer { get; set; }
        public string? EndSer { get; set; }
        public string? PONumber { get; set; }
        public string? Vintage { get; set; }
        public string? CID { get; set; }
        public string? MENumber { get; set; }
        public string? PurCode { get; set; }
        public bool? Est { get; set; }
        public bool? TextFile { get; set; }
        public string? Comments { get; set; }
        public DateTime? PODateFrom { get; set; }
        public DateTime? PODateTo { get; set; }
        public DateTime? RecvDateFrom { get; set; }
        public DateTime? RecvDateTo { get; set; }
        public decimal? CostMin { get; set; }
        public decimal? CostMax { get; set; }
        public int? QtyMin { get; set; }
        public int? QtyMax { get; set; }

        /// <summary>
        /// Gets whether no advanced criteria values are currently set.
        /// </summary>
        public bool IsEmpty =>
            OpCo2 == null && Status == null && MFR == null && DevCode == null &&
            BegSer == null && EndSer == null && PONumber == null && Vintage == null &&
            CID == null && MENumber == null && PurCode == null &&
            Est == null && TextFile == null && Comments == null &&
            PODateFrom == null && PODateTo == null &&
            RecvDateFrom == null && RecvDateTo == null &&
            CostMin == null && CostMax == null &&
            QtyMin == null && QtyMax == null;
    }
}
