namespace mplocker
{
	// MAGIC STARTS HERE ////////////////////////////////////////////
	// GTA SA RenderWare TXD Hex Patch Find
	private static readonly byte[] PatchFind1 = { 0x00, 0x5C, 0x00 };
	private static readonly byte[] PatchFind2 = { 0x00, 0x5C, 0x08 };
	private static readonly byte[] PatchFind3 = { 0x00, 0x5C, 0x10 };
	private static readonly byte[] PatchFind4 = { 0x00, 0x5C, 0x20 };
	private static readonly byte[] PatchFind5 = { 0x00, 0x5C, 0x40 };
	private static readonly byte[] PatchFind6 = { 0x00, 0x5C, 0x80 };
	private static readonly byte[] PatchFind7 = { 0x00, 0x9C, 0x00 };
	// GTA SA RenderWare DFF Hex Patch Find
	private static readonly byte[] PatchFind8 = { 0x00, 0x00, 0xFF, 0xFF, 0x03, 0x18, 0x01, 0x00, 0x00, 0x00, 0x04 };
	private static readonly byte[] PatchFind9 = { 0x00, 0xFF, 0xFF, 0x03, 0x18, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x03, 0x18 };
	private static readonly byte[] PatchFind10 = { 0xFF, 0xFF, 0x7F };
	// GTA SA RenderWare TXD Hex Patch Replace
	private static readonly byte[] PatchReplace1 = { 0x00, 0x5C, 0x01 };
	private static readonly byte[] PatchReplace2 = { 0x00, 0x9C, 0x01 };
	// GTA SA RenderWare DFF Hex Patch Replace
	private static readonly byte[] PatchReplace3 = { 0x00, 0x01, 0xFF, 0xFF, 0x03, 0x18, 0x01, 0x00, 0x00, 0x00, 0x04 };
	private static readonly byte[] PatchReplace4 = { 0x00, 0xFF, 0xFF, 0x03, 0x18, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0x03, 0x18 };
	private static readonly byte[] PatchReplace5 = { 0xDD, 0xDD, 0x7F };
	
	// Detect Patch Functions
	private static bool DetectPatch(byte[] sequence, int position, byte[] patchFind)
	{
		if (position + patchFind.Length > sequence.Length) return false;
		for (int p = 0; p < patchFind.Length; p++)
		{
			if (patchFind[p] != sequence[position + p]) return false;
		}
		return true;
	}

	// Replace Patch Functions
	private void PatchFile(string originalFile, string patchedFile, byte[] patchFind, byte[] patchReplace)
	{
		// Ensure target directory exists.
		var targetDirectory = Path.GetDirectoryName(patchedFile);
		if (targetDirectory == null) return;
		Directory.CreateDirectory(targetDirectory);

		// Read file bytes.
		byte[] fileContent = File.ReadAllBytes(originalFile);

		// Detect and patch file.
		for (int p = 0; p < fileContent.Length; p++)
		{
			if (!DetectPatch(fileContent, p, patchFind)) continue;

			for (int w = 0; w < patchFind.Length; w++)
			{
				fileContent[p + w] = patchReplace[w];
			}
		}

		// Save it to another location.
		File.WriteAllBytes(patchedFile, fileContent);
	}
	// OPEN FILE DIALOGS //////////////////////////////////////////
	string gameFile;

	private void bSelectFile_Click(object sender, EventArgs e)
	{
		if (!bgWork.IsBusy)
		{
			OpenFileDialog op = new OpenFileDialog();
			op.Filter = "SA - RenderWare (.dff, .txd) |*.txd; *.dff";

			if (op.ShowDialog() == DialogResult.OK)
			{
				lbFileName.Text = op.SafeFileName;
				gameFile = op.FileName;
			}
		}
	}

	// MAGIC ALSO STARTS HERE /////////////////////////////////////
	private void bgWork_DoWork(object sender, DoWorkEventArgs e)
	{
		for (int i = 0; i <= 100; i++)
		{
			Thread.Sleep(70);
			bgWork.ReportProgress(i);
		}

		if (gameFile.EndsWith(".dff"))
		{
			System.IO.File.Copy(gameFile, Path.Combine(Path.GetDirectoryName(gameFile), Path.GetFileNameWithoutExtension(gameFile) + ".dff.bak"), true);

			PatchFile(gameFile, gameFile, PatchFind8, PatchReplace3);
			PatchFile(gameFile, gameFile, PatchFind9, PatchReplace4);
			BinaryWriter lockdff = new BinaryWriter(File.OpenWrite(gameFile));
			{
				lockdff.BaseStream.Position = 0x07;
				lockdff.Write(0x01);
				for (int i = 0x51; i <= 0x54; i++)
				{
					lockdff.BaseStream.Position = i;
					lockdff.Write(0x01);
					for (int k = 0x60; k <= 0x64; k++)
					{
						lockdff.BaseStream.Position = k;
						lockdff.Write(0x01);
						for (int j = 0x2A; j <= 0x2B; j++)
						{
							lockdff.BaseStream.Position = j;
							lockdff.Write(0x01);
						}
					}
				}
				lockdff.Close();
			}
		}

		if (gameFile.EndsWith(".txd"))
		{
			System.IO.File.Copy(gameFile, Path.Combine(Path.GetDirectoryName(gameFile), Path.GetFileNameWithoutExtension(gameFile) + ".txd.bak"), true);
			PatchFile(gameFile, gameFile, PatchFind1, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind2, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind3, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind4, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind5, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind6, PatchReplace1);
			PatchFile(gameFile, gameFile, PatchFind7, PatchReplace2);
		}

	}	
}
