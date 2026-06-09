namespace CateringAnalyticsSystem.DTOs;

public class DiningTableDto
{
    public int Id { get; set; }
    public int Number { get; set; }
    public int SeatsCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
