//******************************************************************************************************
//  ConfigurationManager.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/15/2021 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Linq;
using System.Xml.Linq;

namespace XDARTL.Configuration
{
    public class ConfigurationManager
    {
        private XDocument Configuration { get; set; }

        public void Reload()
        {
            Configuration = GetConfiguration();
        }

        private XDocument GetConfiguration()
        {
            return XDocument.Load(@"xda-rtl-config.xml");
        }

        private object GetRTLInfo(XDocument configuration)
        {
            string connectionString = (string)configuration
                .Descendants("RTLInfo")
                .Elements("ConnectionString")
                .FirstOrDefault();

            string eventPollingMinutes = (string)configuration
                .Descendants("RTLInfo")
                .Elements("EventPollingMinutes")
                .FirstOrDefault();

            string trendPollingHours = (string)configuration
                .Descendants("RTLInfo")
                .Elements("TrendPollingHours")
                .FirstOrDefault();

            if (!double.TryParse(eventPollingMinutes, out double doubleEventPollingMinutes))
                doubleEventPollingMinutes = 5;

            if (!double.TryParse(trendPollingHours, out double doubleTrendPollingHours))
                doubleTrendPollingHours = 12;

            return new object();
        }
    }
}
