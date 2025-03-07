//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Accounts
    {
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UserID { get; set; }
        public bool AllowRiskManager { get; set; }
        public bool AllowBusinessCard { get; set; }
        public bool AllowFileRepository { get; set; }
        public bool AllowDAC6 { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public bool Active { get; set; }
        public string SupportEmails { get; set; }
        public bool AllowTrainingCourses { get; set; }
        public int TrainingCoursesCompanyId { get; set; }
        public Nullable<bool> allowadmincourseTrainingCourses { get; set; }
        public string CompanyLogo { get; set; }
    }
}
