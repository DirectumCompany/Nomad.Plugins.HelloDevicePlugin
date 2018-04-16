using NpoComputer.Nomad.Contract;
using NpoComputer.Nomad.Contract.Logging;
using NpoComputer.Nomad.DeviceValidation;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;

namespace NpoComputer.Nomad.Internal.Plugins.HelloDevicePlugin
{
  /// <summary>
  /// Плагин подтверждения регистрации устройств.
  /// </summary>
  public class Plugin : IDeviceValidationPlugin
  {
    #region Поля и свойства
    /// <summary>
    /// Менеджер устройств.
    /// </summary>
    private IDeviceManager _deviceManager;

    /// <summary>
    /// Логгер.
    /// </summary>
    internal static ILog Log => LogManager.GetLogger<Plugin>();

    /// <summary>
    /// Шаблон письма для администратора.
    /// </summary>
    internal static string AdminEmailTemplate => "AdminEmailTemplate.cshtml";

    /// <summary>
    /// Шаблон письма для пользователя.
    /// </summary>
    internal static string UserEmailTemplateJazzAndroid => "UserEmailTemplateJazzAndroid.cshtml";

    /// <summary>
    /// Шаблон письма для пользователя.
    /// </summary>
    internal static string UserEmailTemplateJazziOS => "UserEmailTemplateJazziOS.cshtml";

    /// <summary>
    /// Шаблон письма для пользователя.
    /// </summary>
    internal static string UserEmailTemplateSoloAndroid => "UserEmailTemplateSoloAndroid.cshtml";

    /// <summary>
    /// Шаблон письма для пользователя.
    /// </summary>
    internal static string UserEmailTemplateSoloiOS => "UserEmailTemplateSoloiOS.cshtml";

    /// <summary>
    /// Шаблоны.
    /// </summary>
    private IDictionary<string, Type> Templates => new Dictionary<string, Type>
    {
      { AdminEmailTemplate, typeof(ActivationData) },
      { UserEmailTemplateJazzAndroid, typeof(ActivationData) },
      { UserEmailTemplateJazziOS, typeof(ActivationData) },
      { UserEmailTemplateSoloAndroid, typeof(ActivationData) },
      { UserEmailTemplateSoloiOS, typeof(ActivationData) }
    };
    #endregion

    #region Методы
    public void Initialize()
    {
      Log.InfoFormat(Properties.Resources.InitializationTrace);
      _deviceManager = Dependency.Resolve<IDeviceManager>();

      try
      {
        var pluginFolderPath = Path.Combine(new Uri(Path.GetDirectoryName(typeof(Plugin).Assembly.CodeBase)).LocalPath, "EmailTemplates");
        Log.InfoFormat(Properties.Resources.TemplatesPathTrace, pluginFolderPath);

        var config = new TemplateServiceConfiguration
        {
          TemplateManager = new ResolvePathTemplateManager(new[] { pluginFolderPath })
        };

        Engine.Razor = RazorEngineService.Create(config);

        foreach (var template in Templates)
        {
          Log.InfoFormat(Properties.Resources.CompileTemplateTrace, template.Key);
          Engine.Razor.Compile(template.Key, template.Value);
        }
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(Properties.Resources.InitializationError, ex);
      }
    }

    /// <summary>
    /// Отправка сообщения при подключении нового устройства.
    /// </summary>
    /// <param name="deviceInfo">Информация об устройстве.</param>
    /// <param name="userInfo">Информация о пользователе.</param>
    public void ValidateDevice(IDeviceInfo deviceInfo, IUserInfo userInfo)
    {            
      _deviceManager.EnableDevice(deviceInfo.Id);
      MessageSender.Send(deviceInfo, userInfo);
    }

    #endregion
  }
}
