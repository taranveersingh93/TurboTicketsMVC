namespace TurboTicketsMVC.Models.ChartModels
{
    public class TicketsDevelopersData
    {
        public List<TicketsDevelopersBar> Data { get; set; }
    }

    public class TicketsDevelopersBar
    {
        public string[] X { get; set; }
        public int[] Y { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
