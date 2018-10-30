﻿using System;
using System.Net;

using CommonObjects.Helpers;

namespace Client.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        public Objects.ClientSettings _clientSettings;

        public String UserName { get; set; }
        public String ServerIPAddress { get; set; }

        public SettingsViewModel()
        {
            _clientSettings = new Objects.ClientSettings();
            UserName = _clientSettings.UserName;
            ServerIPAddress = _clientSettings.ServerIPAddress.ToString();
        }

        public void ApplySettings()
        {
            _clientSettings.ServerIPAddress = IPAddress.Parse(ServerIPAddress);
            _clientSettings.UserName = UserName;
            _clientSettings.SaveSettingsToFile();
        }
    }
}
