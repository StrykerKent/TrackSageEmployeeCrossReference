using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tract_XRref.Models
{
    public class Contract_PO_LineMetadata
    {
        [Required]
        [Display(Name = "Contract")]
        public int Client_Contract_ID;

        [StringLength(9,MinimumLength=6)]
        [Display(Name = "Sage Job")]
        public string Sage_Job;

        [Required]
        [StringLength(20)]
        public string PO_Number;

        [Required]
        public int PO_Line;

        [Display(Name = "Description")]
        [StringLength(255)]
        public string PO_Line_Description;

        [Required]
        [Display(Name = "Issued Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public string PO_Line_Issued_Date;

        [StringLength(50)]
        public string Echo_Contact;

        [StringLength(50)]
        public string Customer_Contact;

        [Range(-9999999999999999.99, 9999999999999999.99, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        [DisplayFormat(DataFormatString = "{0:0,0}")]
        public Nullable<decimal> PO_Line_Approved_Amount_Orig;

        [Range(-9999999999999999.99, 9999999999999999.99, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public Nullable<decimal> PO_Line_Approved_Amount_Revised;

        [Range(.00, 1, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public decimal? PO_Line_Percent_Work_Completed;

        [Required]
        [Display(Name="Status")]
        public string PO_Line_Status_ID;

        [StringLength(1000)]
        public string PO_Line_Attachment;

        [StringLength(4000)]
        public string PO_Line_Notes;
    }

    public partial class spEmployeeSelect_ResultMetadata
    {
        public string Client_Employee_ID;
        [Key]
        public string Echo_Employee_Code;
        public int Client_ID;
        public string Client_Site_Code;
        public Nullable<bool> is_Active;
        public string Employee_Full_Name;
        public string Occupation;
        public string JC_Cost_Code;
        public string Description;
        public string SSN4;
    }
}