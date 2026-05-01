using Microsoft.AspNetCore.Http;

namespace AmityApp.Shared.Dtos;

public class SaveCordialDto

{
    public Guid CordialId { get; set; }
    public string? Content { get; set; }
    public IFormFile? Photo     { get; set; }
    public bool ExistingPhotoRemoved { get; set; }

    public string? Vibe { get; set; }
    public string? Visibility { get; set; }
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Content) && Photo is null)
            return false;
        return true;
    }
}
