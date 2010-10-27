using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents parsed Indices attribute. See 5.1.3.
  /// </summary>
  class GlyphIndices
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Glyphs"/> class.
    /// </summary>
    public GlyphIndices(string indices)
    {
      this.glyphMapping = GlyphIndicesParser.Parse(indices);
    }

    /// <summary>
    /// Gets the number of GlyphMapping elements.
    /// </summary>
    public int Count
    {
      get { return this.glyphMapping.Length; }
    }

    /// <summary>
    /// Gets the <see cref="PdfSharp.Xps.XpsModel.GlyphIndices.GlyphMapping"/> with the specified idx.
    /// </summary>
    public GlyphMapping this[int idx]
    {
      get { return this.glyphMapping[idx]; }
    }
    GlyphMapping[] glyphMapping;

    public struct GlyphMapping
    {
      /// <summary>
      /// Number of UTF-16 code units that combine to form this cluster. One or more code units may be
      /// specified.
      /// Default value is 1. 
      /// </summary>
      public int ClusterCodeUnitCount;

      /// <summary>
      /// Number of glyph indices that combine to form this cluster. One or more indices may be specified.
      /// Default value is 1. 
      /// </summary>
      public int ClusterGlyphCount;

      /// <summary>
      ///Index of the glyph (16-bit) in the physical font. The entry MAY be empty [M2.72], in which case
      ///the glyph index is determined by looking up the UTF-16 code unit in the font character map table.
      ///If there is not a one-to-one mapping between code units and the glyph indices, this entry MUST
      ///be specified [M5.5]. In cases where character-to-glyph mappings are not one-to-one, a cluster
      ///mapping specification precedes the glyph index (further described below). 
      /// </summary>
      public int GlyphIndex;

      /// <summary>
      /// Advance width indicating placement for the subsequent glyph, relative to the origin of the current
      /// glyph. Measured in direction of advance as defined by the IsSideways and BidiLevel attributes.
      /// Base glyphs generally have a non-zero advance width and combining glyphs have a zero advance
      /// width.
      /// Advance width is measured in hundredths of the font em size. The default value is defined in the
      /// horizontal metrics font table (hmtx) if the IsSideways attribute is specified as false or the vertical
      /// metrics font table (vmtx) if the IsSideways attribute is specified as true. Advance width is a real
      /// number with units specified in hundredths of an em.  
      /// </summary>
      public double AdvanceWidth;

      /// <summary>
      /// Offset in the effective coordinate space relative to glyph origin to move this glyph (x offset for
      /// uOffset and â€“y offset for vOffset. The sign of vOffset is reversed from the direction of the y
      /// axis. A positive vOffset value shifts the glyph by a negative y offset and vice versa.). Used to
      /// attach marks to base characters. The value is added to the nominal glyph origin calculated using
      /// the advance width to generate the actual origin for the glyph. The setting of the IsSideways
      /// attribute does not change the interpretation of uOffset and vOffset.
      /// Measured in hundredths of the font em size. The default offset values are 0.0,0.0. uOffset and
      /// vOffset are real numbers.
      /// Base glyphs generally have a glyph offset of 0.0,0.0. Combining glyphs generally have an offset
      /// that places them correctly on top of the nearest preceding base glyph.
      /// For left-to-right text, a positive uOffset value points to the right; for right-to-left text, a
      /// positive uOffset value points to the left. 
      /// </summary>
      public double uOffset, vOffset;
    }

  }

  /// <summary>
  /// Converts an indices string into a GlyphMapping array.
  /// </summary>
  class GlyphIndicesParser
  {
    public static GlyphIndices.GlyphMapping[] Parse(string indices)
    {
      GlyphIndicesParser parser = new GlyphIndicesParser(indices);
      return parser.Parse();
    }

    GlyphIndicesParser(string indices)
    {
      this.indices = indices;
    }
    string indices;

    GlyphIndices.GlyphMapping[] Parse()
    {
      string[] parts = this.indices.Split(new char[] { ';' });
      int count = parts.Length;
      GlyphIndices.GlyphMapping[] glyphMapping = new GlyphIndices.GlyphMapping[count];

      for (int idx = 0; idx < count; idx++)
        glyphMapping[idx] = ParsePart(parts[idx]);

      return glyphMapping;
    }

    GlyphIndices.GlyphMapping ParsePart(string parts)
    {
      GlyphIndices.GlyphMapping mapping = new GlyphIndices.GlyphMapping();
      mapping.ClusterCodeUnitCount = 1;
      mapping.ClusterGlyphCount = 1;

      string[] commaSeparated = parts.Split(',');
      if (commaSeparated.Length > 0)
      {
        if (commaSeparated[0] != "")
        {
          //Just split the numbers
          string[] tempStr = commaSeparated[0].Split(new char[] { '(', ')', ':' }, StringSplitOptions.RemoveEmptyEntries);
          //Last number is the index
          mapping.GlyphIndex = int.Parse(tempStr[tempStr.Length - 1]);

          //First and second (if available) are the code unit count and glyph count)
          if (tempStr.Length > 1)
            int.TryParse(tempStr[0], out mapping.ClusterCodeUnitCount);
          if (tempStr.Length > 2)
            int.TryParse(tempStr[1], out mapping.ClusterGlyphCount);
        }

        if (commaSeparated.Length > 1)
          double.TryParse(commaSeparated[1], out mapping.AdvanceWidth);

        if (commaSeparated.Length > 2)
          double.TryParse(commaSeparated[2], out mapping.uOffset);

        if (commaSeparated.Length > 3)
          double.TryParse(commaSeparated[3], out mapping.vOffset);
      }

      return mapping;
    }

  }
}