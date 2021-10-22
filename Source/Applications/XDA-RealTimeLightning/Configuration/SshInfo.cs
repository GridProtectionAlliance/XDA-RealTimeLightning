//******************************************************************************************************
//  RTLInfo.cs - Gbtc
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
    public class SshInfo
    {
        public SshInfo(XDocument configuration)
        {
            Host = (string)configuration
                .Descendants(nameof(SshInfo))
                .Elements(nameof(Host))
                .FirstOrDefault();

            User = (string)configuration
                .Descendants(nameof(SshInfo))
                .Elements(nameof(User))
                .FirstOrDefault();

            KeyFile = (string)configuration
                .Descendants(nameof(SshInfo))
                .Elements(nameof(KeyFile))
                .FirstOrDefault();

            PassPhrase = (string)configuration
                .Descendants(nameof(SshInfo))
                .Elements(nameof(PassPhrase))
                .FirstOrDefault();
        }

        public string Host { get; }
        public string User { get; }
        public string KeyFile { get; }
        public string PassPhrase { get; }
    }
}
