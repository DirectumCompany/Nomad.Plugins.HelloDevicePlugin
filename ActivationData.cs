using NpoComputer.Nomad.DeviceValidation;

namespace NpoComputer.Nomad.Internal.Plugins.HelloDevicePlugin
{
  /// <summary>
  /// Модель данных с информацией об активации устройства.
  /// </summary>
  public class ActivationData
  {
    /// <summary>
    /// Пользователь.
    /// </summary>
    public IUserInfo User { get; set; }

    /// <summary>
    /// Информация об устройстве пользователя.
    /// </summary>
    public IDeviceInfo Device { get; set; }

    /// <summary>
    /// Наименование системы.
    /// </summary>
    public string SystemName { get; set; }
  }
}
