namespace Tanukey
{
    public static class ConfigurationExtension
    {
        public static void AddIdFuscator(this Microsoft.AspNetCore.Mvc.MvcOptions options)
        {
            options.Filters.Add(new DecryptActionFilter());

            // Configurez les filtres de résultat globaux
            options.Filters.Add(new EncryptResultFilter());
        }
    }
}
