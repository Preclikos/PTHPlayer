﻿using System.Collections.Generic;

namespace PTHPlayer.Subtitles.Decoder
{
    public class RegionCompositionSegment
    {
        public int RegionId { get; set; }
        public int RegionVersionNumber { get; set; }
        public bool RegionFillFlag { get; set; }
        public int RegionWidth { get; set; }
        public int RegionHeight { get; set; }
        public int RegionLevelOfCompatibility { get; set; }
        public int RegionDepth { get; set; }
        public int RegionClutId { get; set; }
        public int Region8BitPixelCode { get; set; }
        public int Region4BitPixelCode { get; set; }
        public int Region2BitPixelCode { get; set; }

        public List<RegionCompositionSegmentObject> Objects = new List<RegionCompositionSegmentObject>();

        public RegionCompositionSegment(byte[] buffer, int index, int regionLength)
        {
            RegionId = buffer[index];
            RegionVersionNumber = buffer[index + 1] >> 4;
            RegionFillFlag = (buffer[index + 1] & 0b00001000) > 0;
            RegionWidth = Helper.GetEndianWord(buffer, index + 2);
            RegionHeight = Helper.GetEndianWord(buffer, index + 4);
            RegionLevelOfCompatibility = buffer[index + 6] >> 5;
            RegionDepth = (buffer[index + 6] & 0b00011100) >> 2;
            RegionClutId = buffer[index + 7];
            Region8BitPixelCode = buffer[index + 8];
            Region4BitPixelCode = buffer[index + 9] >> 4;
            Region2BitPixelCode = (buffer[index + 9] & 0b00001100) >> 2;
            int i = 0;
            while (i < regionLength && i + index < buffer.Length)
            {
                var regionCompositionSegmentObject = new RegionCompositionSegmentObject { ObjectId = Helper.GetEndianWord(buffer, i + index + 10) };
                i += 2;
                regionCompositionSegmentObject.ObjectType = buffer[i + index + 10] >> 6;
                regionCompositionSegmentObject.ObjectProviderFlag = (buffer[index + i + 10] & 0b00110000) >> 4;
                regionCompositionSegmentObject.ObjectHorizontalPosition = ((buffer[index + i + 10] & 0b00001111) << 8) + buffer[index + i + 11];
                i += 2;
                regionCompositionSegmentObject.ObjectVerticalPosition = ((buffer[index + i + 10] & 0b00001111) << 8) + buffer[index + i + 11];
                i += 2;
                if (regionCompositionSegmentObject.ObjectType == 1 || regionCompositionSegmentObject.ObjectType == 2)
                {
                    i += 2;
                }

                Objects.Add(regionCompositionSegmentObject);
            }
        }

        public string RegionLevelOfCompatibilityString
        {
            get
            {

                switch (this.RegionLevelOfCompatibility)
                {
                    case 0:
                        return "reserved";
                    case 1:
                        return "2-bit/entry CLUT required";
                    case 2:
                        return "4-bit/entry CLUT required";
                    case 3:
                        return "8-bit/entry CLUT required";
                    default:
                        return "reserved or unknown";
                }

            }
        }

        public string RegionDepthString
        {
            get
            {
                switch (this.RegionDepth)
                {
                    case 0:
                        return "reserved";
                    case 1:
                        return "2 bit";
                    case 2:
                        return "4 bit";
                    case 3:
                        return "8 bit";
                    default:
                        return "reserved or unknown";
                }
            }
        }
    }
}