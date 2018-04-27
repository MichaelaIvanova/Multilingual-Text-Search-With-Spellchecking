﻿using System.Configuration;

namespace Example.BusinessLogic.Helpers
{
    public class AppSettingsHelper : IAppSettingsHelper
    {
        public string GetValue(string paramName)
        {
            string value = string.Empty;

            try
            {
                value = ConfigurationManager.AppSettings[paramName];
            }
            catch (ConfigurationErrorsException ex)
            {
               // new ErrorLogger().Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, ex.Message, ex);
            }

            return value;
        }
    }
}