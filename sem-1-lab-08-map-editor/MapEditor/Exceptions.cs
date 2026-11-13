namespace Exceptions;

public class PlayerMovementException : Exception
{
    public PlayerMovementException() : base() { }

    public PlayerMovementException(string message) : base(message) { }

    public PlayerMovementException(string message, Exception innerException) : base(message, innerException) { }
}

public class BlockedException : PlayerMovementException
{
    public BlockedException() : base("Игрок не может оказаться в препятствии") { }

    public BlockedException(string message) : base(message) { }

    public BlockedException(string message, Exception innerException) : base(message, innerException) { }
}

public class OutOfBoundsException : PlayerMovementException
{
    public OutOfBoundsException() : base("Игрок не может уйти за карту") { }

    public OutOfBoundsException(string message) : base(message) { }

    public OutOfBoundsException(string message, Exception innerException) : base(message, innerException) { }
}
