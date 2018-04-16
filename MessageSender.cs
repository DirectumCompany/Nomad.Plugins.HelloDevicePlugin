using NpoComputer.Nomad.DeviceValidation;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Net;
using System.Net.Mail;

namespace NpoComputer.Nomad.Internal.Plugins.HelloDevicePlugin
{
  /// <summary>
  /// Отправитель сообщений.
  /// </summary>
  public static class MessageSender
  {
    /// <summary>
    /// Формирование сообщений.
    /// </summary>
    /// <param name="deviceInfo">Информация об устройстве</param>
    /// <param name="userInfo">Информация о пользователе.</param>
    public static void Send(IDeviceInfo deviceInfo, IUserInfo userInfo)
    {
      var activationData = new ActivationData
      {
        Device = deviceInfo,
        User = userInfo,
        SystemName = Properties.Resources.SystemName
      };

      try
      {
        var adminMessage = new MailMessage(PluginConfiguration.SmtpSection.From, PluginConfiguration.DeviceAdminEmail);
        adminMessage.IsBodyHtml = true;
        adminMessage.Body = Engine.Razor.Run(Plugin.AdminEmailTemplate, typeof(ActivationData), activationData);
        adminMessage.Subject = string.Format(Properties.Resources.AdminEmailSubject, userInfo.Name, deviceInfo.Name);
        Send(adminMessage);
      }
      catch (InvalidOperationException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(Properties.Resources.AdminEmailError, ex);
      }

      if (string.IsNullOrEmpty(userInfo.Email)) return;
      try
      {
        var userMessage = new MailMessage(PluginConfiguration.SmtpSection.From, userInfo.Email);
        userMessage.IsBodyHtml = true;
        var template = string.Format("UserEmailTemplate{0}{1}.cshtml",
          deviceInfo.AppVersion.Contains("Jazz") ? "Jazz" : "Solo",
          deviceInfo.OSVersion.Contains("Android") ? "Android" : "iOS");
        userMessage.Body = Engine.Razor.Run(template, typeof(ActivationData), activationData);
        userMessage.Subject = string.Format(Properties.Resources.UserEmailSubject, deviceInfo.Name);
        Send(userMessage);
      }
      catch (InvalidOperationException)
      {
        throw;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(Properties.Resources.UserEmailError, ex);
      }
    }

    /// <summary>
    /// Отправляет сообщение.
    /// </summary>
    /// <param name="message">Сообщение.</param>
    public static void Send(MailMessage message)
    {
      using (var client = new SmtpClient())
      {
        var smtpSection = PluginConfiguration.SmtpSection;

        if (smtpSection.Network != null)
        {
          client.Host = smtpSection.Network.Host;
          client.Port = smtpSection.Network.Port;
          client.UseDefaultCredentials = smtpSection.Network.DefaultCredentials;

          client.Credentials = new NetworkCredential(
            smtpSection.Network.UserName,
            smtpSection.Network.Password);

          client.EnableSsl = smtpSection.Network.EnableSsl;

          if (smtpSection.Network.TargetName != null)
            client.TargetName = smtpSection.Network.TargetName;
        }

        client.DeliveryMethod = smtpSection.DeliveryMethod;

        if (smtpSection.SpecifiedPickupDirectory?.PickupDirectoryLocation != null)
          client.PickupDirectoryLocation = smtpSection.SpecifiedPickupDirectory.PickupDirectoryLocation;

        Plugin.Log.InfoFormat(Properties.Resources.EmailTrace, message.To, message.Subject);

        try
        {
          client.Send(message);
        }
        catch (Exception ex)
        {
          var errorMessage = string.Format(Properties.Resources.EmailError, message.To);
          throw new InvalidOperationException(errorMessage, ex);
        }
      }
    }
  }
}
