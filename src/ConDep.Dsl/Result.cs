using System.Dynamic;

namespace ConDep.Dsl
{
    public class Result
    {

        public Result(bool success, bool changed)
        {
            Changed = changed;
            Success = success;
            Data = new ExpandoObject();
        }

        public bool Success { get; set; }
        public bool Changed { get; set; }
        public dynamic Data { get; }

        public static Result SuccessChanged()
        {
            return new Result(true, true);
        }

        public static Result SuccessUnChanged()
        {
            return new Result(true, false);
        }

        public static Result Failed()
        {
            return new Result(false, false);
        }
    }
}