using JetBrains.Annotations;
using NpoComputer.Nomad.Contract;
using NpoComputer.Nomad.Contract.Logging;
using NpoComputer.Nomad.Contract.Settings;
using NpoComputer.Nomad.Utility;
using System;
using System.Configuration;
using System.IO;
using System.Net.Configuration;

namespace NpoComputer.Nomad.Internal.Plugins.HelloDevicePlugin
{
  /// <summary>
  /// Конфигурация плагина.
  /// </summary>
  public class PluginConfiguration
  {
    #region Поля и свойства

    /// <summary>
    /// Путь до файла конфигурации по умолчанию.
    /// </summary>
    [NotNull]
    private static readonly string _configFilePath =
      ConfigurationUtils.GetAssemblyConfigurationFilePath(typeof(PluginConfiguration).Assembly);

    /// <summary>
    /// Имя файла конфигурации.
    /// </summary>
    [NotNull]
    public static readonly string ConfigFileName = Path.GetFileName(_configFilePath);

    /// <summary>
    /// Менеджер конфигурации.
    /// </summary>
    private static readonly HotPlugConfigManager _сonfigManager =
      new HotPlugConfigManager(ConfigFileName,
        @"App_Data\Plugins\" + Path.GetFileName(Path.GetDirectoryName(_configFilePath)), "configuration");

    /// <summary>
    /// Получает секцию настроек SMTP.
    /// </summary>
    /// <returns>Секция настроек SMTP.</returns>
    internal static SmtpSection SmtpSection
    {
      get
      {
        var configuration = _standaloneConfiguration.Value;

        if (configuration == null)
          throw new ConfigurationErrorsException("Section mailSettings not found");

        return (SmtpSection)configuration.GetSection("system.net/mailSettings/smtp");
      }
    }

    /// <summary>
    /// Возвращает <see cref="ILog"/> текущего класса.
    /// </summary>
    private static ILog Log => LogManager.GetLogger<PluginConfiguration>();

    /// <summary>
    /// Возвращает настройки Nomad.
    /// </summary>
    [NotNull]
    private static SettingsConfigurationItem Settings => _сonfigManager.GetSection<SettingsConfigurationItem>("settings");

    /// <summary>
    /// Конфигурация сборки.
    /// </summary>
    private static Lazy<Configuration> _standaloneConfiguration =
      new Lazy<Configuration>(() => ConfigurationUtils.GetConfiguration(_configFilePath), true);

    #endregion

    #region Методы

    /// <summary>
    /// Email администратора устройств.
    /// </summary>
    [CanBeNull]
    public static string DeviceAdminEmail => GetOrDefaultDynamicSetting("deviceAdminEmail", nullIfEmpty: true);

    /// <summary>
    /// Получает значения, связанные с указанным ключом, объединенные в один список, в котором используются разделители-запятые.
    /// </summary>
    /// <param name="key">Ключ для записи, содержащей значения, которые требуется получить.</param>
    /// <param name="defaultValue">Значение по умолчанию, возвращаемое в случае если ключ не найден в конфигурационном файле.</param>
    /// <param name="nullIfEmpty">true - в случае если искомое значение пустое возвращать null; иначе - false.</param>
    /// <returns>Объект System.String, который содержит список значений с разделителями-запятыми, 
    /// связанных с указанным ключом, если найден; в противном случае — значение <paramref name="defaultValue"/>.</returns>
    public static string GetOrDefaultDynamicSetting(string key, string defaultValue = null, bool nullIfEmpty = false)
    {
      var res = Settings[key] ?? defaultValue;
      res = res?.Trim();

      if (nullIfEmpty && string.IsNullOrEmpty(res))
        res = null;

      return res;
    }

    /// <summary>
    /// Преобразует значение настройки в соответствующий тип перечисления.
    /// </summary>
    /// <typeparam name="T">Целевой тип перечисления.</typeparam>
    /// <param name="key">Имя настройки.</param>
    /// <returns>Значение настройки.</returns>
    private static T GetEnumSetting<T>(string key) where T : struct
    {
      if (!Enum.TryParse(Settings[key], true, out T value))
        Log.WarnFormat("Параметр {0} файла конфигурации {1} не задан или имеет неверный формат. Присвоено значение по умолчанию: {2}", key, ConfigFileName, value);

      return value;
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Конструктор.
    /// </summary>
    static PluginConfiguration()
    {
      _сonfigManager.ConfigChanged += (sender, args) =>
      {
        _standaloneConfiguration = new Lazy<Configuration>(
          () => ConfigurationUtils.GetConfiguration(_configFilePath), true);
      };
    }

    #endregion
  }
}
