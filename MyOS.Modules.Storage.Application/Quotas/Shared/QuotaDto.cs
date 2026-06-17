namespace MyOS.Modules.Storage.Application.Quotas.Shared
{
    public sealed record QuotaDto
    {
        public Guid UserId { get; init; }
        public long MaxBytes { get; init; }
        public long UsedBytes { get; init; }
        public long AvailableBytes { get; init; }
    }
}
