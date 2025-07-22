using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public uint AssignmentId { get; set; }
        public string StudentId { get; set; } = null!;
        public DateTime SubmissionTime { get; set; }
        public uint Score { get; set; }
        public string SContents { get; set; } = null!;

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
