//******************************************************************************************************
//  TunnelInfo.cs - Gbtc
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
//  10/22/2021 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Linq;
using System.Xml.Linq;

namespace XDARTL.Configuration
{
    public class TunnelInfo
    {
        public TunnelInfo(XDocument configuration)
        {
            LocalHost = (string)configuration
                .Descendants(nameof(TunnelInfo))
                .Elements(nameof(LocalHost))
                .FirstOrDefault();

            string localPortText = (string)configuration
                .Descendants(nameof(TunnelInfo))
                .Elements(nameof(LocalPort))
                .FirstOrDefault();

            RemoteHost = (string)configuration
                .Descendants(nameof(TunnelInfo))
                .Elements(nameof(RemoteHost))
                .FirstOrDefault();

            string remotePortText = (string)configuration
                .Descendants(nameof(TunnelInfo))
                .Elements(nameof(RemotePort))
                .FirstOrDefault();

            if (ushort.TryParse(localPortText, out ushort localPort))
                LocalPort = localPort;

            if (ushort.TryParse(remotePortText, out ushort remotePort))
                RemotePort = remotePort;
        }

        public string LocalHost { get; } = "localhost";
        public ushort LocalPort { get; } = 21792;
        public string RemoteHost { get; } = "127.0.0.1";
        public ushort RemotePort { get; } = 21792;
    }
}
