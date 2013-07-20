namespace CommonClasses.MethodResults
{
    public class MethodResult<T> : BaseResult
    {
        public T AttachedObject { get; set; }

        public MethodResult() { }

        public MethodResult(T obj)
        {
            AttachedObject = obj;
        }
    }
}