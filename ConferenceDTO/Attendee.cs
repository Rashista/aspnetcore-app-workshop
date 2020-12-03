using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConferenceDTO
{
    public class Attendee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public virtual string FirstName { get; set; }

        [Required]
        [StringLength(200)]
        public virtual string Lastname { get; set; }

        [Required]
        [StringLength(200)]
        public string UserName { get; set; }

        [StringLength(200)]
        public virtual string EmailAddress { get; set; }
    }
}
