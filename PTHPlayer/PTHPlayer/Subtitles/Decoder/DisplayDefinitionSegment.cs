﻿namespace PTHPlayer.Subtitles.Decoder
{
    public class DisplayDefinitionSegment
    {
        public int DisplayDefinitionVersionNumber { get; set; }
        public bool DisplayWindowFlag { get; set; }
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }
        public int? DisplayWindowHorizontalPositionMinimum { get; set; }
        public int? DisplayWindowHorizontalPositionMaximum { get; set; }
        public int? DisplayWindowVerticalPositionMinimum { get; set; }
        public int? DisplayWindowVerticalPositionMaximum { get; set; }

        public DisplayDefinitionSegment(byte[] buffer, int index)
        {
            DisplayDefinitionVersionNumber = buffer[index] >> 4;
            DisplayWindowFlag = (buffer[index] & 0b00001000) > 0;
            DisplayWidth = Helper.GetEndianWord(buffer, index + 1);
            DisplayHeight = Helper.GetEndianWord(buffer, index + 3);
            if (DisplayWindowFlag)
            {
                DisplayWindowHorizontalPositionMinimum = Helper.GetEndianWord(buffer, index + 5);
                DisplayWindowHorizontalPositionMaximum = Helper.GetEndianWord(buffer, index + 7);
                DisplayWindowVerticalPositionMinimum = Helper.GetEndianWord(buffer, index + 9);
                DisplayWindowVerticalPositionMaximum = Helper.GetEndianWord(buffer, index + 11);
            }
        }
    }
}