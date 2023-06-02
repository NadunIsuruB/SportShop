namespace HPlusSport.API.Filters
{
    public class QueryParameters
    {
        const int _maxSize = 100;
        private int _size = 50;

        public int Page { get; set; } = 1;

        public int Size
        {
            get { return _size; }
            set
            {
                _size = Math.Min(_maxSize, value);
            }
        }

        private string _sortOder = "asc";
        public string SortBy { get; set; } = "Id";
        public string SortOrder { get { return _sortOder; }
            set
            {
                if (value == "asc" || value == "desc")
                {
                    _sortOder= value;
                }
            } 
        }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SearchStr { get; set; } = string.Empty;
    }
}
