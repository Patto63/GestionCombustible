using System.ComponentModel.DataAnnotations;

namespace AuthService.Infrastructure.Security;

public class BcryptOptions
{
    public const string SectionName = "BcryptOptions";

    [Range(10, 16, ErrorMessage = "WorkFactor debe estar entre 10 y 16")]
    public int WorkFactor { get; set; } = 12;
}