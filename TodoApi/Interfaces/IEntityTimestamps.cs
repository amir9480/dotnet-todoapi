namespace TodoApi.Interfaces;

/// <summary>
/// Interface to automatically handle create date and update date in entities automatically.
/// </summary>
public interface IEntityTimestamps
{
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}