﻿#region Disclaimer / License

// Copyright (C) 2011, Jackie Ng
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

using Maestro.Editors.Preview;
using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.MaestroAPI.SchemaOverrides;
using OSGeo.MapGuide.ObjectModels.Common;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Maestro.Editors.FeatureSource.Providers.Wms
{
    internal partial class WmsAdvancedConfigurationDialog : Form
    {
        private IEditorService _service;
        private WmsConfigurationDocument _config;
        private IFeatureSource _fs;
        readonly BindingList<RasterWmsItem> _items;
        public WmsConfigurationDocument Document { get { return _config; } }

        public WmsAdvancedConfigurationDialog(IEditorService service)
        {
            InitializeComponent();
            _items = new BindingList<RasterWmsItem>();
            grdSpatialContexts.AutoGenerateColumns = false;
            _service = service;
            _fs = (IFeatureSource)_service.GetEditedResource();
            txtFeatureServer.Text = _fs.GetConnectionProperty("FeatureServer"); //NOXLATE
            string xml = _fs.GetConfigurationContent(service.CurrentConnection);
            if (!string.IsNullOrEmpty(xml))
            {
                try
                {
                    _config = (WmsConfigurationDocument)ConfigurationDocument.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Strings.ErrorLoadingWmsConfig, ex.Message), Strings.TitleError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MakeDefaultDocument();
                }
            }
            else
            {
                MakeDefaultDocument();
            }

            foreach (var klass in _config.RasterOverrides)
            {
                _items.Add(klass);
            }

            lstFeatureClasses.DataSource = _items;
            grdSpatialContexts.DataSource = _config.SpatialContexts;
        }

        private WmsConfigurationDocument BuildDefaultWmsDocument()
        {
            var doc = new WmsConfigurationDocument();
            var contexts = _service.CurrentConnection.FeatureService.GetSpatialContextInfo(_fs.ResourceID, false);
            var schemaName = _service.CurrentConnection.FeatureService.GetSchemas(_fs.ResourceID)[0];
            var clsNames = _service.CurrentConnection.FeatureService.GetClassNames(_fs.ResourceID, schemaName);
            var schema = new FeatureSchema(schemaName, string.Empty);
            doc.AddSchema(schema);

            foreach (var sc in contexts.SpatialContext)
            {
                doc.AddSpatialContext(sc);
            }

            var defaultSc = contexts.SpatialContext[0];

            foreach (var clsName in clsNames)
            {
                var className = clsName.Split(':')[1]; //NOXLATE
                var cls = new ClassDefinition(className, string.Empty);
                cls.AddProperty(new DataPropertyDefinition("Id", string.Empty) //NOXLATE
                {
                    DataType = DataPropertyType.String,
                    Length = 256,
                    IsNullable = false
                }, true);
                cls.AddProperty(new RasterPropertyDefinition("Image", string.Empty) //NOXLATE
                {
                    DefaultImageXSize = 1024,
                    DefaultImageYSize = 1024,
                    SpatialContextAssociation = defaultSc.Name
                });

                schema.AddClass(cls);

                var item = CreateDefaultItem(schema.Name, cls.Name, "Image", defaultSc); //NOXLATE
                doc.AddRasterItem(item);
            }

            return doc;
        }

        private static RasterWmsItem CreateDefaultItem(string schemaName, string clsName, string rasName, IFdoSpatialContext defaultSc)
        {
            var item = new RasterWmsItem(schemaName, clsName, rasName);
            item.ImageFormat = "PNG"; //NOXLATE
            item.IsTransparent = true;
            item.BackgroundColor = Color.White;
            item.SpatialContextName = defaultSc.Name;
            item.UseTileCache = false;
            item.AddLayer(new WmsLayerDefinition(clsName) { Style = "default" }); //NOXLATE
            return item;
        }

        private static RasterPropertyDefinition GetRasterProperty(ClassDefinition cls)
        {
            foreach (var prop in cls.Properties)
            {
                if (prop.Type == OSGeo.MapGuide.MaestroAPI.Schema.PropertyDefinitionType.Raster)
                    return (RasterPropertyDefinition)prop;
            }
            return null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _fs.SetConfigurationContent(_service.CurrentConnection, _config.ToXml());
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private ClassDefinition _logicalClass;
        private bool _updatingLogicalClassUI = false;

        private void lstFeatureClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdating)
                return;

            _updatingLogicalClassUI = true;
            try
            {
                var item = (RasterWmsItem)lstFeatureClasses.SelectedItem;
                if (item != null && lstFeatureClasses.SelectedItems.Count == 1)
                {
                    RasterDefinitionCtrl ctrl = null;
                    if (grpRaster.Controls.Count == 1)
                        ctrl = grpRaster.Controls[0] as RasterDefinitionCtrl;

                    if (ctrl == null)
                    {
                        grpRaster.Controls.Clear();
                        ctrl = new RasterDefinitionCtrl(_config, item, _service);
                        ctrl.Dock = DockStyle.Fill;
                        grpRaster.Controls.Add(ctrl);
                    }
                    else
                    {
                        ctrl.BindItem(item);
                    }

                    btnRemove.Enabled = true;

                    //Get logical class
                    string schemaName = item.SchemaName;
                    string className = item.FeatureClass;

                    if (!string.IsNullOrEmpty(schemaName) && !string.IsNullOrEmpty(className))
                    {
                        _logicalClass = _config.GetClass(schemaName, className);
                        if (_logicalClass != null)
                        {
                            txtClassName.Text = _logicalClass.Name;
                            txtClassDescription.Text = _logicalClass.Description;
                        }
                        else
                        {
                            txtClassName.Text = string.Empty;
                            txtClassDescription.Text = string.Empty;
                        }
                    }
                    else
                    {
                        _logicalClass = null;
                        txtClassName.Text = string.Empty;
                        txtClassDescription.Text = string.Empty;
                    }
                }
                else
                {
                    grpRaster.Controls.Clear();
                    _logicalClass = null;
                    txtClassName.Text = string.Empty;
                    txtClassDescription.Text = string.Empty;
                }
            }
            finally
            {
                _updatingLogicalClassUI = false;
            }
            grpLogicalClass.Enabled = (_logicalClass != null);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var selected = new List<RasterWmsItem>();
            foreach (var selItem in lstFeatureClasses.SelectedItems)
            {
                if (selItem is RasterWmsItem item)
                    selected.Add(item);
            }
            try
            {
                this.BeginUpdate();
                foreach (var item in selected)
                {
                    if (_items.Remove(item))
                    {
                        //Remove schema mapping item
                        _config.RemoveRasterItem(item);

                        //Remove mapped class from logical schema
                        var schema = _config.Schemas[0];
                        schema.RemoveClass(item.FeatureClass);
                    }
                }
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            /*
            var name = GenericInputDialog.GetValue(Properties.Resources.TitleNewFeatureClass, Properties.Resources.PromptName, null);
            var schema = _config.Schemas[0];

            var cls = new ClassDefinition(name, string.Empty);
            cls.AddProperty(new DataPropertyDefinition("Id", string.Empty) //NOXLATE
            {
                DataType = DataPropertyType.String,
                Length = 256,
                IsNullable = false
            }, true);

            var rp = new RasterPropertyDefinition("Image", string.Empty) //NOXLATE
            {
                DefaultImageXSize = 800,
                DefaultImageYSize = 800
            };
            cls.AddProperty(rp);

            schema.AddClass(cls);

            var item = CreateDefaultItem(cls, rp);
            _config.AddRasterItem(item);

            _items.Add(item);
             */
        }

        private bool _isUpdating = false;

        private void BeginUpdate()
        {
            lstFeatureClasses.BeginUpdate();
            _isUpdating = true;
        }

        private void EndUpdate()
        {
            lstFeatureClasses.EndUpdate();
            _isUpdating = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            BusyWaitDialog.Run(Strings.PrgBuildingDefaultWmsDocument, () =>
            {
                MakeDefaultDocument();
                return _config;
            }, (res, ex) =>
            {
                if (ex != null)
                {
                    ErrorDialog.Show(ex);
                }
                else
                {
                    try
                    {
                        this.BeginUpdate();
                        _items.Clear();
                        foreach (var item in _config.RasterOverrides)
                        {
                            _items.Add(item);
                        }
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                    grdSpatialContexts.DataSource = _config.SpatialContexts;
                }
            });
        }

        private void MakeDefaultDocument()
        {
            try
            {
                _config = (WmsConfigurationDocument)_service.CurrentConnection.FeatureService.GetSchemaMapping("OSGeo.WMS", _fs.ConnectionString); //NOXLATE
                foreach (var sc in _config.SpatialContexts)
                {
                    if (sc.Name.StartsWith("EPSG:"))
                    {
                        var tokens = sc.Name.Split(':');
                        try
                        {
                            sc.CoordinateSystemWkt = _service.CurrentConnection.CoordinateSystemCatalog.ConvertEpsgCodeToWkt(tokens[1]);
                        }
                        catch
                        {

                        }
                    }
                }
                string defaultScName = _config.GetDefaultSpatialContext(_fs, _service.CurrentConnection);
                _config.EnsureRasterProperties(defaultScName);
                _config.EnsureConsistency();
            }
            catch
            {
                _config = BuildDefaultWmsDocument();
            }
        }

        private void grdSpatialContexts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                string wkt = _service.GetCoordinateSystem();
                if (!string.IsNullOrEmpty(wkt))
                {
                    grdSpatialContexts[e.ColumnIndex, e.RowIndex].Value = wkt;
                }
            }
        }

        private void txtClassName_TextChanged(object sender, EventArgs e)
        {
            if (_updatingLogicalClassUI) return;
            if (_logicalClass == null) return;
            var item = lstFeatureClasses.SelectedItem as RasterWmsItem;
            if (item == null) return;

            _logicalClass.Name = txtClassName.Text;
            item.FeatureClass = _logicalClass.Name;
            lstFeatureClasses.DataSource = _config.RasterOverrides; //rebind
        }

        private void txtClassDescription_TextChanged(object sender, EventArgs e)
        {
            if (_updatingLogicalClassUI) return;
            if (_logicalClass == null) return;
            var item = lstFeatureClasses.SelectedItem as RasterWmsItem;
            if (item == null) return;

            _logicalClass.Description = txtClassDescription.Text;
            item.FeatureClass = _logicalClass.Name;
            lstFeatureClasses.DataSource = _config.RasterOverrides; //rebind
        }

        private void lnkSwap_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_logicalClass == null) return;
            var item = lstFeatureClasses.SelectedItem as RasterWmsItem;
            if (item == null) return;

            try
            {
                _updatingLogicalClassUI = true;
                var tmp = txtClassName.Text;
                txtClassName.Text = txtClassDescription.Text;
                txtClassDescription.Text = tmp;

                _logicalClass.Name = txtClassName.Text;
                _logicalClass.Description = txtClassDescription.Text;
                item.FeatureClass = _logicalClass.Name;
                lstFeatureClasses.DataSource = _config.RasterOverrides; //rebind
            }
            finally
            {
                _updatingLogicalClassUI = false;
            }
        }

        private void btnSwapAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Strings.ConfirmWmsLogicalClassSwap, string.Empty, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                btnSwapAll.Enabled = false;
                try
                {
                    using (new WaitCursor(this))
                    {
                        _logicalClass = null;
                        lstFeatureClasses.SelectedItem = null;
                        foreach (var item in _config.RasterOverrides)
                        {
                            var cls = _config.GetClass(item.SchemaName, item.FeatureClass);
                            if (cls == null)
                                continue;

                            var tmp = cls.Name;
                            cls.Name = cls.Description;
                            cls.Description = tmp;

                            item.FeatureClass = cls.Name;
                        }
                        lstFeatureClasses.DataSource = _config.RasterOverrides; //rebind
                    }
                }
                finally
                {
                    btnSwapAll.Enabled = true;
                }
            }
        }
    }
}