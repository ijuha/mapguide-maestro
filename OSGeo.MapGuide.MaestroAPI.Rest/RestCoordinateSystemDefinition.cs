﻿#region Disclaimer / License
// Copyright (C) 2014, Jackie Ng
// http://trac.osgeo.org/mapguide/wiki/maestro, jumpinjackie@gmail.com
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
#endregion
using OSGeo.MapGuide.MaestroAPI.CoordinateSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OSGeo.MapGuide.MaestroAPI.Rest
{
    public class RestCoordinateSystemDefinition : CoordinateSystemDefinitionBase
    {
        internal RestCoordinateSystemDefinition() { }

        internal RestCoordinateSystemDefinition(CoordinateSystemCategory parent, XmlNode topnode)
            : base(parent)
        {
            foreach (XmlNode node in topnode.ChildNodes)
            {
                switch (node["Name"].InnerText.ToLower())
                {
                    case "code":
                        m_code = node["Value"].InnerText;
                        break;
                    case "description":
                        m_description = node["Value"].InnerText;
                        break;
                    case "projection":
                        m_projection = node["Value"].InnerText;
                        break;
                    case "projection description":
                        m_projectionDescription = node["Value"].InnerText;
                        break;
                    case "Datum":
                        m_datum = node["Value"].InnerText;
                        break;
                    case "datum description":
                        m_datumDescription = node["Value"].InnerText;
                        break;
                    case "ellipsoid":
                        m_ellipsoid = node["Value"].InnerText;
                        break;
                    case "ellipsoid description":
                        m_ellipsoidDescription = node["Value"].InnerText;
                        break;
                }
            }
        }
    }
}