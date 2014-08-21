namespace ConDep.Dsl.Validation
{
    public class ValidationError
	{
		private readonly string _message;

		public ValidationError(string message)
		{
			_message = message;
		}

		public string Message
		{
			get { return _message; }
		}
	}
}