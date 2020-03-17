﻿// <copyright file="FirefoxPrivateVPNSession.cs" company="Mozilla">
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// </copyright>

namespace FirefoxPrivateVPNUITest
{
    using System;
    using FirefoxPrivateVPNUITest.Screens;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Appium.Windows;
    using OpenQA.Selenium.Remote;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Firefox Private VPN session.
    /// </summary>
    public class FirefoxPrivateVPNSession : BaseSession
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private const string FirefoxPrivateVPNAppId = @"C:\Program Files\Mozilla\Firefox Private Network VPN\FirefoxPrivateNetworkVPN.exe";

        /// <summary>
        /// Initializes a new instance of the <see cref="FirefoxPrivateVPNSession"/> class.
        /// </summary>
        public FirefoxPrivateVPNSession()
        {
            if (this.Session == null)
            {
                // Create a new session to bring up an instance of the FirefoxPrivateNetworkVPN application
                DesiredCapabilities appCapabilities = new DesiredCapabilities();
                appCapabilities.SetCapability("app", FirefoxPrivateVPNAppId);
                appCapabilities.SetCapability("deviceName", "WindowsPC");

                try
                {
                    this.Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities, TimeSpan.FromSeconds(Constants.SessionTimeoutInSeconds));
                    Assert.IsNotNull(this.Session);
                }
                catch (Exception)
                {
                    // 1. Creating a Desktop session
                    var desktopSession = new DesktopSession();
                    WebDriverWait wait = new WebDriverWait(desktopSession.Session, TimeSpan.FromSeconds(30));
                    var firefoxVPN = wait.Until(ExpectedConditions.ElementExists(By.Name("Firefox Private Network VPN")));

                    // 2. Attaching to existing firefox Window
                    string applicationSessionHandle = firefoxVPN.GetAttribute("NativeWindowHandle");
                    applicationSessionHandle = int.Parse(applicationSessionHandle).ToString("x");

                    appCapabilities.SetCapability("app", null);
                    appCapabilities.SetCapability("appTopLevelWindow", applicationSessionHandle);
                    this.Session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities, TimeSpan.FromSeconds(Constants.SessionTimeoutInSeconds));
                    Assert.IsNotNull(this.Session);
                }

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                this.Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            }
        }

        /// <summary>
        /// Dispose the VPN session and close the app.
        /// </summary>
        public void Dispose()
        {
            // Close the application and delete the session
            if (this.Session != null)
            {
                this.Session.SwitchTo();
                try
                {
                    WindowsElement landingView = this.Session.FindElementByClassName("LandingView");
                }
                catch (InvalidOperationException)
                {
                    UserCommonOperation.UserSignOut(this);
                }

                this.Session.Quit();
                var desktopSession = new DesktopSession();
                desktopSession.CloseVPNClient();
                this.Session = null;
            }
        }
    }
}
