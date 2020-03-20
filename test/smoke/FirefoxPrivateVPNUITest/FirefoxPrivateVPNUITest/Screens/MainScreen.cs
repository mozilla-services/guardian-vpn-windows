﻿// <copyright file="MainScreen.cs" company="Mozilla">
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// </copyright>

namespace FirefoxPrivateVPNUITest.Screens
{
    using System;
    using System.Threading;
    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Appium.Windows;

    /// <summary>
    /// This screen model is for main screen of VPN client.
    /// </summary>
    internal class MainScreen
    {
        private AppiumWebElement titleElement;
        private AppiumWebElement subtitleElement;
        private AppiumWebElement vpnOffSettingButton;
        private AppiumWebElement vpnOnSettingButton;
        private AppiumWebElement vpnSwitch;
        private AppiumWebElement serverListButton;
        private AppiumWebElement deviceListButton;
        private AppiumWebElement vpnStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainScreen"/> class.
        /// </summary>
        /// <param name="vpnSession">VPN session.</param>
        public MainScreen(WindowsDriver<WindowsElement> vpnSession)
        {
            var mainView = Utils.WaitUntilFindElement(vpnSession.FindElementByClassName, "MainView");
            var mainCard = Utils.WaitUntilFindElement(mainView.FindElementByAccessibilityId, "MainCard");
            this.titleElement = Utils.WaitUntilFindElement(mainCard.FindElementByClassName, "HeroText");
            this.subtitleElement = Utils.WaitUntilFindElement(mainCard.FindElementByClassName, "HeroSubText");
            var settingButtons = Utils.WaitUntilFindElements(mainCard.FindElementsByName, "Settings");
            this.vpnOffSettingButton = settingButtons[0];
            this.vpnOnSettingButton = settingButtons[1];
            this.vpnSwitch = Utils.WaitUntilFindElement(mainCard.FindElementByName, "Toggle");
            this.serverListButton = Utils.WaitUntilFindElement(mainView.FindElementByAccessibilityId, "ConnectionNavButton");
            this.deviceListButton = Utils.WaitUntilFindElement(mainView.FindElementByName, "My devices");
            this.vpnStatus = Utils.WaitUntilFindElement(mainCard.FindElementByName, "VPN status");
        }

        /// <summary>
        /// Get title on main screen.
        /// </summary>
        /// <returns>The title string.</returns>
        public string GetTitle()
        {
            return this.titleElement.Text;
        }

        /// <summary>
        /// Get subtitle on main screen.
        /// </summary>
        /// <returns>The subtitle string.</returns>
        public string GetSubtitle()
        {
            return Utils.WaitUntilFindElements(this.subtitleElement.FindElementsByClassName, "TextBlock")[1].Text;
        }

        /// <summary>
        /// Click Settings button.
        /// </summary>
        public void ClickSettingsButton()
        {
            if (this.vpnOnSettingButton.Displayed)
            {
                this.vpnOnSettingButton.Click();
            }
            else
            {
                this.vpnOffSettingButton.Click();
            }
        }

        /// <summary>
        /// Toggle the VPN switch.
        /// </summary>
        public void ToggleVPNSwitch()
        {
            this.vpnSwitch.Click();

            // There is frozen state after toggle. We need to sleep at least 1.5 seconds here.
            Thread.Sleep(TimeSpan.FromSeconds(1.5));
        }

        /// <summary>
        /// Click server list button.
        /// </summary>
        public void ClickServerListButton()
        {
            this.serverListButton.Click();
        }

        /// <summary>
        /// Click device list button.
        /// </summary>
        public void ClickDeviceListButton()
        {
            this.deviceListButton.Click();
        }

        /// <summary>
        /// Get On Image.
        /// </summary>
        /// <returns>OnImage Element.</returns>
        public AppiumWebElement GetOnImage()
        {
            return Utils.WaitUntilFindElement(this.vpnStatus.FindElementByAccessibilityId, "OnImage");
        }

        /// <summary>
        /// Get Off Image.
        /// </summary>
        /// <returns>OffImage Element.</returns>
        public AppiumWebElement GetOffImage()
        {
            return Utils.WaitUntilFindElement(this.vpnStatus.FindElementByAccessibilityId, "OffImage");
        }
    }
}
