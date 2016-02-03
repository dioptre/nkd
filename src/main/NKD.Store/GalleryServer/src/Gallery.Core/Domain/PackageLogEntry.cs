using System;
using System.ComponentModel.DataAnnotations;
using Gallery.Core.Enums;

namespace Gallery.Core.Domain
{
    public class PackageLogEntry
    {
        public int Id { get; set; }
        [Required]
        public string PackageId { get; set; }
        [Required]
        public string PackageVersion { get; set; }
        [Required]
        public DateTime DateLogged { get; set; }
        [Required]
        public int ActionValue { get; set; }

        public PackageLogAction Action
        {
            get { return (PackageLogAction)ActionValue; }
            set { ActionValue = (int)value; }
        }
    }
}