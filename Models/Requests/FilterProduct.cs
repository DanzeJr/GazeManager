namespace GazeManager.Models.Requests
{
    public class FilterProduct : BaseFilter
    {
        public long? CategoryId { get; set; }

        public string Search { get; set; }
    }
}