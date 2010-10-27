namespace PdfSharp.Viewing
{
  partial class Acrobat6Viewer
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Acrobat6Viewer));
      this.axPdf6 = new AxPdfLib.AxPdf();
      ((System.ComponentModel.ISupportInitialize)(this.axPdf6)).BeginInit();
      this.SuspendLayout();
      // 
      // axPdf1
      // 
      this.axPdf6.Enabled = true;
      this.axPdf6.Location = new System.Drawing.Point(12, 12);
      this.axPdf6.Name = "axPdf1";
      this.axPdf6.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axPdf1.OcxState")));
      this.axPdf6.Size = new System.Drawing.Size(268, 249);
      this.axPdf6.TabIndex = 0;
      // 
      // Acrobat6Viewer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 273);
      this.Controls.Add(this.axPdf6);
      this.Name = "Acrobat6Viewer";
      this.Text = "Acrobat6Viewer";
      ((System.ComponentModel.ISupportInitialize)(this.axPdf6)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private AxPdfLib.AxPdf axPdf6;
  }
}