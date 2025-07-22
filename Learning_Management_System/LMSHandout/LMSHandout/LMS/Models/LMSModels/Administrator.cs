using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Administrator
    {
        public string UId { get; set; } = null!;
        public string FName { get; set; } = null!;
        public string? LName { get; set; }
        public DateOnly Dob { get; set; }
    }
}
