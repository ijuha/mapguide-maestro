﻿#region Disclaimer / License
// Copyright (C) 2011, Jackie Ng
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OSGeo.MapGuide.MaestroAPI.SchemaOverrides;
using OSGeo.MapGuide.ObjectModels.FeatureSource;

namespace Maestro.Editors.FeatureSource.Providers.Wms
{
    public partial class RasterDefinitionCtrl : UserControl
    {
        private RasterWmsItem _item;
        private IEditorService _edsvc;
        private IFeatureSource _fs;

        public RasterDefinitionCtrl(RasterWmsItem item, IEditorService edsvc)
        {
            InitializeComponent();
            cmbBackground.ResetColors();

            _fs = (IFeatureSource)edsvc.GetEditedResource();
            _item = item;
            _edsvc = edsvc;

            txtImageFormat.Text = item.ImageFormat;
            chkTransparent.Checked = item.IsTransparent;
            cmbBackground.CurrentColor = item.BackgroundColor;
            txtElevation.Text = item.ElevationDimension;
            txtEpsg.Text = item.SpatialContextName;
            txtTime.Text = item.Time;

            txtImageFormat.TextChanged += (s, e) => { item.ImageFormat = txtImageFormat.Text; };
            chkTransparent.CheckedChanged += (s, e) => { item.IsTransparent = chkTransparent.Checked; };
            cmbBackground.SelectedIndexChanged += (s, e) => { item.BackgroundColor = cmbBackground.CurrentColor; };
            txtElevation.TextChanged += (s, e) => { item.ElevationDimension = txtElevation.Text; };
            txtEpsg.TextChanged += (s, e) => { item.SpatialContextName = txtEpsg.Text; };
            txtTime.TextChanged += (s, e) => { item.Time = txtTime.Text; };

            List<string> names = new List<string>();
            foreach (var layer in item.Layers)
            {
                names.Add(layer.Name);
            }
            txtLayers.Lines = names.ToArray();
            lnkUpdate.Enabled = false;
        }

        private void lnkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _item.RemoveAllLayers();
            foreach (var line in txtLayers.Lines)
            {
                if (!string.IsNullOrEmpty(line) && line.Trim().Length > 0)
                {
                    _item.AddLayer(new WmsLayerDefinition(line.Trim()));
                }
            }
            MessageBox.Show(Properties.Resources.WmsLayersUpdated);
            lnkUpdate.Enabled = false;
        }

        private void txtLayers_TextChanged(object sender, EventArgs e)
        {
            lnkUpdate.Enabled = true;
        }

        private void btnSelectEpsg_Click(object sender, EventArgs e)
        {
            var conn = _fs.CurrentConnection;
            string cswkt = _edsvc.GetCoordinateSystem();
            if (!string.IsNullOrEmpty(cswkt))
            {
                //We want epsg form
                try
                {
                    txtEpsg.Text = "EPSG:" + conn.CoordinateSystemCatalog.ConvertWktToEpsgCode(cswkt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Properties.Resources.TitleError);
                }
            }
        }
    }
}