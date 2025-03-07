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
    
    public partial class Users
    {
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserEmail { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> LastPasswordChange { get; set; }
        public string ResetHash { get; set; }
        public Nullable<System.DateTime> ResetRequestDate { get; set; }
        public int WrongPaswordAttempts { get; set; }
        public Nullable<bool> LicenseAgreement { get; set; }
        public bool Active { get; set; }
        public Nullable<bool> TwoFactorEnabled { get; set; }
        public string GoogleAuthenticatorSecretKey { get; set; }
        public Nullable<bool> IsGoogleAuthenticatorEnabled { get; set; }
        public string TwoFAEmailCode { get; set; }
        public Nullable<System.DateTime> TwoFAEmailCodeDateTime { get; set; }
    }
}
