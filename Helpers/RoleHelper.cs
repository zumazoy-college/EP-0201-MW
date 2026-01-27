using EP_0201_MW.Models;
using System.Windows;

namespace EP_0201_MW.Helpers
{
    public static class RoleHelper
    {
        public static void CheckAccess(User user, string requiredAction, string message = "У вас нет доступа")
        {
            if (!HasAccess(user, requiredAction))
            {
                MessageBox.Show(message, "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                throw new UnauthorizedAccessException(message);
            }
        }

        public static bool HasAccess(User user, string requiredAction)
        {
            if (user == null || user.Role == null) return false;

            string role = user.Role.Title;

            switch (requiredAction)
            {
                // Администратор - всё
                case "ViewAnything":
                case "EditAnything":
                case "DeleteAnything":
                case "AddAnything":
                case "GenerateReports":
                    return role == "Администратор";

                // Менеджер
                case "ViewMain":
                case "ViewClients":
                case "EditClients":
                case "DeleteClients":
                case "AddClients":
                case "ViewLeases":
                case "EditLeases":
                case "DeleteLeases":
                case "AddLeases":
                case "AddServices":
                    return role == "Администратор" || role == "Менеджер";

                // Директор
                case "ViewReports":
                case "GenerateReportsForDirector":
                    return role == "Администратор" || role == "Директор";

                // Просмотр для всех
                case "ViewOnly":
                    return true;

                default:
                    return false;
            }
        }

        // Быстрые методы для часто используемых проверок
        public static bool CanEditClients(User user) => HasAccess(user, "EditClients");
        public static bool CanEditLeases(User user) => HasAccess(user, "EditLeases");
        public static bool CanEditWarehouses(User user) => HasAccess(user, "EditAnything"); // Только админ
        public static bool CanViewReports(User user) => HasAccess(user, "ViewReports");
        public static bool CanGenerateReports(User user) => HasAccess(user, "GenerateReports");
    }
}