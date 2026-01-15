namespace TaskManager.Infrastructure.Services
{
    /// <summary>
    /// Proveedor de fecha y hora.
    /// Facilita testing ymanejo de zonas horarias.
    /// </summary>
    public class DateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Today => DateTime.UtcNow.Date;
    }
}
