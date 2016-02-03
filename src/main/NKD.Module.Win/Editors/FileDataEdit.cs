using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NKD.Module.Win.Editors
{
	[ToolboxItem(false)]
	public class FileDataEdit : ButtonEdit
	{
		private ControlCursorHelper cursorHelper;
		public override string EditorTypeName
		{
			get
			{
				return RepositoryFileEditor.EditorName;
			}
		}
		public IFileData FileData
		{
			get
			{
				return this.EditValue as IFileData;
			}
		}
		public new RepositoryFileEditor Properties
		{
			get
			{
				return (RepositoryFileEditor)base.Properties;
			}
		}
		static FileDataEdit()
		{
			RepositoryFileEditor.Register();
		}
		private void OnClearMenuItemSelected(object sender, EventArgs e)
		{
			this.ClearFileData();
		}
		private void OnSaveMenuItemSelected(object sender, EventArgs e)
		{
			this.Properties.Save(this.FileData);
			this.UpdateEditValue();
		}
		private void OnOpenMenuItemSelected(object sender, EventArgs e)
		{
			this.Properties.Open(this.FileData);
			this.UpdateEditValue();
		}
		private void UpdateDeleteMenuItem(DXMenuItem item, EventArgs e)
		{
			item.Visible = false;
		}
		private void UpdateClearMenuItem(DXMenuItem item, EventArgs e)
		{
			item.Enabled = (!this.Properties.FileDataReadOnly && !FileDataHelper.IsFileDataEmpty(this.FileData));
		}
		private void UpdateSaveOpenMenuItem(DXMenuItem item, EventArgs e)
		{
			item.Enabled = !FileDataHelper.IsFileDataEmpty(this.FileData);
		}
		private void UpdateEditValue()
		{
			this.UpdateDisplayText();
		}
		private void DropFile(string[] fileNames)
		{
			this.FileSelected(fileNames[0]);
		}
		private void buttonPressed(object sender, ButtonPressedEventArgs e)
		{
			this.ShowFileOpenDialog();
		}
		private void MaskBox_MouseLeave(object sender, EventArgs e)
		{
			this.cursorHelper.Restore();
		}
		private void MaskBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.MaskBox != null)
			{
				float width = base.CreateGraphics().MeasureString(this.MaskBox.MaskBoxText, base.ViewInfo.Appearance.Font).Width;
				if ((float)e.X < width && e.Button == MouseButtons.None)
				{
					this.cursorHelper.ChangeControlCursor(Cursors.Hand);
					return;
				}
				this.cursorHelper.Restore();
			}
		}
		protected override void OnClick(EventArgs e)
		{
			if (this.MaskBox != null && this.MaskBox.Cursor == Cursors.Hand)
			{
				this.OnOpenMenuItemSelected(this, e);
			}
			base.OnClick(e);
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Return || (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Down))
			{
				this.ShowFileOpenDialog();
			}
		}
		protected override void OnEditorKeyDown(KeyEventArgs e)
		{
			base.OnEditorKeyDown(e);
			if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
			{
				this.ClearFileData();
			}
		}
		protected override void OnClickButton(EditorButtonObjectInfoArgs buttonInfo)
		{
			base.OnClickButton(buttonInfo);
			if (buttonInfo.BuiltIn)
			{
				this.ShowFileOpenDialog();
			}
		}
		protected override DXPopupMenu CreateMenu()
		{
			DXPopupMenu dXPopupMenu = base.CreateMenu();
			string localizedString = Localizer.Active.GetLocalizedString(StringId.TextEditMenuDelete);
			foreach (DXMenuItem dXMenuItem in dXPopupMenu.Items)
			{
				if (dXMenuItem is TextEdit.DXMenuItemTextEdit && dXMenuItem.Caption == localizedString)
				{
					TextEdit.DXMenuItemTextEdit dXMenuItemTextEdit = (TextEdit.DXMenuItemTextEdit)dXMenuItem;
					dXMenuItemTextEdit.UpdateElement = new TextEdit.MenuItemUpdateElement(dXMenuItemTextEdit, new TextEdit.MenuItemUpdateHandler(this.UpdateDeleteMenuItem));
				}
			}
			Image image = ImageLoader.Instance.GetImageInfo("MenuBar_Open").Image;
			TextEdit.DXMenuItemTextEdit dXMenuItemTextEdit2 = new TextEdit.DXMenuItemTextEdit(StringId.PictureEditOpenFileTitle, new EventHandler(this.OnOpenMenuItemSelected), image);
			dXMenuItemTextEdit2.Caption = CaptionHelper.GetLocalizedText("FileAttachments", "Editor_Open");
			dXMenuItemTextEdit2.UpdateElement = new TextEdit.MenuItemUpdateElement(dXMenuItemTextEdit2, new TextEdit.MenuItemUpdateHandler(this.UpdateSaveOpenMenuItem));
			dXMenuItemTextEdit2.BeginGroup = true;
			dXPopupMenu.Items.Add(dXMenuItemTextEdit2);
			image = ImageLoader.Instance.GetImageInfo("MenuBar_SaveTo").Image;
			dXMenuItemTextEdit2 = new TextEdit.DXMenuItemTextEdit(StringId.PictureEditSaveFileTitle, new EventHandler(this.OnSaveMenuItemSelected), image);
			dXMenuItemTextEdit2.Caption = CaptionHelper.GetLocalizedText("FileAttachments", "Editor_Save");
			dXMenuItemTextEdit2.UpdateElement = new TextEdit.MenuItemUpdateElement(dXMenuItemTextEdit2, new TextEdit.MenuItemUpdateHandler(this.UpdateSaveOpenMenuItem));
			dXPopupMenu.Items.Add(dXMenuItemTextEdit2);
			image = ImageLoader.Instance.GetImageInfo("MenuBar_Clear").Image;
			dXMenuItemTextEdit2 = new TextEdit.DXMenuItemTextEdit(StringId.DateEditClear, new EventHandler(this.OnClearMenuItemSelected), image);
			dXMenuItemTextEdit2.Caption = CaptionHelper.GetLocalizedText("FileAttachments", "Editor_Clear");
			dXMenuItemTextEdit2.UpdateElement = new TextEdit.MenuItemUpdateElement(dXMenuItemTextEdit2, new TextEdit.MenuItemUpdateHandler(this.UpdateClearMenuItem));
			dXPopupMenu.Items.Add(dXMenuItemTextEdit2);
			return dXPopupMenu;
		}
		protected override void OnDragDrop(DragEventArgs e)
		{
			if (e.Effect == DragDropEffects.Copy)
			{
				this.DropFile((string[])e.Data.GetData(DataFormats.FileDrop));
				return;
			}
			base.OnDragDrop(e);
		}
		protected override void UpdateMaskBoxProperties(bool always)
		{
			base.UpdateMaskBoxProperties(always);
			this.MaskBox.ReadOnly = true;
		}
		protected override void OnDragOver(DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;
			if (!this.Properties.FileDataReadOnly && e.Data.GetDataPresent(DataFormats.FileDrop) && ((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1)
			{
				e.Effect = DragDropEffects.Copy;
			}
		}
		protected virtual IFileData OnCreateCustomFileDataObject()
		{
			return this.Properties.CreateFileDataObject();
		}
		protected virtual void ShowFileOpenDialog()
		{
			if (!this.Properties.FileDataReadOnly)
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.CheckFileExists = true;
				openFileDialog.CheckPathExists = true;
				openFileDialog.DereferenceLinks = true;
				openFileDialog.Multiselect = false;
				openFileDialog.Filter = this.Properties.FileTypesFilter;
				if (openFileDialog.ShowDialog(Form.ActiveForm) == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
				{
					this.FileSelected(openFileDialog.FileName);
				}
			}
		}
		public FileDataEdit()
		{
			this.AllowDrop = true;
			this.MaskBox.MouseMove += new MouseEventHandler(this.MaskBox_MouseMove);
			this.MaskBox.MouseLeave += new EventHandler(this.MaskBox_MouseLeave);
			this.MaskBox.DoubleClick += new EventHandler(this.MaskBox_DoubleClick);
			this.cursorHelper = new ControlCursorHelper(this.MaskBox);
		}
		private void MaskBox_DoubleClick(object sender, EventArgs e)
		{
			if (this.MaskBox != null)
			{
				float width = base.CreateGraphics().MeasureString(this.MaskBox.MaskBoxText, base.ViewInfo.Appearance.Font).Width;
				if (width < (float)((MouseEventArgs)e).X)
				{
					this.ShowFileOpenDialog();
					return;
				}
			}
			else
			{
				this.ShowFileOpenDialog();
			}
		}
		public void ClearFileData()
		{
			if (this.FileData != null)
			{
				this.FileData.Clear();
				this.UpdateEditValue();
				this.IsModified = true;
			}
		}
		public void FileSelected(string fileName)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				base.Focus();
				if (base.ContainsFocus)
				{
					using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						if (this.FileData == null)
						{
							this.EditValue = this.OnCreateCustomFileDataObject();
							this.IsModified = true;
							this.DoValidate();
						}
						if (this.FileData != null)
						{
							FileDataHelper.LoadFromStream(this.FileData, Path.GetFileName(fileName), fileStream, fileName);
							this.UpdateEditValue();
							this.IsModified = true;
						}
					}
				}
			}
		}
	}
}
