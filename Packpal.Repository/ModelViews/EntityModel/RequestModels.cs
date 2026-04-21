using Packpal.DAL.Enum;

namespace Packpal.DAL.ModelViews.EntityModel
{
    public class RequestQuery
    {
        public RequestTypeEnum? Type { get; set; }
        public RequestStatusEnum? Status { get; set; }
        public string? Username { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class ViewRequestModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string? Data { get; set; } // JSON string containing request-specific data
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
        public string Username { get; set; } = string.Empty;
    }
    public class CreateRequestModel
    {
        public Guid UserId { get; set; }
        public required string Type { get; set; }
        public string? Data { get; set; } // JSON string containing request-specific data
    }
}
