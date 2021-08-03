using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
    public interface IAgsColumnBase
    {
        dynamic this[int rowIndex] { get; set; }

        List<dynamic> Data { get; }
        string Heading { get; set; }
        string PartOfGroup { get; set; }
        int RowCount { get; }
        string Status { get; set; }
        string Type { get; set; }
        string Unit { get; set; }
    }
}