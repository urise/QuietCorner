namespace CommonClasses.MethodArguments
{
    public class DeleteArg
    {
        public int Id { get; set; }
        public string Reason { get; set; }

        public DeleteArg() { }

        public DeleteArg(int id, string reason)
        {
            Id = id;
            Reason = reason;
        }
    }
}