using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public uint AssignmentCategoryId { get; set; }
        public string AcName { get; set; } = null!;
        public uint CategoryOf { get; set; }
        public uint Weight { get; set; }

        public virtual Class CategoryOfNavigation { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
