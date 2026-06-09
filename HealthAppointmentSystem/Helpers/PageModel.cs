namespace HealthAppointmentSystem.Helpers
{
    public class PageModel<TModel>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<TModel> Items { get; set; }
        public PageModel()
        {
            Items = new List<TModel>();
        }
    }
}
