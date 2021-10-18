﻿#region Disclaimer / License

// Copyright (C) 2017, Jackie Ng
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

using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Mapping;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Maestro.Editors.Preview
{
    internal partial class MapPreviewViewContent : ViewContentBase
    {
        private readonly RuntimeMap _map;
        private readonly IUrlLauncherService _launcher;
        private readonly IServerConnection _conn;
        private readonly string _resourceId;
        
        public MapPreviewViewContent(RuntimeMap map, IUrlLauncherService urlLauncher, string resourceId)
        {
            InitializeComponent();
            _map = map;
            _resourceId = resourceId;
            txtCoordinateSystem.Text = map.CoordinateSystem;
            numZoomToScale.Minimum = 1;
            numZoomToScale.Maximum = Int32.MaxValue;
            _launcher = urlLauncher;
            _conn = map.CurrentConnection;
            btnGetMapKml.Enabled = (_conn.ProviderName.ToUpper() == "MAESTRO.HTTP"); //NOXLATE
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(_resourceId))
                this.Title = $"{Strings.ResourcePreviewTitle} - {_resourceId}"; //NOXLATE
            this.Description = _resourceId;
            if (_map.Layers.Count == 1) //Single-layer preview
            {
                var maxScale = _map.Layers.Max(x => x.ScaleRanges.Max(y => y.MaxScale));
                var initScale = Math.Floor(maxScale - 0.5);
                var env = _map.MapExtent;
                double mcsW = env.MaxX - env.MinX;
                double mcsH = env.MaxY - env.MinY;
                var bboxScale = Maestro.MapViewer.MapViewer.CalculateScale(_map, mcsW, mcsH, mapViewer.Width, mapViewer.Height);
                double desiredScale = Math.Min(initScale, bboxScale);
                mapViewer.LoadMap(_map, desiredScale);
            }
            else
            {
                mapViewer.LoadMap(_map);
            }
        }

        private void mapViewer_MapScaleChanged(object sender, EventArgs e)
        {
            numZoomToScale.Value = Convert.ToDecimal(mapViewer.GetMap().ViewScale);
        }

        private void lnkZoomToScale_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mapViewer.ZoomToScale(Convert.ToDouble(numZoomToScale.Value));
        }

        private void mapViewer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mapViewer.IsBusy))
            {
                grpOtherTools.Enabled = !mapViewer.IsBusy;

                if (!mapViewer.IsBusy)
                {
                    double minx, miny, maxx, maxy;
                    mapViewer.GetViewExtent(out minx, out miny, out maxx, out maxy);

                    txtMinX.Text = minx.ToString(CultureInfo.InvariantCulture);
                    txtMinY.Text = miny.ToString(CultureInfo.InvariantCulture);
                    txtMaxX.Text = maxx.ToString(CultureInfo.InvariantCulture);
                    txtMaxY.Text = maxy.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void btnGetMapKml_Click(object sender, EventArgs e)
        {
            var mapagent = _conn.GetCustomProperty("BaseUrl").ToString(); //NOXLATE
            mapagent += "mapagent/mapagent.fcgi?SESSION=" + _conn.SessionID + "&VERSION=1.0.0&OPERATION=GETMAPKML&DPI=96&MAPDEFINITION=" + _map.MapDefinition + "&FORMAT=KML&CLIENTAGENT=Maestro Local Map Previewer"; //NOXLATE
            _launcher.OpenUrl(mapagent);
        }

        public override ViewRegion DefaultRegion => ViewRegion.Document;

        private void mapViewer_MapSelectionAttributesChanged(object sender, OSGeo.MapGuide.ObjectModels.SelectionModel.FeatureInformation model)
        {
            selectionPanel1.SelectedFeatureAttributes = model;
        }
    }
}
