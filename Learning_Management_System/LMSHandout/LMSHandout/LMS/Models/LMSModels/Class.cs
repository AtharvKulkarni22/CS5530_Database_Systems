using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrollments = new HashSet<Enrollment>();
        }

        public uint ClassId { get; set; }
        public uint SemesterYear { get; set; }
        public string SemesterSeason { get; set; } = null!;
        public uint OfferingOf { get; set; }
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? ProfessorId { get; set; }

        public virtual Course OfferingOfNavigation { get; set; } = null!;
        public virtual Professor? Professor { get; set; }
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
