using System.ComponentModel.DataAnnotations;

namespace AgsVerifierLibrary.Enums
{
    public enum AgsStatus
    {
        KEY = 0,
        REQUIRED = 1,
        [Display(Name = "KEY+REQUIRED")]
        KEYPLUSREQUIRED = 2,
        OTHER = 3,
    }
}
