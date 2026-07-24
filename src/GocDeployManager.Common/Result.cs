using System;

namespace GocDeployManager.Common
{
    /// <summary>
    /// Representa el resultado de una operación que puede fallar por una causa de negocio
    /// esperada (no una excepción). Ver sección 19 del análisis: las excepciones quedan
    /// reservadas para lo verdaderamente inesperado.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        public Exception Exception { get; }

        protected Result(bool isSuccess, string error, Exception exception)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Un resultado exitoso no puede tener un mensaje de error.");
            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Un resultado fallido debe tener un mensaje de error.");

            IsSuccess = isSuccess;
            Error = error;
            Exception = exception;
        }

        public static Result Ok() => new Result(true, null, null);

        public static Result Fail(string error, Exception exception = null) =>
            new Result(false, Guard.ContraNuloOVacio(error, nameof(error)), exception);

        public static Result<T> Ok<T>(T value) => new Result<T>(value, true, null, null);

        public static Result<T> Fail<T>(string error, Exception exception = null) =>
            new Result<T>(default(T), false, Guard.ContraNuloOVacio(error, nameof(error)), exception);
    }

    public sealed class Result<T> : Result
    {
        public T Value { get; }

        internal Result(T value, bool isSuccess, string error, Exception exception)
            : base(isSuccess, error, exception)
        {
            Value = value;
        }
    }
}
