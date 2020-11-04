namespace Pk3DSRNGTool
{
    partial class CitraViewForm
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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.B_UpdatePokemons = new System.Windows.Forms.Button();
            this.B_UseParent2 = new System.Windows.Forms.Button();
            this.B_UseParent1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // B_UpdatePokemons
            // 
            this.B_UpdatePokemons.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.B_UpdatePokemons.Location = new System.Drawing.Point(12, 646);
            this.B_UpdatePokemons.Name = "B_UpdatePokemons";
            this.B_UpdatePokemons.Size = new System.Drawing.Size(1101, 23);
            this.B_UpdatePokemons.TabIndex = 64;
            this.B_UpdatePokemons.Text = "Update all Pokémon slots";
            this.B_UpdatePokemons.UseVisualStyleBackColor = true;
            this.B_UpdatePokemons.Click += new System.EventHandler(this.B_UpdatePokemons_Click);
            // 
            // B_UseParent2
            // 
            this.B_UseParent2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.B_UseParent2.Location = new System.Drawing.Point(934, 617);
            this.B_UseParent2.Name = "B_UseParent2";
            this.B_UseParent2.Size = new System.Drawing.Size(178, 23);
            this.B_UseParent2.TabIndex = 65;
            this.B_UseParent2.Text = "Use parent 2";
            this.B_UseParent2.UseVisualStyleBackColor = true;
            this.B_UseParent2.Click += new System.EventHandler(this.B_UseParent2_Click);
            // 
            // B_UseParent1
            // 
            this.B_UseParent1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.B_UseParent1.Location = new System.Drawing.Point(750, 617);
            this.B_UseParent1.Name = "B_UseParent1";
            this.B_UseParent1.Size = new System.Drawing.Size(178, 23);
            this.B_UseParent1.TabIndex = 66;
            this.B_UseParent1.Text = "Use parent 1";
            this.B_UseParent1.UseVisualStyleBackColor = true;
            this.B_UseParent1.Click += new System.EventHandler(this.B_UseParent1_Click);
            // 
            // CitraViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 681);
            this.Controls.Add(this.B_UseParent1);
            this.Controls.Add(this.B_UseParent2);
            this.Controls.Add(this.B_UpdatePokemons);
            this.Name = "CitraViewForm";
            this.Text = "CitraViewForm";
            this.ResumeLayout(false);

        }

        #endregion
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.Button B_UpdatePokemons;
        private System.Windows.Forms.Button B_UseParent2;
        private System.Windows.Forms.Button B_UseParent1;
    }
}