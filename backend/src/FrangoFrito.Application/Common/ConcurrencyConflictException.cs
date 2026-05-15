namespace FrangoFrito.Application.Common;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(Exception innerException)
        : base("O registro foi alterado por outro processo. Recarregue e tente novamente.", innerException)
    {
    }
}
