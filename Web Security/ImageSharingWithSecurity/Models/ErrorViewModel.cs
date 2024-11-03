namespace ImageSharingWithSecurity.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; init; }

        public string ErrId { get; init; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}