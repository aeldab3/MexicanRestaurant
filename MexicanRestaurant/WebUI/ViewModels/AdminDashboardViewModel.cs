namespace MexicanRestaurant.WebUI.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<string> TopProductNames { get; set; }
        public List<int> TopProductSales { get; set; }
        public Dictionary<string, decimal> RevenueByDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
