namespace GazeManager.Models.Requests
{
    public class BaseFilter
    {
        public int? PageIndex { get; set; } = null;

        public int? PageSize { get; set; } = null;
    }
}