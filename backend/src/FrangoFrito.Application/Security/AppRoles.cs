namespace FrangoFrito.Application.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Attendant = "Atendente";
    public const string Kitchen = "Cozinha";
    public const string Courier = "Entregador";

    public static readonly string[] All = [Admin, Attendant, Kitchen, Courier];
}
