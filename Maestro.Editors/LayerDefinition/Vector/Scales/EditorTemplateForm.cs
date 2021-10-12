#region Disclaimer / License

// Copyright (C) 2009, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
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

using System;
using System.Windows.Forms;

namespace Maestro.Editors.LayerDefinition.Vector.Scales
{
    internal partial class EditorTemplateForm : Form
    {
        public EditorTemplateForm()
        {
            InitializeComponent();
            this.ManualSizeManagement = false;
        }

        public bool ManualSizeManagement { get; set; }

        private bool GetItemPanelSize(out int height, out int width)
        {
            height = -1;
            width = -1;

            if (ItemPanel.Controls.Count > 0 && ItemPanel.Controls[0] as UserControl != null)
            {
                height = ButtonPanel.Height + ItemPanel.Top + (ItemPanel.Controls[0] as UserControl).AutoScrollMinSize.Height + (8 * 6);
                width = Math.Max(this.Width, (ItemPanel.Controls[0] as UserControl).AutoScrollMinSize.Width + 2 * ItemPanel.Left + (8 * 4));
                return true;
            }
            return false;
        }

        public void RefreshSize()
        {
            if (this.GetItemPanelSize(out var h, out var w))
            {
                this.Height = h;
                this.Width = w;
            }
        }

        private void EditorTemplateForm_Load(object sender, EventArgs e)
        {
            if (!this.ManualSizeManagement)
                RefreshSize();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}