namespace App.Models.DTO
{
    /// <summary>
    /// Returns the results of a service to its calling method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }

        /// <summary>
        /// Creates a new instance of ServiceResult for successful results 
        /// </summary>
        /// <returns>New ServiceResult<T> with only success true</returns>
        public static ServiceResult<T> SuccessResult()
        {
            return new ServiceResult<T> { Success = true };
        }

        /// <summary>
        /// Creates a new instance of ServiceResult for successful results, along with return data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>New ServiceResult<T> with success true and data type</returns>
        public static ServiceResult<T> SuccessResult(T? data)
        {
            return new ServiceResult<T> { Success = true, Data = data };
        }

        /// <summary>
        /// Creates a new instance of ServiceResult for failure results, along with error message and method name
        /// /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="methodName"></param>        
        /// <returns>New ServiceResult<T> with success false and an error message</returns>
        public static ServiceResult<T> FailureResult(string errorMessage)
        {
            return new ServiceResult<T> { Success = false, ErrorMessage = errorMessage };
        }

        /// <summary>
        /// Creates a new instance of ServiceResult for failure results, along with return data, exception message, and method name
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="ex"></param>
        /// <param name="methodName"></param>
        /// <returns>New ServiceResult<T></returns>
        public static ServiceResult<T> FailureResult(string errorMessage, Exception ex)
        {
            return new ServiceResult<T> { Success = false, ErrorMessage = errorMessage, Exception = ex };
        }
    }
}
