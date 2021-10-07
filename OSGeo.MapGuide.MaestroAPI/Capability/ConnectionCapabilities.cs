﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
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

#endregion Disclaimer / License

using OSGeo.MapGuide.MaestroAPI.Exceptions;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSGeo.MapGuide.MaestroAPI.Capability
{
    /// <summary>
    /// Base connection capabilitiy class
    /// </summary>
    public abstract class ConnectionCapabilities : IConnectionCapabilities
    {
        /// <summary>
        /// Resource types supported on *all* versions of MapGuide
        /// </summary>
        private static readonly string[] _defaultResTypes = {
            //ResourceTypes.RuntimeMap.ToString(),
            //ResourceTypes.Selection.ToString(),
            //ResourceTypes.Folder.ToString(),

            ResourceTypes.DrawingSource.ToString(),
            ResourceTypes.FeatureSource.ToString(),
            ResourceTypes.LayerDefinition.ToString(),
            ResourceTypes.LoadProcedure.ToString(),
            ResourceTypes.MapDefinition.ToString(),
            ResourceTypes.PrintLayout.ToString(),
            ResourceTypes.SymbolLibrary.ToString(),
            ResourceTypes.WebLayout.ToString()
        };

        /// <summary>
        /// The parent connection
        /// </summary>
        protected IServerConnection _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionCapabilities"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected ConnectionCapabilities(IServerConnection parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Gets the highest supported resource version.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public virtual Version GetMaxSupportedResourceVersion(string resourceType)
        {
            Version ver = new Version(1, 0, 0);
            switch (resourceType)
            {
                case nameof(ResourceTypes.ApplicationDefinition):
                    if (!SupportsFusion())
                        throw new UnsupportedResourceTypeException(nameof(ResourceTypes.ApplicationDefinition));
                    break;

                case nameof(ResourceTypes.WatermarkDefinition):
                    ver = GetMaxWatermarkDefinitionVersion();
                    break;

                case nameof(ResourceTypes.MapDefinition):
                    ver = GetMaxMapDefinitionVersion();
                    break;

                case nameof(ResourceTypes.LayerDefinition):
                    ver = GetMaxLayerDefinitionVersion();
                    break;

                case nameof(ResourceTypes.LoadProcedure):
                    ver = GetMaxLoadProcedureVersion();
                    break;

                case nameof(ResourceTypes.WebLayout):
                    ver = GetMaxWebLayoutVersion();
                    break;

                case nameof(ResourceTypes.SymbolDefinition):
                    if (!SupportsAdvancedSymbols())
                        throw new UnsupportedResourceTypeException(nameof(ResourceTypes.SymbolDefinition));
                    else
                        ver = GetMaxSymbolDefinitionVersion();
                    break;
            }
            return ver;
        }

        /// <summary>
        /// Supportses the advanced symbols.
        /// </summary>
        /// <returns></returns>
        protected virtual bool SupportsAdvancedSymbols() => (_parent.SiteVersion >= new Version(1, 2));

        /// <summary>
        /// Supportses the fusion.
        /// </summary>
        /// <returns></returns>
        protected virtual bool SupportsFusion() => (_parent.SiteVersion >= new Version(2, 0));

        /// <summary>
        /// Gets the max watermark definition version
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxWatermarkDefinitionVersion()
        {
            if (_parent.SiteVersion >= new Version(2, 4))
                return new Version(2, 4, 0);
            return new Version(2, 3, 0);
        }

        /// <summary>
        /// Gets the max load procedure version.
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxLoadProcedureVersion()
        {
            if (_parent.SiteVersion >= new Version(2, 2))
                return new Version(2, 2, 0);

            if (_parent.SiteVersion >= new Version(2, 0))
                return new Version(1, 1, 0);

            return new Version(1, 0, 0);
        }

        /// <summary>
        /// Gets the max symbol definition version.
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxSymbolDefinitionVersion()
        {
            if (_parent.SiteVersion >= new Version(2, 4))
                return new Version(2, 4, 0);
            if (_parent.SiteVersion >= new Version(2, 0))
                return new Version(1, 1, 0);

            return new Version(1, 0, 0);
        }

        /// <summary>
        /// Gets the max web layout version.
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxWebLayoutVersion()
        {
            if (_parent.SiteVersion >= new Version(2, 6))
                return new Version(2, 6, 0);
            if (_parent.SiteVersion >= new Version(2, 4))
                return new Version(2, 4, 0);
            if (_parent.SiteVersion >= new Version(2, 2))
                return new Version(1, 1, 0);
            return new Version(1, 0, 0);
        }

        /// <summary>
        /// Gets the max map definition version
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxMapDefinitionVersion()
        {
            if (_parent.SiteVersion >= new Version(2, 4))
                return new Version(2, 4, 0);
            if (_parent.SiteVersion >= new Version(2, 3))
                return new Version(2, 3, 0);

            return new Version(1, 0, 0);
        }

        /// <summary>
        /// Gets the max layer definition version.
        /// </summary>
        /// <returns></returns>
        protected virtual Version GetMaxLayerDefinitionVersion()
        {
            if (_parent.SiteVersion >= new Version(4, 0))
                return new Version(4, 0, 0);
            if (_parent.SiteVersion >= new Version(2, 4))
                return new Version(2, 4, 0);
            if (_parent.SiteVersion >= new Version(2, 3))
                return new Version(2, 3, 0);
            if (_parent.SiteVersion >= new Version(2, 1))
                return new Version(1, 3, 0);
            if (_parent.SiteVersion >= new Version(2, 0))
                return new Version(1, 2, 0);
            if (_parent.SiteVersion >= new Version(1, 2))
                return new Version(1, 1, 0);

            return new Version(1, 0, 0);
        }

        /// <summary>
        /// Gets an array of supported commands
        /// </summary>
        /// <value></value>
        public abstract int[] SupportedCommands
        {
            get;
        }

        /// <summary>
        /// Gets an array of supported services
        /// </summary>
        /// <value></value>
        public virtual int[] SupportedServices
        {
            get
            {
                if (_parent.SiteVersion >= new Version(2, 0))
                {
                    return new int[] {
                        (int)ServiceType.Resource,
                        (int)ServiceType.Feature,
                        (int)ServiceType.Fusion,
                        (int)ServiceType.Mapping,
                        (int)ServiceType.Tile,
                        (int)ServiceType.Drawing,
                        (int)ServiceType.Site
                    };
                }
                else //Fusion doesn't exist pre-2.0
                {
                    return new int[] {
                        (int)ServiceType.Resource,
                        (int)ServiceType.Feature,
                        (int)ServiceType.Mapping,
                        (int)ServiceType.Tile,
                        (int)ServiceType.Drawing,
                        (int)ServiceType.Site
                    };
                }
            }
        }

        /// <summary>
        /// Indicates whether web-based previewing capabilities are possible with this connection
        /// </summary>
        /// <value></value>
        public abstract bool SupportsResourcePreviews
        {
            get;
        }

        /// <summary>
        /// Indicates whether the current connection can be used between multiple threads
        /// </summary>
        /// <value></value>
        public abstract bool IsMultithreaded
        {
            get;
        }

        /// <summary>
        /// Indicates if this current connection supports the specified resource type
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public virtual bool IsSupportedResourceType(string resourceType)
        {
            Check.ArgumentNotEmpty(resourceType, nameof(resourceType));

            return Array.IndexOf(this.SupportedResourceTypes, resourceType) >= 0;
        }

        /// <summary>
        /// Gets whether this connection supports publishing resources for WFS
        /// </summary>
        public virtual bool SupportsWfsPublishing => true;

        /// <summary>
        /// Gets whether this connection supports publishing resources for WMS
        /// </summary>
        public virtual bool SupportsWmsPublishing => true;

        /// <summary>
        /// Gets whether this connection supports resource reference tracking
        /// </summary>
        public virtual bool SupportsResourceReferences => true;

        /// <summary>
        /// Gets whether this connection supports resource security
        /// </summary>
        public virtual bool SupportsResourceSecurity => true;

        /// <summary>
        /// Gets whether this connection supports the concept of resource headers
        /// </summary>
        public virtual bool SupportsResourceHeaders => true;

        /// <summary>
        /// Gets the array of supported resource types
        /// </summary>
        public string[] SupportedResourceTypes
        {
            get
            {
                var ver = _parent.SiteVersion;
                var types = new HashSet<string>(_defaultResTypes);

                if (ver >= new Version(1, 2))
                    types.Add(ResourceTypes.SymbolDefinition.ToString());
                if (ver >= new Version(2, 0))
                    types.Add(ResourceTypes.ApplicationDefinition.ToString());
                if (ver >= new Version(2, 3))
                    types.Add(ResourceTypes.WatermarkDefinition.ToString());
                if (ver >= new Version(3, 0))
                    types.Add(ResourceTypes.TileSetDefinition.ToString());

                //When new types are introduced to MapGuide and we wish to add such support, put the new types here
                return types.OrderBy(x => x).ToArray();
            }
        }
    }
}