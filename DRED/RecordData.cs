using System;
using System.Data;

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

        /// <summary>
        /// Creates a <see cref="RecordData"/> instance from a database <see cref="DataRow"/>.
        /// </summary>
        /// <param name="row">The source row containing record column values.</param>
        /// <returns>A populated <see cref="RecordData"/> object.</returns>
        public static RecordData FromDataRow(DataRow row)
        {
            return new RecordData
            {
                OpCo2 = row["OpCo2"] as string,
                Status = row["Status"] as string,
                MFR = row["MFR"] as string,
                DevCode = row["DevCode"] as string,
                BegSer = row["BegSer"] as string,
                EndSer = row["EndSer"] as string,
                Qty = row["Qty"] is DBNull ? null : Convert.ToInt32(row["Qty"]),
                PODate = row["PODate"] is DBNull ? null : Convert.ToDateTime(row["PODate"]),
                Vintage = row["Vintage"] as string,
                PONumber = row["PONumber"] as string,
                RecvDate = row["RecvDate"] is DBNull ? null : Convert.ToDateTime(row["RecvDate"]),
                UnitCost = row["UnitCost"] is DBNull ? null : Convert.ToDecimal(row["UnitCost"]),
                CID = row["CID"] as string,
                MENumber = row["MENumber"] as string,
                PurCode = row["PurCode"] as string,
                Est = row.Table.Columns.Contains("Est") && row["Est"] is not DBNull && Convert.ToBoolean(row["Est"]),
                TextFile = row.Table.Columns.Contains("TextFile") && row["TextFile"] is not DBNull && Convert.ToBoolean(row["TextFile"]),
                Comments = row["Comments"] as string,
                OOSSerials = row.Table.Columns.Contains("OOSSerials") ? row["OOSSerials"] as string : null,
                CreatedBy = row.Table.Columns.Contains("CreatedBy") ? row["CreatedBy"] as string : null,
                CreatedDate = row.Table.Columns.Contains("CreatedDate") && row["CreatedDate"] is not DBNull
                    ? Convert.ToDateTime(row["CreatedDate"])
                    : (DateTime?)null,
                ModifiedBy = row.Table.Columns.Contains("ModifiedBy") ? row["ModifiedBy"] as string : null,
                ModifiedDate = row.Table.Columns.Contains("ModifiedDate") && row["ModifiedDate"] is not DBNull
                    ? Convert.ToDateTime(row["ModifiedDate"])
                    : (DateTime?)null,
            };
        }
    }
}
