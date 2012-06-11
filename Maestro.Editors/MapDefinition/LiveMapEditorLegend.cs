﻿#region Disclaimer / License
// Copyright (C) 2012, Jackie Ng
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Maestro.MapViewer;
using OSGeo.MapGuide.MaestroAPI.Services;
using Maestro.Editors.Generic;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Mapping;
using OSGeo.MapGuide.MaestroAPI.Resource;
using OSGeo.MapGuide.ObjectModels.LayerDefinition;

namespace Maestro.Editors.MapDefinition
{
    /// <summary>
    /// Description of LiveMapEditorLegend.
    /// </summary>
    public partial class LiveMapEditorLegend : UserControl
    {
        public LiveMapEditorLegend()
        {
            InitializeComponent();
            legendCtrl.NodeSelected += new NodeEventHandler(OnInnerNodeSelected);
        }
        
        private void OnInnerNodeSelected(object sender, TreeNode e)
        {
            var h = this.NodeSelected;
            if (h != null)
                h(this, e);
        }

        public event NodeEventHandler NodeDeleted;

        public event NodeEventHandler NodeSelected;
        
        public IMapViewer Viewer
        {
            get { return legendCtrl.Viewer; }
            set { legendCtrl.Viewer = value; }
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            var map = this.Viewer.GetMap();
            if (map != null)
            {
                var diag = new Live.NewGroupDialog(map);
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
                    var group = mapSvc.CreateMapGroup(map, diag.GroupName);
                    group.LegendLabel = diag.GroupLabel;
                    group.Visible = true;
                    group.ShowInLegend = true;
                    map.Groups.Add(group);
                    legendCtrl.Viewer.RefreshMap();
                }
            }
        }

        private static string GenerateUniqueName(string prefix, RuntimeMapLayerCollection layers)
        {
            int counter = 0;
            string name = prefix;
            while (layers[name] != null)
            {
                counter++;
                name = prefix + counter;
            }
            return name;
        }

        private void btnAddLayer_Click(object sender, EventArgs e)
        {
            var map = this.Viewer.GetMap();
            if (map != null)
            {
                using (var picker = new ResourcePicker(map.CurrentConnection.ResourceService, ResourceTypes.LayerDefinition, ResourcePickerMode.OpenResource))
                {
                    if (picker.ShowDialog() == DialogResult.OK)
                    {
                        var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
                        var layer = mapSvc.CreateMapLayer(map, ((ILayerDefinition)map.CurrentConnection.ResourceService.GetResource(picker.ResourceID)));
                        layer.Name = GenerateUniqueName(ResourceIdentifier.GetName(picker.ResourceID), map.Layers);
                        layer.LegendLabel = ResourceIdentifier.GetName(picker.ResourceID);
                        layer.Visible = true;
                        layer.ShowInLegend = true;
                        map.Layers.Insert(0, layer);
                        legendCtrl.Viewer.RefreshMap();
                    }
                }
            }
        }

        private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var map = this.Viewer.GetMap();
            if (map != null)
            {
                var node = legendCtrl.SelectedNode;
                if (node != null)
                {
                    var grp = node.Tag as Legend.GroupNodeMetadata;
                    if (grp != null)
                    {
                        using (var picker = new ResourcePicker(map.CurrentConnection.ResourceService, ResourceTypes.LayerDefinition, ResourcePickerMode.OpenResource))
                        {
                            if (picker.ShowDialog() == DialogResult.OK)
                            {
                                var mapSvc = (IMappingService)map.CurrentConnection.GetService((int)ServiceType.Mapping);
                                var layer = mapSvc.CreateMapLayer(map, ((ILayerDefinition)map.CurrentConnection.ResourceService.GetResource(picker.ResourceID)));
                                layer.Name = GenerateUniqueName(ResourceIdentifier.GetName(picker.ResourceID), map.Layers);
                                layer.LegendLabel = ResourceIdentifier.GetName(picker.ResourceID);
                                layer.Group = grp.Name;
                                layer.Visible = true;
                                layer.ShowInLegend = true;
                                map.Layers.Insert(0, layer);
                                legendCtrl.Viewer.RefreshMap();
                            }
                        }
                    }
                }
            }
        }

        private void removeThisGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var map = this.Viewer.GetMap();
            if (map != null)
            {
                var node = legendCtrl.SelectedNode;
                if (node != null)
                {
                    var grp = node.Tag as Legend.GroupNodeMetadata;
                    if (grp != null)
                    {
                        var group = map.Groups[grp.Group];
                        if (group != null)
                        {
                            map.Groups.Remove(group);
                            legendCtrl.Viewer.RefreshMap();
                        }
                    }
                }
            }
        }

        private void removeThisLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var map = this.Viewer.GetMap();
            if (map != null)
            {
                var node = legendCtrl.SelectedNode;
                if (node != null)
                {
                    var lyr = node.Tag as Legend.LayerNodeMetadata;
                    if (lyr != null && !lyr.IsThemeRule)
                    {
                        var layer = map.Layers[lyr.Name];
                        if (layer != null)
                        {
                            map.Layers.Remove(layer);
                            legendCtrl.Viewer.RefreshMap();
                        }
                    }
                }
            }
        }
    }
}