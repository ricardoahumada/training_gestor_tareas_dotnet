namespace TaskManager.Domain.Common
{
    /// <summary>
    /// Representa el resultado de una operación que puede tener éxito o fallar.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public object? Data { get; }
        
        protected Result(bool isSuccess, string? errorMessage, object? data)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Data = data;
        }
        
        public static Result Success() => new Result(true, null, null);
        public static Result Success(object data) => new Result(true, null, data);
        public static Result Failure(string errorMessage) => new Result(false, errorMessage, null);
        
        public T GetData<T>()
        {
            if (Data is T result)
                return result;
            throw new InvalidOperationException("No data available");
        }
    }
    
    /// <summary>
    /// Representa el resultado genérico de una operación que puede tener éxito o fallar.
    /// </summary>
    /// <typeparam name="T">Tipo de datos devuelto en caso de éxito.</typeparam>
    public class Result<T> : Result
    {
        private readonly T? _data;
        
        private Result(bool isSuccess, string? errorMessage, T? data)
            : base(isSuccess, errorMessage, data)
        {
            _data = data;
        }
        
        public new T? Data => _data;
        
        public static new Result<T> Success(T data) => new Result<T>(true, null, data);
        public static new Result<T> Failure(string errorMessage) => new Result<T>(false, errorMessage, default);
        
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            if (IsSuccess && _data != null)
                return onSuccess(_data);
            return onFailure(ErrorMessage ?? "Unknown error");
        }
    }
    
    /// <summary>
    /// Resultado paginado de una consulta.
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la página.</typeparam>
    public class PagedResult<T>
    {
        public IList<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        
        public PagedResult(IList<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
