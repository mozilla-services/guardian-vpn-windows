﻿// <copyright file="ServerList.cs" company="Mozilla">
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not distributed with this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FirefoxPrivateNetwork.FxA
{
    /// <summary>
    /// Manages the VPN servers list.
    /// </summary>
    public class ServerList
    {
        private const string DefaultServerCountry = "USA";
        private List<Models.CityServerListItem> serverCityData;
        private Dictionary<int, VPNServer> vpnServers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerList"/> class.
        /// </summary>
        public ServerList()
        {
            serverCityData = new List<Models.CityServerListItem>();
            vpnServers = new Dictionary<int, VPNServer>();
        }

        /// <summary>
        /// Gets the server city list to be displayed.
        /// </summary>
        /// <returns>A list of <see cref="Models.CityServerListItem"/> objects.</returns>
        public List<Models.CityServerListItem> GetServerCitiesList()
        {
            return serverCityData;
        }

        /// <summary>
        /// Gets the server list, grouped by country, to be displayed.
        /// </summary>
        /// <returns>A list of <see cref="Models.CountryServerListItem"/> objects.</returns>
        public List<Models.CountryServerListItem> GetServerListCountryItems()
        {
            // Group the servers by country
            var countryServerList = new List<Models.CountryServerListItem>();
            var groupedCountryServerList = serverCityData
                .GroupBy(s => s.Country)
                .ToDictionary(grp => grp.Key, grp => grp.ToList());

            foreach (KeyValuePair<string, List<Models.CityServerListItem>> countryItem in groupedCountryServerList)
            {
                // Get the country flag icon
                var countryFlag = Application.Current.TryFindResource(countryItem.Key);

                countryServerList.Add(new Models.CountryServerListItem
                {
                    Country = countryItem.Key,
                    CountryFlag = countryFlag == null ? string.Empty : countryFlag.ToString(),
                    Servers = countryItem.Value,
                });
            }

            return countryServerList;
        }

        /// <summary>
        /// Gets a <see cref="VPNServer"/> by IP address (endpoint).
        /// </summary>
        /// <param name="value">IP address of the VPN server.</param>
        /// <returns>The VPN server if a matching IP address is found, otherwise null.</returns>
        public VPNServer GetServerByIP(string value)
        {
            return vpnServers.FirstOrDefault(x => x.Value.Endpoint == value).Value;
        }

        /// <summary>
        /// Gets the index within <see cref="vpnServers"/> of a VPN server by IP address (endpoint).
        /// </summary>
        /// <param name="value">IP address of the VPN server.</param>
        /// <returns>Index of the VPN server in the server list.</returns>
        public int GetServerIndexByIP(string value)
        {
            var serverIndex = 0;

            if (string.IsNullOrEmpty(value))
            {
                // Gets index of a random server in the default server country
                Random rand = new Random();
                var serversInDefaultServerCounty = vpnServers.Where(x => x.Value.Country == DefaultServerCountry).ToDictionary(x => x.Key, x => x.Value);

                if (serversInDefaultServerCounty.Count() > 0)
                {
                    serverIndex = serversInDefaultServerCounty.ElementAt(rand.Next(0, serversInDefaultServerCounty.Count)).Key;
                }
                else
                {
                    serverIndex = vpnServers.ElementAt(rand.Next(0, vpnServers.Count)).Key;
                }
            }
            else
            {
                serverIndex = vpnServers.FirstOrDefault(x => x.Value.Endpoint == value).Key;
            }

            return serverIndex;
        }

        /// <summary>
        /// Gets the index of a country within a list of servers that are grouped by country.
        /// </summary>
        /// <param name="countryServerList">List of VPN servers, grouped by country.</param>
        /// <param name="country">Country to search for.</param>
        /// <returns>Index of the country within a list of servers that are grouped by country.</returns>
        public int GetServerIndexByCountry(List<Models.CountryServerListItem> countryServerList, string country)
        {
            return countryServerList.FindIndex(x => x.Country == country);
        }

        /// <summary>
        /// Gets the list of <see cref="VPNServer"/>.
        /// </summary>
        /// <returns>List of <see cref="VPNServer"/>.</returns>
        public Dictionary<int, VPNServer> GetServerItems()
        {
            return vpnServers;
        }

        /// <summary>
        /// Gets the <see cref="VPNServer"/> within <see cref="vpnServers"/> by index.
        /// </summary>
        /// <param name="index">Index of the <see cref="VPNServer"/>.</param>
        /// <returns>The <see cref="VPNServer"/> if found, otherwise null.</returns>
        public VPNServer GetServerByIndex(int index)
        {
            try
            {
                return vpnServers[index];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the IP address with a random port of a <see cref="VPNServer"/> within <see cref="vpnServers"/>, by index.
        /// </summary>
        /// <param name="index">Index of the <see cref="VPNServer"/>.</param>
        /// <returns>IP address (endpoint) with a random port of the found <see cref="VPNServer"/>.</returns>
        public string GetServerIPByIndex(int index)
        {
            return vpnServers[index].GetEndpointWithRandomPort();
        }

        /// <summary>
        /// Gets the public key  of a <see cref="VPNServer"/> within <see cref="vpnServers"/>, by index.
        /// </summary>
        /// <param name="index">Index of the <see cref="VPNServer"/>.</param>
        /// <returns>Public key of the found <see cref="VPNServer"/>.</returns>
        public string GetServerPublicKeyByIndex(int index)
        {
            return vpnServers[index].PublicKey;
        }

        /// <summary>
        /// Retrieves the VPN server list from the FxA API.
        /// </summary>
        /// <returns>Success status of the remote server list retrieval.</returns>
        public bool RetrieveRemoteServerList()
        {
            var api = new ApiRequest(Manager.Account.Config.FxALogin.Token, "/vpn/servers", Method.GET);
            var response = api.SendRequest();
            if (response == null)
            {
                return false;
            }

            var contents = response.Content;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            // Saves the retrieved server list to a file in the user's app data folder
            try
            {
                File.WriteAllText(Path.Combine(ProductConstants.UserAppDataFolder, "servers.json"), contents);
            }
            catch (Exception e)
            {
                ErrorHandling.ErrorHandler.Handle(e, ErrorHandling.LogLevel.Error);
                return false;
            }

            LoadServerDataFromFile(Path.Combine(ProductConstants.UserAppDataFolder, "servers.json"));

            return true;
        }

        /// <summary>
        /// Loads the server list from a file.
        /// </summary>
        /// <param name="filePath">Path of the file containing the server list.</param>
        public void LoadServerDataFromFile(string filePath)
        {
            string jsonStringData = string.Empty;
            JSONStructures.ServerList servers;

            try
            {
                jsonStringData = File.ReadAllText(filePath);
                JObject jObjectServerData = JObject.Parse(jsonStringData);
                servers = JsonConvert.DeserializeObject<JSONStructures.ServerList>(jObjectServerData.ToString());
            }
            catch (Exception)
            {
                return;
            }

            var newVPNServers = new Dictionary<int, VPNServer>();

            var serverIndex = 0;
            foreach (var country in servers.Countries)
            {
                foreach (var city in country.Cities)
                {
                    try
                    {
                        foreach (var server in city.Servers)
                        {
                            var portRanges = new List<List<int>> { new List<int> { 53, 53 } };
                            try
                            {
                                portRanges = new List<List<int>>();

                                // Port range parsing
                                foreach (var portRange in server.PortRanges)
                                {
                                    portRanges.Add(new List<int> { portRange.First(), portRange.Last() });
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorHandling.ErrorHandler.Handle(e, ErrorHandling.LogLevel.Debug);
                            }

                            var newVPNServer = new VPNServer
                            {
                                IPv4Address = server.IPv4Gateway,
                                IPv6Address = server.IPv6Gateway,
                                Endpoint = server.IPv4EndpointAddress,
                                DNSServerAddress = server.IPv4Gateway,
                                Ports = portRanges,
                                PublicKey = server.PublicKey,
                                Weight = server.Weight,
                                Name = server.Hostname,
                                Country = country.Name,
                                City = city.Name,
                            };

                            newVPNServers[serverIndex] = newVPNServer;
                            serverIndex++;
                        }
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }
            }

            // Sort the server list
            newVPNServers = SortServerList(newVPNServers);
            List<Models.CityServerListItem> newServerData = new List<Models.CityServerListItem>();

            var groupedServersByCity = newVPNServers.GroupBy(s => s.Value.City).ToDictionary(grp => grp.Key, grp => grp.ToList());

            foreach (var serverCity in groupedServersByCity)
            {
                List<Models.ServerListItem> cityServers = new List<Models.ServerListItem>();

                for (int i = 0; i < serverCity.Value.Count; i++)
                {
                    cityServers.Add(new Models.ServerListItem
                    {
                        Country = serverCity.Value[i].Value.Country,
                        City = serverCity.Value[i].Value.City,
                        Name = serverCity.Value.Count > 1 ? string.Format("{0} {1}", serverCity.Value[i].Value.City, i + 1) : serverCity.Value[i].Value.City,
                        Code = serverCity.Value[i].Value.Name,
                        Endpoint = serverCity.Value[i].Value.Endpoint,
                    });
                }

                newServerData.Add(new Models.CityServerListItem
                {
                    Country = serverCity.Value[0].Value.Country,
                    City = serverCity.Key,
                    Servers = cityServers,
                });
            }

            serverCityData = newServerData;
            vpnServers = newVPNServers;
        }

        /// <summary>
        /// Sorts the server list by country, city and weight.
        /// </summary>
        /// <param name="vpnServers">VPN servers to be sorted.</param>
        /// <returns>Sorted list of VPN servers.</returns>
        public Dictionary<int, VPNServer> SortServerList(Dictionary<int, VPNServer> vpnServers)
        {
            var sortedServerList = from server in vpnServers orderby server.Value.Country ascending, server.Value.City ascending, server.Value.Weight descending select server;
            return sortedServerList.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Chooses a server from the selected server city according to server weights.
        /// </summary>
        /// <param name="serverCityItem">Selected server city for connection.</param>
        /// <returns>VPN server selected for connection.</returns>
        public VPNServer SelectServer(Models.CityServerListItem serverCityItem)
        {
            var city = serverCityItem.City;

            Dictionary<int, VPNServer> servers = new Dictionary<int, VPNServer>();
            servers = GetServerItems().Where(x => x.Value.City == city).ToDictionary(x => x.Key, x => x.Value);

            var weightSum = servers.Sum(server => server.Value.Weight);

            Random rand = new Random();
            int r = rand.Next(0, weightSum + 1);

            VPNServer serverSelection = new VPNServer();

            foreach (var server in servers)
            {
                r -= server.Value.Weight;
                if (r <= 0)
                {
                    serverSelection = server.Value;
                    break;
                }
            }

            return serverSelection;
        }
    }
}
