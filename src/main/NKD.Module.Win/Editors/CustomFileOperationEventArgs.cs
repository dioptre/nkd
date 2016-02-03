using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using System.ComponentModel;
namespace NKD.Module.Win.Editors
{

	/// <summary>
	///     <para>Arguments passed to the <see cref="E:DevExpress.ExpressApp.FileAttachments.Win.FileAttachmentsWindowsFormsModule.CustomOpenFileWithDefaultProgram" /> event.
	/// </para>
	/// </summary>
	public class CustomFileOperationEventArgs : HandledEventArgs
	{
		private IFileData fileData;
		/// <summary>
		///     <para>Specifies a file about to be opened via its associated program.
		/// </para>
		/// </summary>
		/// <value>An <b>IFileData</b> object, representing a file about to be opened via its associated program.
		///
		/// </value>
		public IFileData FileData
		{
			get
			{
				return this.fileData;
			}
		}
		/// <summary>
		///     <para>Creates a new instance of the CustomFileOperationEventArgs class.
		/// </para>
		/// </summary>
		/// <param name="fileData">
		/// 		An <b>IFileData</b> object, representing a file about to be opened via its associated program. This parameter value is assigned to the <see cref="P:DevExpress.ExpressApp.FileAttachments.Win.CustomFileOperationEventArgs.FileData" /> property.
		///
		///
		/// </param>
		public CustomFileOperationEventArgs(IFileData fileData)
		{
			this.fileData = fileData;
		}
	}
}
