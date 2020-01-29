﻿// <copyright file="CityServerListItem.cs" company="Mozilla">
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// </copyright>

using System.Collections.Generic;

namespace FirefoxPrivateNetwork.Models
{
    /// <summary>
    /// City wrapper for servers in the server list.
    /// </summary>
    public class CityServerListItem
    {
        /// <summary>
        /// Gets or sets the country name.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the City name.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the server list.
        /// </summary>
        public List<ServerListItem> Servers { get; set; }

        /// <summary>
        /// Gets the name of the country.
        /// </summary>
        /// <returns>
        /// Returns the name of the country.
        /// </returns>
        public override string ToString()
        {
            return this.Country;
        }
    }
}
